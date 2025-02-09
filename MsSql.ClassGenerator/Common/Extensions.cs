using MsSql.ClassGenerator.Core.Model;
using MsSql.ClassGenerator.Model;
using System.Collections.ObjectModel;

namespace MsSql.ClassGenerator.Common;

/// <summary>
/// Provides several extensions.
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// Converts the given source list into an observable collection.
    /// </summary>
    /// <typeparam name="T">The type of the entries.</typeparam>
    /// <param name="source">The source list.</param>
    /// <returns>The observable collection.</returns>
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
    {
        return new ObservableCollection<T>(source);
    }

    /// <summary>
    /// Converts the table list into the desired target forma (<see cref="TableEntry"/>).
    /// </summary>
    /// <param name="source">The source list.</param>
    /// <returns>The list with the tables.</returns>
    public static List<TableEntry> ToTableEntries(this IEnumerable<TableColumnDto> source)
    {
        var result = new List<TableEntry>();

        foreach (var entry in source.Where(w => w.Use))
        {
            if (entry.OriginalItem is not TableEntry table)
                continue;

            table.Alias = entry.Alias;
            table.Columns = entry.Columns.ToColumnEntries();

            result.Add(table);
        }

        return result;
    }

    /// <summary>
    /// Converts the column list into the desired target format (<see cref="ColumnEntry"/>).
    /// </summary>
    /// <param name="source">The source list.</param>
    /// <returns>The list with the columns.</returns>
    private static List<ColumnEntry> ToColumnEntries(this IEnumerable<TableColumnDto> source)
    {
        var result = new List<ColumnEntry>();

        foreach (var entry in source.Where(w => w.Use))
        {
            if (entry.OriginalItem is not ColumnEntry column)
                continue;

            column.Alias = entry.Alias;

            result.Add(column);
        }

        return result;
    }
}