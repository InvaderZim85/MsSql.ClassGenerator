using MsSql.ClassGenerator.Core.Business;
using MsSql.ClassGenerator.Core.Common;
using MsSql.ClassGenerator.Core.Model;
using Serilog.Events;

namespace MsSql.ClassGenerator.Cli;

/// <summary>
/// Provides the main code.
/// </summary>
internal static class Program
{
    private static async Task Main(string[] args)
    {
        Helper.InitLog(LogEventLevel.Debug, true);

        var options = new ClassGeneratorOptions
        {
            Output = @"D:\Dump\ClassGenerator",
            Namespace = "Some.NameSpace Here",
            SealedClass = true,
            DbModel = false,
            AddColumnAttribute = false,
            WithBackingField = true,
            AddSetProperty = false,
            AddSummary = false,
            EmptyOutputDirectoryBeforeExport = true,
            AddTableNameToClassSummary = false,
            Modifier = "public"
        };

        var tableManager = new TableManager("(localdb)\\MsSqlLocalDb", "CmsDev");
        await tableManager.LoadTablesAsync("tab*");

        var clasGenerator = new ClassManager();
        await clasGenerator.GenerateClassAsync(options, tableManager.Tables);
    }
}
