using MsSql.ClassGenerator.Core.Data;
using MsSql.ClassGenerator.Core.Model;
using Serilog;

namespace MsSql.ClassGenerator.Core.Business;

/// <summary>
/// Provides the functions for the interaction with the tables.
/// </summary>
/// <param name="server">The name / path of the MS SQL server.</param>
/// <param name="database">The name of the desired database.</param>
public sealed class TableManager(string server, string database)
{
    /// <summary>
    /// Occurs when progress was made.
    /// </summary>
    public event EventHandler<string>? ProgressEvent; 

    /// <summary>
    /// The instance for the interaction with the table data.
    /// </summary>
    private readonly TableRepo _tableRepo = new(server, database);

    /// <summary>
    /// Gets the list with the tables.
    /// </summary>
    /// <remarks>
    /// <i>Info</i>: To load the tables, call the function <see cref="LoadTablesAsync"/>.
    /// </remarks>
    public List<TableEntry> Tables { get; private set; } = [];

    /// <summary>
    /// Loads all available user tables (with its columns and PK information) and stores the result into <see cref="Tables"/>.
    /// </summary>
    /// <param name="filter">The desired filter.</param>
    /// <returns>The awaitable task.</returns>
    public async Task LoadTablesAsync(string filter)
    {
        Log.Debug("Load tables. Filter: '{filter}'", filter);
        ProgressEvent?.Invoke(this, "Load tables...");

        Tables = await _tableRepo.LoadTablesAsync(filter);

        // Load the PK information
        var count = 1;
        foreach (var table in Tables)
        {
            var message = $"{count++} of {Tables.Count} > Load PK information for table '{table.Name}'...";
            Log.Debug(message);
            ProgressEvent?.Invoke(this, message);
            await _tableRepo.LoadPrimaryKeyInfoAsync(table);
        }

        Log.Debug("{count} tables loaded.", Tables.Count);
    }
}
