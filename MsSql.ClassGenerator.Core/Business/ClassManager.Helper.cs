using MsSql.ClassGenerator.Core.Common;
using MsSql.ClassGenerator.Core.Common.Enums;
using MsSql.ClassGenerator.Core.Model;
using Serilog;
using System.Text;

namespace MsSql.ClassGenerator.Core.Business;

/// <inheritdoc cref="ClassManager"/>
public sealed partial class ClassManager
{
    /// <summary>
    /// The name of the default class modifier.
    /// </summary>
    private const string ModifierFallback = "public";

    /// <summary>
    /// Contains the tab indent.
    /// </summary>
    private static readonly string Tab = new(' ', 4);

    /// <summary>
    /// Gets the path of the <i>Files</i> directory.
    /// </summary>
    private static string FileDir => Path.Combine(AppContext.BaseDirectory, "Files");

    /// <summary>
    /// The list with the type conversion data.
    /// </summary>
    /// <remarks>
    /// <i>Info</i>: To load the data, call the function <see cref="LoadTypeConversionDataAsync"/>.
    /// </remarks>
    private List<TypeConversionDto> _typeConversionList = [];

    /// <summary>
    /// Contains the templates.
    /// </summary>
    private readonly List<TemplateDto> _templates  = [];

    /// <summary>
    /// Loads the type conversion data and stores the data into <see cref="_typeConversionList"/>.
    /// </summary>
    /// <returns>The awaitable task.</returns>
    private async Task LoadTypeConversionDataAsync()
    {
        if (_typeConversionList.Count > 0)
            return;

        var path = Path.Combine(FileDir, "TypeConversion.json");
        if (!File.Exists(path))
        {
            Log.Error("The type conversion file is missing. Path: {path}", path);
            return;
        }

        _typeConversionList = await Helper.LoadJsonAsync<List<TypeConversionDto>>(path);
    }

    #region Templates

    /// <summary>
    /// Loads the templates and stores them into <see cref="_templates"/>.
    /// </summary>
    /// <returns>The awaitable task.</returns>
    private async Task LoadTemplatesAsync()
    {
        if (_templates.Count > 0)
            return;

        _templates.Clear();

        // Load all template files (file extension "cgt").
        var templateFiles = new DirectoryInfo(FileDir).GetFiles("*.cgt");

        foreach (var templateType in Enum.GetValues<TemplateType>())
        {
            _templates.Add(await ExtractTemplateAsync(templateType, templateFiles));
        }
    }

    /// <summary>
    /// Extracts the content of the desired template.
    /// </summary>
    /// <param name="type">The desired type.</param>
    /// <param name="templateFiles">The list with the template files.</param>
    /// <returns>The template.</returns>
    private static async Task<TemplateDto> ExtractTemplateAsync(TemplateType type, FileInfo[] templateFiles)
    {
        // Get the desired file
        var templateFile = templateFiles.FirstOrDefault(f =>
            f.Name.StartsWith(type.ToString(), StringComparison.InvariantCultureIgnoreCase));

        // Check if the file exists
        if (templateFile == null)
        {
            Log.Error("The template for '{type}' is missing.", type);
            return new TemplateDto(type, string.Empty);
        }

        // Load the content of the file
        var content = await File.ReadAllTextAsync(templateFile.FullName);

        return new TemplateDto(type, content);
    }

    /// <summary>
    /// Gets the desired template.
    /// </summary>
    /// <param name="options">The options of the class generator.</param>
    /// <returns>The desired template.</returns>
    private async Task<string> GetClassTemplateAsync(ClassGeneratorOptions options)
    {
        if (_templates.Count == 0)
            await LoadTemplatesAsync();

        var type = options.AddSummary ? TemplateType.ClassDefaultWithNsComment : TemplateType.ClassDefaultWithNs;

        return GetTemplateContent(type);
    }

    /// <summary>
    /// Gets the desired property template.
    /// </summary>
    /// <param name="options">The options of the class generator.</param>
    /// <returns>The desired template.</returns>
    private async Task<string> GetPropertyTemplateAsync(ClassGeneratorOptions options)
    {
        if (_templates.Count == 0)
            await LoadTemplatesAsync();

        return options.AddSummary switch
        {
            true when options is { WithBackingField: true, AddSetProperty: false } => GetTemplateContent(TemplateType
                .PropertyBackingFieldComment),
            true when options is { WithBackingField: true, AddSetProperty: true } => GetTemplateContent(TemplateType
                .PropertyBackingFieldCommentSetField),
            true when !options.WithBackingField => GetTemplateContent(TemplateType.PropertyDefaultComment),
            false when options is { WithBackingField: true, AddSetProperty: false } => GetTemplateContent(TemplateType
                .PropertyBackingFieldDefault),
            false when options is { WithBackingField: true, AddSetProperty: false } => GetTemplateContent(TemplateType
                .PropertyBackingFieldDefaultSetField),
            false when !options.WithBackingField => GetTemplateContent(TemplateType.PropertyDefault),
            _ => string.Empty
        };
    }

    /// <summary>
    /// Gets the desired template.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The template.</returns>
    private string GetTemplateContent(TemplateType type)
    {
        return _templates.FirstOrDefault(f => f.Type == type)?.Content ?? string.Empty;
    }

    #endregion

    #region Various

    /// <summary>
    /// Gets the C# type according to the specified sql type.
    /// </summary>
    /// <param name="sqlType">The sql type.</param>
    /// <returns>The C# type</returns>
    private string GetCSharpType(string sqlType)
    {
        return _typeConversionList
                   .FirstOrDefault(f => f.SqlType.Equals(sqlType, StringComparison.InvariantCultureIgnoreCase))
                   ?.CsharpType ??
               "object"; // Fallback
    }

    /// <summary>
    /// Replaces the values.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="replaceList">The list with the replacement data.</param>
    /// <returns></returns>
    private static string ReplaceValues(string content, SortedList<string, string> replaceList)
    {
        foreach (var entry in replaceList)
        {
            var key = $"${entry.Key.ToUpper()}$";
            content = content.Replace(key, entry.Value);
        }

        return content;
    }

    /// <summary>
    /// Cleans the name of the namespace and removes spaces
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>The cleaned namespace name</returns>
    private static string CleanNamespace(string name)
    {
        const char dot = '.';
        if (!name.Contains(dot))
            return name.FirstChatToUpper().Replace(" ", "");

        var content = name.Split(dot, StringSplitOptions.RemoveEmptyEntries).ToList();
        name = string.Join(dot, content.Select(s => s.FirstChatToUpper()));

        return name.FirstChatToUpper().Replace(" ", "");
    }

    /// <summary>
    /// Cleans the content and removes all empty lines.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="tab">The desired tab.</param>
    /// <returns>The cleaned content.</returns>
    private static string CleanContent(string content, string tab = "")
    {
        var lines = content.Split([Environment.NewLine], StringSplitOptions.None);

        var sb = new StringBuilder();

        foreach (var line in lines.Where(w => !string.IsNullOrWhiteSpace(w)))
        {
            sb.AppendLine(string.IsNullOrEmpty(tab) ? line : $"{tab}{line}");
        }

        return sb.ToString();
    }
    #endregion
}
