using CommandLine;

namespace MsSql.ClassGenerator.Model.Internal;

/// <summary>
/// Provides the arguments.
/// </summary>
internal sealed class Arguments
{
    /// <summary>
    /// Gets or sets the name / path of the ms sql server.
    /// </summary>
    [Option('s', "server", Required = false, HelpText = "The name / path of the MS SQL server.")]
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the desired database.
    /// </summary>
    [Option('d', "database", Required = false, HelpText = "The name of the desired database which holds the desired tables.")]
    public string Database { get; set; } = string.Empty;
}
