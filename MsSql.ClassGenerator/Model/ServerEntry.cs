namespace MsSql.ClassGenerator.Model;

/// <summary>
/// Represents a server entry.
/// </summary>
public sealed class ServerEntry
{
    /// <summary>
    /// Gets or sets the name of the server.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the default database.
    /// </summary>
    public string DefaultDatabase { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value which indicates whether the server should connect automatically.
    /// </summary>
    public bool AutoConnect { get; set; }

    /// <summary>
    /// Gets or sets the description of the entry.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <inheritdoc />
    public override string ToString()
    {
        var name = string.IsNullOrWhiteSpace(Description)
            ? Name
            : $"{Name} - {Description}";

        return AutoConnect
            ? $"{name} (A)"
            : name;
    }
}