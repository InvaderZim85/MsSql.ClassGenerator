using CommandLine;
using Serilog.Events;

namespace MsSql.ClassGenerator.Model;

/// <summary>
/// Provides the arguments.
/// </summary>
public sealed class Arguments
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

    /// <summary>
    /// Gets or sets the desired log level.
    /// </summary>
    [Option('l', "log-level", Required = false, Default = LogEventLevel.Information, HelpText = "The desired log level (0 = verbose - 5 = fatal).")]
    public LogEventLevel LogLevel { get; set; }

    /// <summary>
    /// Gets the value which indicates whether the arguments are empty.
    /// </summary>
    public bool Empty => string.IsNullOrWhiteSpace(Server) &&
                         string.IsNullOrWhiteSpace(Database);
}
