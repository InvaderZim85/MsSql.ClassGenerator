using System.Reflection.Emit;
using System.Text;
using MsSql.ClassGenerator.Core.Common;
using MsSql.ClassGenerator.Core.Model;
using Serilog;

namespace MsSql.ClassGenerator.Core.Business;

/// <summary>
/// Provides the functions for the class generation.
/// </summary>
public partial class ClassManager
{
    /// <summary>
    /// Occurs when progress was made.
    /// </summary>
    public event EventHandler<string>? ProgressEvent;

    /// <summary>
    /// Gets the EF Key code.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: The code is only generated when the option <see cref="ClassGeneratorOptions.DbModel"/> is set to <see langword="true"/>.
    /// </remarks>
    public EfKeyCodeResult EfKeyCode { get; private set; } = new();

    /// <summary>
    /// Generates the classes out of the specified tables according to the specified options.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="tables">The list with the tables.</param>
    /// <returns>The awaitable task.</returns>
    /// <exception cref="DirectoryNotFoundException">Will be thrown when the specified output directory doesn't exist.</exception>
    public async Task GenerateClassAsync(ClassGeneratorOptions options, List<TableEntry> tables)
    {
        EfKeyCode = new EfKeyCodeResult();

        // Step 0: Check the options.
        if (!Directory.Exists(options.Output))
            throw new DirectoryNotFoundException($"The specified output ({options.Output}) folder doesn't exist.");

        if (options.EmptyOutputDirectoryBeforeExport)
            CleanDirectory(options.Output);

        // Step 1: Load the type conversion information (needed for the class generator).
        await LoadTypeConversionDataAsync();

        // Step 2: Load the tables which should be exported
        foreach (var table in tables)
        {
            ProgressEvent?.Invoke(this, $"Generate class for table '{table.Name}'...");
            Log.Debug("Generate class for table '{name}'...", table.Name);
            await GenerateClassAsync(options, table);
        }

        // Generate the EF Key code
        if (options.DbModel)
            GenerateEfKeyCode(tables);
    }

    /// <summary>
    /// Generates the class for the desired table.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="table">The table.</param>
    /// <returns>The awaitable task.</returns>
    private async Task GenerateClassAsync(ClassGeneratorOptions options, TableEntry table)
    {
        // The list with the replacement values
        // Note: Before replacing, the keys are converted accordingly (capitalised with a leading and trailing dollar sign).
        var replaceList = new List<ReplacementDto>();

        // Load the needed template
        var template = await GetClassTemplateAsync(options);

        // Load the property template
        var propertyTemplate = await GetPropertyTemplateAsync(options);

        // Generate the properties
        var properties = table.Columns
            .OrderBy(o => o.Order)
            .Select(s => GeneratePropertyCode(options, propertyTemplate, s))
            .ToList();

        replaceList.Add(new ReplacementDto("properties", string.Join(Environment.NewLine, properties), true));

        // Set the name space
        replaceList.Add(new ReplacementDto("namespace", GenerateNamespace(options.Namespace)));

        // Set the modifier
        replaceList.Add(new ReplacementDto("modifier", options.Modifier));

        // Set the sealed value
        replaceList.Add(new ReplacementDto("sealed", options.SealedClass ? " sealed" : string.Empty));

        // Set the class name
        var className = GenerateClassName(table.ClassName);
        replaceList.Add(new ReplacementDto("name", className));

        // Set the "inherits" value of the community toolkit.
        var inheritValue = string.Empty;
        if (options.AddSetProperty)
        {
            template.Insert(0, $"using CommunityToolkit.Mvvm.ComponentModel;");
            inheritValue = ": ObservableObject";
        }

        replaceList.Add(new ReplacementDto("inherits", inheritValue));

        // Get the attributes
        replaceList.Add(new ReplacementDto("attributes", GetClassAttributes(options, table)));

        // Finalize the template and inject the values.
        var result = FinalizeTemplate(template, replaceList);

        // Save the generated class.
        var path = Path.Combine(options.Output, $"{className}.cs");
        await File.WriteAllTextAsync(path, result);
    }

    /// <summary>
    /// Gets the class attributes.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="table">The table.</param>
    /// <returns>The class attributes.</returns>
    private static string GetClassAttributes(ClassGeneratorOptions options, TableEntry table)
    {
        // Create the attributes
        var attributes = new SortedList<int, string>();

        if (options.AddTableNameToClassSummary)
        {
            attributes.Add(1, "/// <remarks>");
            attributes.Add(2, $"/// Table <c>[{table.Schema}].[{table.Name}]</c>");
            attributes.Add(3, "/// </remarks>");
        }

        if (options.DbModel)
        {
            attributes.Add(4, string.IsNullOrWhiteSpace(table.Schema)
                ? $"[Table(\"{table.Name}\")]"
                : $"[Table(\"{table.Name}\", Schema = \"{table.Schema}\")]");
        }

        return string.Join(Environment.NewLine, attributes.OrderBy(o => o.Key).Select(s => s.Value));
    }

