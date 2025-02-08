using System.Diagnostics;
using System.Reflection;
using MsSql.ClassGenerator.Cli.Model;
using MsSql.ClassGenerator.Core.Business;
using MsSql.ClassGenerator.Core.Common;
using MsSql.ClassGenerator.Core.Model;
using Serilog;
using Serilog.Events;

namespace MsSql.ClassGenerator.Cli;

/// <summary>
/// Provides the main code.
/// </summary>
internal static class Program
{
    /// <summary>
    /// The main entry point of the program.
    /// </summary>
    /// <param name="args">The provided arguments.</param>
    /// <returns>The awaitable task.</returns>
    private static async Task Main(string[] args)
    {
        var argResult = args.ExtractArguments(out Arguments arguments);

        Helper.InitLog(arguments.LogLevel, true);

        PrintFooterHeader(true);

        if (!argResult)
        {
            Log.Error("Arguments missing.");
            PrintFooterHeader(false);
            return;
        }

        // Print the arguments
        arguments.LogObject("Arguments");

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var options = GetOptions(arguments);

            // Load the tables
            var tableManager = new TableManager(arguments.Server, arguments.Database);
            await tableManager.LoadTablesAsync(arguments.Filter);

            // Generate the class
            var classGenerator = new ClassManager();
            await classGenerator.GenerateClassAsync(options, tableManager.Tables);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "A fatal error has occurred.");
        }
        finally
        {
            stopwatch.Stop();
            Log.Information("Duration: {duration}", stopwatch.Elapsed);
            PrintFooterHeader(false);
        }
    }

    /// <summary>
    /// Converts the arguments into the needed options.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <returns>The class generator options.</returns>
    private static ClassGeneratorOptions GetOptions(Arguments arguments)
    {
        return new ClassGeneratorOptions
        {
            Output = arguments.OutputPath,
            Namespace = arguments.Namespace,
            Modifier = arguments.Modifier,
            SealedClass = arguments.SealedClass,
            DbModel = arguments.DbModel,
            AddColumnAttribute = arguments.AddColumnAttribute,
            WithBackingField = arguments.AddBackingField,
            AddSetProperty = arguments.AddSetProperty,
            AddSummary = arguments.AddSummary,
            EmptyOutputDirectoryBeforeExport = arguments.EmptyOutputDirectoryBeforeExport,
            AddTableNameToClassSummary = arguments.AddTableNameToClassSummary
        };
    }

    /// <summary>
    /// Prints a header / footer.
    /// </summary>
    /// <param name="header"><see langword="true"/> to print a header, otherwise <see langword="false"/>.</param>
    private static void PrintFooterHeader(bool header)
    {
        var type = header ? "Start" : "End";

        Log.Information("==== {type} {name} ====", type, "MsSqlClassGenerator");

        if (header)
            Log.Information("Version: {version}", Assembly.GetExecutingAssembly().GetName().Version);
    }
}
