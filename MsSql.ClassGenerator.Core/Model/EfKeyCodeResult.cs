namespace MsSql.ClassGenerator.Core.Model;

/// <summary>
/// Provides the key code result.
/// </summary>
public sealed class EfKeyCodeResult
{
    /// <summary>
    /// Gets the code.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Gets the amount of tables which contains multiple keys.
    /// </summary>
    public int TableCount { get; init; }

    /// <summary>
    /// Gets the value which indicates whether the code is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(Code);
}