    /// <summary>
    /// Generates the code for a property.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="template">The property template.</param>
    /// <param name="column">The column.</param>
    /// <returns>The property content.</returns>
    private string GeneratePropertyCode(ClassGeneratorOptions options, List<string> template, ColumnEntry column)
    {
        // The list with the replacement values
        // Note: Before replacing, the keys are converted accordingly (capitalised with a leading and trailing dollar sign).
        var replaceList = new List<ReplacementDto>();

        // Get the C# type
        var dataType = GetCSharpType(column.DataType);

        // Prepare the template
        var tmpTemplate = template.ToList(); // We need a "copy" of the original template.
        PrepareTemplate(tmpTemplate, dataType);

        // Set the type
        replaceList.Add(new ReplacementDto("type", dataType));

        // Set the nullable value
        replaceList.Add(new ReplacementDto("nullable", column.IsNullable ? "?" : ""));

        var tmpName = GeneratePropertyName(column.PropertyName);
        // Set the name of the property
        replaceList.Add(new ReplacementDto("name", tmpName));

        // Add the backing field name
        replaceList.Add(new ReplacementDto("name2", $"_{tmpName.FirstCharToLower()}"));

        // Get the attributes
        replaceList.Add(new ReplacementDto("attributes", GetPropertyAttributes(options, column, dataType)));

        // Finalize the template and inject the values.
        return FinalizeTemplate(tmpTemplate, replaceList);
    }

    /// <summary>
    /// Prepares the template to add the initialization of a string (<c>= string.Empty;</c>).
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="dataType">The data type.</param>
    private static void PrepareTemplate(List<string> template, string dataType)
    {
        const string variablePlaceholder = "$NAME2$";
        const string propertyPlaceholder = "$NAME$";

        if (!dataType.Equals("string", StringComparison.InvariantCultureIgnoreCase))
            return;

        if (template.Any(a => a.Contains(variablePlaceholder)))
        {
            var index = GetLineIndex(template, variablePlaceholder);
            if (index == -1)
                return;

            template[index] = template[index].Replace(";", " = string.Empty;");
        }
        else if (template.Any(a => a.Contains(propertyPlaceholder)))
        {
            var index = GetLineIndex(template, propertyPlaceholder);
            if (index == -1)
                return;

            template[index] += " = string.Empty;";
        }

        return;

        static int GetLineIndex(IReadOnlyList<string> lines, string value)
        {
            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains(value))
                    return i;
            }

            return -1;
        }
    }

    /// <summary>
    /// Gets the attributes.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="column">The column.</param>
    /// <param name="dataType">The data type.</param>
    /// <returns>The attributes.</returns>
    private static string GetPropertyAttributes(ClassGeneratorOptions options, ColumnEntry column, string dataType)
    {
        // Create the attributes
        var attributes = new SortedList<int, string>();

        // Check if the attributes should be added
        if (options is { DbModel: false, AddColumnAttribute: false }) 
            return string.Empty;

        if (options.DbModel && column.IsPrimaryKey)
            attributes.Add(1, "[Key]");
        if (options.AddColumnAttribute ||
            (!string.IsNullOrWhiteSpace(column.Alias) && !column.Alias.Equals(column.Name)))
            attributes.Add(2, $"[Column(\"{column.Name}\")]");

        if (column.DataType.Equals("date", StringComparison.InvariantCultureIgnoreCase))
            attributes.Add(3, "[DataType(DataType.Date)]");

        if (dataType.Equals("string"))
        {
            attributes.Add(4, column.MaxLength == -1 // -1 indicates NVARCHAR(MAX)
                ? "[MaxLength(int.MaxValue)]"
                : $"[MaxLength({column.MaxLength})]");
        }

        return string.Join(Environment.NewLine, attributes.OrderBy(o => o.Key).Select(s => s.Value));
    }

    /// <summary>
    /// Generates the EF Key code.
    /// </summary>
    /// <param name="tables">The list with the tables.</param>
    private void GenerateEfKeyCode(List<TableEntry> tables)
    {
        // Get all tables which contains more than one key column
        var tmpTables = tables.Where(w => w.Columns.Count(c => c.IsPrimaryKey) > 1).ToList();
        if (tmpTables.Count == 0)
            return;

        var sb = PrepareStringBuilder();
        var count = 1;
        foreach (var tableEntry in tmpTables)
        {
            // Add the entity
            sb.AppendLine($"{Tab}modelBuilder.Entity<{tableEntry.ClassName}>().HasKey(k => new")
                .AppendLine($"{Tab}{{");

            // Get the key columns
            var columnCount = 1;
            var columns = tableEntry.Columns.Where(w => w.IsPrimaryKey).ToList();
            foreach (var columnEntry in columns)
            {
                var comma = columnCount++ != columns.Count ? "," : string.Empty;

                sb.AppendLine($"{Tab}{Tab}k.{columnEntry.PropertyName}{comma}");
            }

            // Add the closing brackets
            sb.AppendLine($"{Tab}}});");

            if (count++ != tmpTables.Count)
                sb.AppendLine(); // Spacer

        }

        EfKeyCode = new EfKeyCodeResult
        {
            Code = FinalizeStringBuilder(),
            TableCount = tmpTables.Count
        };

        return;

        StringBuilder PrepareStringBuilder()
        {
            var stringBuilder = new StringBuilder()
                .AppendLine("/// <inheritdoc />")
                .AppendLine("protected override void OnModelCreating(ModelBuilder modelBuilder)")
                .AppendLine("{");

            return stringBuilder;
        }

        // Adds the final code
        string FinalizeStringBuilder()
        {
            sb.AppendLine("}");

            return sb.ToString();
        }
    }

    
}