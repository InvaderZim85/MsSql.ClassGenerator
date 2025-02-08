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
}