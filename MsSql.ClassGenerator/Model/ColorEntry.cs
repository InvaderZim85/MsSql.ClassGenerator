using System.Windows.Media;

namespace MsSql.ClassGenerator.Model;

/// <summary>
/// Represents a color entry (only needed for the settings).
/// </summary>
/// <param name="Name">The name of the color.</param>
/// <param name="CustomColor">The value which indicates whether the color is a custom color.</param>
/// <param name="Color">The color value (only needed for the custom colors).</param>
internal sealed record ColorEntry(string Name, bool CustomColor, string Color = "")
{
    /// <summary>
    /// Gets the color value
    /// </summary>
    public Color ColorValue => ColorConverter.ConvertFromString(Color) is Color color
        ? color // The custom color
        : System.Windows.Media.Color.FromRgb(29, 136, 188); // Default color "Cyan"

    /// <inheritdoc />
    public override string ToString()
    {
        return CustomColor ? $"{Name} (Custom)" : Name;
    }
}