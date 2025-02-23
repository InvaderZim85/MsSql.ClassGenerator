using MsSql.ClassGenerator.Core.Common;
using Newtonsoft.Json;

namespace MsSql.ClassGenerator.Core.Model;

/// <summary>
/// Represents the assets of the last release
/// </summary>
public sealed class Asset
{
    /// <summary>
    /// Gets or sets the name of the zip file
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the size of the latest release (in bytes)
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets the size of the latest release in a readable format
    /// </summary>
    [JsonIgnore]
    public string SizeView => Size.ConvertSize();

    /// <summary>
    /// Gets or sets the url of the release
    /// </summary>
    [JsonProperty("browser_download_url")]
    public string DownloadUrl { get; set; } = string.Empty;
}