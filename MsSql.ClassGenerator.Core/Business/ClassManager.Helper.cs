﻿using MsSql.ClassGenerator.Core.Common;
using MsSql.ClassGenerator.Core.Common.Enums;
using MsSql.ClassGenerator.Core.Model;
using Serilog;
using System.Text;

namespace MsSql.ClassGenerator.Core.Business;

/// <inheritdoc cref="ClassManager"/>
public sealed partial class ClassManager
{
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
            return new TemplateDto(type, []);
        }

        // Load the content of the file
        var content = await File.ReadAllLinesAsync(templateFile.FullName);

        return new TemplateDto(type, [..content]);
    }

    /// <summary>
    /// Gets the desired template.
    /// </summary>
    /// <param name="options">The options of the class generator.</param>
    /// <returns>The desired template.</returns>
    private async Task<List<string>> GetClassTemplateAsync(ClassGeneratorOptions options)
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
    private async Task<List<string>> GetPropertyTemplateAsync(ClassGeneratorOptions options)
    {
        if (_templates.Count == 0)
            await LoadTemplatesAsync();

        // Check the options
        var withBackingField = options.AddSetProperty || options.WithBackingField;

        return options.AddSummary switch
        {
            true when withBackingField && options is { AddSetProperty: false } => GetTemplateContent(TemplateType
                .PropertyBackingFieldComment),
            true when withBackingField && options is { AddSetProperty: true } => GetTemplateContent(TemplateType
                .PropertyBackingFieldCommentSetField),
            true when !withBackingField => GetTemplateContent(TemplateType.PropertyDefaultComment),
            false when withBackingField && options is { AddSetProperty: false } => GetTemplateContent(TemplateType
                .PropertyBackingFieldDefault),
            false when withBackingField && options is { AddSetProperty: true } => GetTemplateContent(TemplateType
                .PropertyBackingFieldDefaultSetField),
            false when !withBackingField => GetTemplateContent(TemplateType.PropertyDefault),
            _ => []
        };
    }

    /// <summary>
    /// Gets the desired template.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The template.</returns>
    private List<string> GetTemplateContent(TemplateType type)
    {
        return _templates.FirstOrDefault(f => f.Type == type)?.Content ?? [];
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
    /// Finalizes the template.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="replaceList">The list with the replacement data.</param>
    /// <returns>The content of the template.</returns>
    private static string FinalizeTemplate(List<string> content, List<ReplacementDto> replaceList)
    {
        var sb = new StringBuilder();

        foreach (var line in content)
        {
            // Check if the line contains a value which should be replaced.
            if (!line.Contains('$'))
            {
                sb.AppendLine(line);
                continue;
            }

            var tmpLine = line;

            // Iterate through the replacement values
            foreach (var replacement in replaceList)
            {
                var key = $"${replacement.Key.ToUpper()}$";

                if (!replacement.Indent)
                {
                    tmpLine = tmpLine.Replace(key, replacement.Value);
                    continue;
                }

                // Split the value, because it's possible that the value has multiple entries
                var tmpValue = replacement.Value
                    .Split([Environment.NewLine], StringSplitOptions.None)
                    .Select(s => $"{Tab}{s}");
                tmpLine = tmpLine.Replace(key, string.Join(Environment.NewLine, tmpValue));
            }

            if (!string.IsNullOrWhiteSpace(tmpLine))
                sb.AppendLine(tmpLine);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates a <i>clean</i> namespace and removes all <i>invalid</i> chars like ä, ö, ü and so on.
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>The cleaned namespace name</returns>
    private static string GenerateNamespace(string name)
    {
        const char dot = '.';
        if (!name.Contains(dot))
            return name.FirstChatToUpper().Replace(" ", "");

        var content = name.Split(dot, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        name = string.Join(dot, content.Select(s => s.FirstChatToUpper()));

        var result = name.FirstChatToUpper().Replace(" ", "");

        // Remove all "invalid" chars
        result = GetInvalidCharReplaceList(false)
            .Aggregate(result, (current, entry) => current.Replace(entry.Key, entry.Value));

        return result.StartsWithNumber() ? $"Ns{result}" : result;
    }

    /// <summary>
    /// Generates a <i>clean</i> class name and removes all <i>invalid</i> chars like ä, ö, ü and so on.
    /// </summary>
    /// <param name="name">The desired name.</param>
    /// <returns>The generated class name.</returns>
    private static string GenerateClassName(string name)
    {
        IReadOnlyCollection<ReplacementDto> replaceList;

        if (name.Contains('_'))
        {
            replaceList = GetInvalidCharReplaceList(false);

            // Split the entry at the underscore
            var content = name.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Create a new "class" name
            name = content.Aggregate(string.Empty, (current, entry) => current + entry.FirstChatToUpper());
        }
        else
        {
            replaceList = GetInvalidCharReplaceList();
            name = name.FirstChatToUpper();
        }

        // Remove all "invalid" chars
        var result = replaceList.Aggregate(name, (current, entry) => current.Replace(entry.Key, entry.Value));

        return result.StartsWithNumber() ? $"Class{result}" : result;
    }

    /// <summary>
    /// Generates a <i>clean</i> property name and removes all <i>invalid</i> chars like ä, ö, ü and so on.
    /// </summary>
    /// <param name="name">The desired name.</param>
    /// <returns>The generated property name.</returns>
    private static string GeneratePropertyName(string name)
    {
        // Remove all "invalid" chars
        name = GetInvalidCharReplaceList(true)
            .Aggregate(name, (current, entry) => current.Replace(entry.Key, entry.Value));

        // Change the first char to upper
        name = name.FirstChatToUpper();

        return name.StartsWithNumber() ? $"Column{name}" : name;
    }

    /// <summary>
    /// Gets the list with the <i>invalid</i> chars which should be always replaced.
    /// </summary>
    /// <param name="includeUnderscore"><see langword="true"/> to include an underscore in the list, otherwise <see langword="false"/>.</param>
    /// <returns>The list with the values.</returns>
    private static List<ReplacementDto> GetInvalidCharReplaceList(bool includeUnderscore = true)
    {
        var tmpList = new List<ReplacementDto>
        {
            new(" ", ""), // this should never happen...
            new("ä", "ae"),
            new("ö", "oe"),
            new("ü", "ue"),
            new("ß", "ss"),
            new("Ä", "Ae"),
            new("Ö", "Oe"),
            new("Ü", "Ue")
        };

        if (includeUnderscore)
            tmpList.Add(new ReplacementDto("_", ""));

        return tmpList;
    }

    /// <summary>
    /// Cleans the directory and deletes all existing <c>*.cs</c> files.
    /// </summary>
    /// <param name="path">The path of the desired directory.</param>
    private static void CleanDirectory(string path)
    {
        var files = Directory.GetFiles(path, "*.cs", SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            File.Delete(file);
        }
    }
    #endregion
}
