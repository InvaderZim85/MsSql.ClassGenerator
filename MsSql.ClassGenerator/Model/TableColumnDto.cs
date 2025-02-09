using CommunityToolkit.Mvvm.ComponentModel;
using MsSql.ClassGenerator.Core.Model;

namespace MsSql.ClassGenerator.Model;

/// <summary>
/// Represents a table.
/// </summary>
public sealed partial class TableColumnDto : ObservableObject
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the table.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the alias.
    /// </summary>
    [ObservableProperty]
    private string _alias = string.Empty;

    /// <summary>
    /// Occurs when the user changes the alias.
    /// </summary>
    /// <param name="value">The new alias.</param>
    partial void OnAliasChanged(string value)
    {
        if (Use)
            return;
            
        Use = !string.IsNullOrWhiteSpace(value);
        OnPropertyChanged(nameof(Use));
    }

    /// <summary>
    /// Gets or sets the position (only needed for the columns).
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates whether the table should be used for the export.
    /// </summary>
    [ObservableProperty]
    private bool _use = true;

    /// <summary>
    /// Gets or sets the original item (table / column)
    /// </summary>
    public object? OriginalItem { get; set; }

    /// <summary>
    /// Gets or sets the list with the columns.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: Use this only when the current entry is a <i>table</i>.
    /// </remarks>
    public List<TableColumnDto> Columns { get; set; } = [];

    /// <summary>
    /// Creates a new table instance.
    /// </summary>
    /// <param name="table">The source table.</param>
    public TableColumnDto(TableEntry table)
    {
        Id = table.Id;
        Name = table.Name;
        Alias = table.Alias;
        Columns = table.Columns.Select(s => new TableColumnDto(s)).ToList();
        Use = true;
        OriginalItem = table;
    }

    /// <summary>
    /// Creates a new column instance.
    /// </summary>
    /// <param name="column">The source column.</param>
    public TableColumnDto(ColumnEntry column)
    {
        Name = column.Name;
        Alias = column.Alias;
        Position = column.Order;
        Use = true;
        OriginalItem = column;
    }
}