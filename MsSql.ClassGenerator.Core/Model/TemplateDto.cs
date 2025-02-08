using MsSql.ClassGenerator.Core.Common.Enums;

namespace MsSql.ClassGenerator.Core.Model;

/// <summary>
/// Provides the content of a template.
/// </summary>
/// <param name="Type">The type of the template.</param>
/// <param name="Content">The content.</param>
public sealed record TemplateDto(TemplateType Type, List<string> Content);
