using System.Dynamic;
using MsSql.ClassGenerator.Core.Common;
using MsSql.ClassGenerator.Core.Model;
using System.Text;

namespace MsSql.ClassGenerator.Core.Business;

/// <summary>
/// Provides the functions for the class generation.
/// </summary>
public partial class ClassManager
{
    public async Task GenerateClassAsync(ClassGeneratorOptions options, List<TableEntry> tables)
    {
        // Step 1: Load the type conversion information (needed for the class generator).
        await LoadTypeConversionDataAsync();

        // Step 2: Load the tables which should be exported
        foreach (var table in tables)
        {
            await GenerateClassAsync(options, table);
        }
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
        var replaceList = new SortedList<string, string>();

        // Load the needed template
        var template = await GetClassTemplateAsync(options);

        // Load the property template
        var propertyTemplate = await GetPropertyTemplateAsync(options);

        // Generate the properties
        var properties = table.Columns
            .OrderBy(o => o.Order)
            .Select(s => GeneratePropertyCode(options, propertyTemplate, s))
            .ToList();

        replaceList.Add("properties", string.Join(Environment.NewLine, properties));

        // Set the name space
        replaceList.Add("namespace", CleanNamespace(options.Namespace));

        // Set the modifier
        replaceList.Add("modifier", options.Modifier);

        // Set the sealed value
        replaceList.Add("sealed", options.SealedClass ? " sealed" : string.Empty);

        // Set the class name
        replaceList.Add("name", table.ClassName); // TODO: Class name muss noch gecheckt werden! Wegen zahlen, leerzeichen, etc.

        // Set the "inherits" value of the community toolkit.
        var inheritValue = string.Empty;
        if (options.AddSetProperty)
        {
            template = template.Insert(0, $"using CommunityToolkit.Mvvm.ComponentModel;{Environment.NewLine}");
            inheritValue = ": ObservableObject";
        }

        replaceList.Add("inherits", inheritValue);

        // Get the attributes
        replaceList.Add("attributes", GetClassAttributes(options, table));

        // Replace the values
        var result = ReplaceValues(template, replaceList);

        await File.WriteAllTextAsync(@"C:\Users\Anwender\Desktop\TestClass.cs", result);
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
            attributes.Add(2, $"/// Table <c>{table.Name}</c>");
            attributes.Add(3, "/// </summary>");
        }

        if (options.DbModel && !table.Name.Equals(table.ClassName))
        {
            attributes.Add(4, string.IsNullOrWhiteSpace(table.Schema)
                ? $"[Table(\"{table.Name}\")]"
                : $"[Table(\"{table.Name}\", Schema = \"{table.Schema}\"]");
        }

        
        var sb = new StringBuilder();

        foreach (var attribute in attributes.OrderBy(o => o.Key))
        {
            sb.AppendLine(attribute.Value);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates the code for a property.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="template">The property template.</param>
    /// <param name="column">The column.</param>
    /// <returns>The property content.</returns>
    private string GeneratePropertyCode(ClassGeneratorOptions options, string template, ColumnEntry column)
    {
        // The list with the replacement values
        // Note: Before replacing, the keys are converted accordingly (capitalised with a leading and trailing dollar sign).
        var replaceList = new SortedList<string, string>();

        // Get the C# type
        var dataType = GetCSharpType(column.DataType);

        // Set the type
        replaceList.Add("type", dataType);

        // Set the nullable value
        replaceList.Add("nullable", column.IsNullable ? "?" : "");

        // Set the name of the property
        replaceList.Add("name", column.PropertyName);

        // Add the backing field name
        replaceList.Add("name2", $"_{column.PropertyName.FirstCharToLower()}");

        // Get the attributes
        replaceList.Add("attributes", GetPropertyAttributes(options, column, dataType));

        // Replace the values
        var result = ReplaceValues(template, replaceList);

        return CleanContent(result, Tab);
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

        var sb = new StringBuilder();

        foreach (var attribute in attributes.OrderBy(o => o.Key))
        {
            sb.AppendLine(attribute.Value);
        }

        return sb.ToString();
    }
}