using CommandLine;
using MsSql.ClassGenerator.Core.Common.Enums;
using Serilog;

namespace MsSql.ClassGenerator.Core.Common;

/// <summary>
/// Provides several helper functions.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Extracts the arguments.
    /// </summary>
    /// <param name="args">The provided arguments.</param>
    /// <param name="arguments">The class with the extracted arguments.</param>
    /// <returns><see langword="true"/> when everything was successful, otherwise <see langword="false"/>.</returns>
    public static bool ExtractArguments<T>(this IEnumerable<string> args, out T arguments) where T : class, new()
    {
        arguments = new T();

        var tmpArgs = Parser.Default.ParseArguments<T>(args).Value;

        if (tmpArgs == null)
            return false;

        arguments = tmpArgs;
        return true;
    }

    /// <summary>
    /// Logs the properties of the object.
    /// </summary>
    /// <param name="obj">The obj.</param>
    /// <param name="header">The desired header.</param>
    public static void LogObject(this object obj, string header)
    {
        var properties = obj.GetType().GetProperties();

        var maxLength = properties.Max(m => m.Name.Length);

        Log.Information(header);

        foreach (var property in properties)
        {
            Log.Information("- {name}: {value}", property.Name.PadRight(maxLength, '.'), property.GetValue(obj));
        }
    }

    /// <summary>
    /// Checks if the value matches the desired filter.
    /// </summary>
    /// <param name="value">The value (for example the name of a table).</param>
    /// <param name="filter">The desired filter.</param>
    /// <returns><see langword="true"/> when the filter matches the value, otherwise <see langword="false"/></returns>
    public static bool MatchFilter(this string value, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return true; // No filter.

        var start = filter.StartsWith('*');
        var end = filter.EndsWith('*');

        // Remove the wildcard
        filter = filter.Replace("*", "");

        return start switch
        {
            true when end => value.Contains(filter, StringComparison.InvariantCultureIgnoreCase),
            true => value.EndsWith(filter, StringComparison.InvariantCultureIgnoreCase),
            false when end => value.StartsWith(filter, StringComparison.InvariantCultureIgnoreCase),
            _ => value.Equals(filter, StringComparison.InvariantCultureIgnoreCase)
        };
    }

    /// <summary>
    /// Converts the first char of a string to lower case
    /// </summary>
    /// <param name="value">The original value</param>
    /// <returns>The converted string</returns>
    public static string FirstCharToLower(this string value)
    {
        return ChangeFirstChart(value, false);
    }

    /// <summary>
    /// Converts the first char of a string to upper case
    /// </summary>
    /// <param name="value">The original value</param>
    /// <returns>The converted string</returns>
    public static string FirstChatToUpper(this string value)
    {
        return ChangeFirstChart(value, true);
    }

    /// <summary>
    /// Changes the casing of the first char
    /// </summary>
    /// <param name="value">The original value</param>
    /// <param name="upper"><see langword="true"/> to convert the first char to upper, <see langword="false"/> to convert the first char to lower</param>
    /// <returns>The converted string</returns>
    private static string ChangeFirstChart(string value, bool upper)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return upper
            ? $"{value[..1].ToUpper()}{value[1..]}"
            : $"{value[..1].ToLower()}{value[1..]}";
    }
}
