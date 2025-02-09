namespace MsSql.ClassGenerator.Model;

/// <summary>
/// Contains the options.
/// </summary>
internal sealed class OptionDto
{
    /// <summary>
    /// Gets or sets the value which indicates whether the output directory should be cleaned before a new export.
    /// </summary>
    public bool CleanExportDirectory { get; set; }

    /// <summary>
    /// Gets or sets the desired modifier.
    /// </summary>
    public string Modifier { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value which indicates whether the <c>sealed</c> modifier should be added.
    /// </summary>
    public bool AddSealed { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates whether the classes can be used for <i>Entity Framework Core</i>.
    /// </summary>
    public bool DbModel { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates whether a column attribute should be added.
    /// </summary>
    public bool AddColumnAttribute { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates whether a backing field should be generated.
    /// </summary>
    public bool AddBackingField { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates whether the <c>SetProperty</c> method should be added.
    /// </summary>
    public bool AddSetProperty { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates whether a summary should be added to the class and each property.
    /// </summary>
    public bool AddSummary { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates whether the table name should be added to the class summary.
    /// </summary>
    public bool AddTableToSummary { get; set; }
}
