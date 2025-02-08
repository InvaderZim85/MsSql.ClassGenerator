using CommandLine;
using Serilog.Events;

namespace MsSql.ClassGenerator.Cli.Model;

/// <summary>
/// Provides the arguments.
/// </summary>
internal sealed class Arguments
{
    /// <summary>
    /// Gets or sets the name / path of the ms sql server.
    /// </summary>
    [Option('s', "server", Required = true, HelpText = "The name / path of the MS SQL server.")]
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the desired database.
    /// </summary>
    [Option('d', "database", Required = true, HelpText = "The name of the desired database which holds the desired tables.")]
    public string Database { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the desired output path.
    /// </summary>
    [Option('o', "output", Required = true, HelpText = "The desired output path which should hold the generated classes.")]
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the desired namespace for the generated classes.
    /// </summary>
    [Option('n', "namespace", Required = true, HelpText = "The desired namespace of the generated classes.")]
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the desired modifier for the classes.
    /// </summary>
    [Option('m', "modifier", Required = false, Default = "public", HelpText = "The class modifier which should be used.")]
    public string Modifier { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the desired filter.
    /// </summary>
    [Option('f', "filter", Required = false, Default = "", HelpText = "The desired filter which should be used to determine the tables.")]
    public string Filter { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value which indicates whether the sealed modifier should be added.
    /// </summary>
    [Option("sealed", Required = false, Default = false, HelpText = "Adds the 'sealed' modifier to the class.")]
    public bool SealedClass { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates whether a class should be created which can be used with <i>Entity Framework Core</i>.
    /// </summary>
    [Option("db-model", Required = false, Default = false, HelpText = "Creates a class which can be used with EF Core.")]
    public bool DbModel { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates whether a column attribute should be added.
    /// </summary>
    [Option("column-attribute", Required = false, HelpText = "Adds the column attribute to each property.")]
    public bool AddColumnAttribute { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates whether a backing field should be added.
    /// </summary>
    [Option("backing-field", Required = false, Default = false, HelpText = "Add a backing-field to each property.")]
    public bool AddBackingField { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates whether the <i>SetProperty</i> function of the <a href="https://learn.microsoft.com/de-de/dotnet/communitytoolkit/mvvm/">CommunityToolKit.MVVM</a> should be used.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: If the value is set to <see langword="true"/>, a backing field is automatically created, even if the value of <see cref="AddBackingField"/> is set to <see langword="false"/>.
    /// </remarks>
    [Option("set-property", Required = false, Default = false, HelpText = "Adds the 'SetProperty' method to the property. Note: This value overwrites the 'backing-field' argument.")]
    public bool AddSetProperty { get; set; }

    /// <summary>
    /// Gets the value which indicates whether an empty <i>summary</i> should be added to the class and each property.
    /// </summary>
    [Option("summary", Required = false, Default = false, HelpText = "Add a summary to each property and the class.")]
    public bool AddSummary { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates whether the output directory should be cleared before the export starts.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: If set to <see langword="true"/>, all <c>*.cs</c> files in the output directory are irretrievably deleted!
    /// </remarks>
    [Option('c', "clean", Required = false, Default = false, HelpText = "Cleans the output directory. Note: All *.cs files in the specified directory will be deleted irretrievable!")]
    public bool EmptyOutputDirectoryBeforeExport { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates whether the table name should be added in the class summary.
    /// </summary>
    [Option("table-name", Required = false, Default = false, HelpText = "Add the table name to the class summary.")]
    public bool AddTableNameToClassSummary { get; set; }

    /// <summary>
    /// Gets or sets the desired log level.
    /// </summary>
    [Option('l', "log-level", Required = false, Default = LogEventLevel.Information, HelpText = "The desired log level (0 = verbose - 5 = fatal).")]
    public LogEventLevel LogLevel { get; set; }
}
