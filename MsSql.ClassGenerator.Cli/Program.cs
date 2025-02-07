using MsSql.ClassGenerator.Core.Business;
using MsSql.ClassGenerator.Core.Common;
using MsSql.ClassGenerator.Core.Model;
using Serilog.Events;

namespace MsSql.ClassGenerator.Cli;

internal class Program
{
    static async Task Main(string[] args)
    {
        Helper.InitLog(LogEventLevel.Debug, true);

        var options = new ClassGeneratorOptions
        {
            Output = string.Empty,
            Namespace = "Blub",
            SealedClass = true,
            DbModel = true,
            AddColumnAttribute = true,
            WithBackingField = false,
            AddSetProperty = false,
            AddSummary = true,
            EmptyOutputDirectoryBeforeExport = true,
            AddTableNameToClassSummary = true,
            Modifier = "public",
            Filter = "size_*"
        };

        var tableManager = new TableManager("(localdb)\\MsSqlLocalDb", "CmsDev");
        await tableManager.LoadTablesAsync("ProductCategory");

        var clasGenerator = new ClassManager();
        await clasGenerator.GenerateClassAsync(options, tableManager.Tables);
    }
}
