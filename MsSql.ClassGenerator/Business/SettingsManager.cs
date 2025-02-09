using ControlzEx.Theming;
using MsSql.ClassGenerator.Core.Common;
using MsSql.ClassGenerator.Model;
using Serilog;
using System.IO;

namespace MsSql.ClassGenerator.Business;

/// <summary>
/// Provides several helper functions.
/// </summary>
internal static class SettingsManager
{
    /// <summary>
    /// Contains the path of the file which contains the server information.
    /// </summary>
    private static readonly string ServerListFile = Path.Combine(AppContext.BaseDirectory, "ServerList.json");

    /// <summary>
    /// Contains the path of the file which contains the options.
    /// </summary>
    private static readonly string OptionsFile = Path.Combine(AppContext.BaseDirectory, "Options.json");

    /// <summary>
    /// Contains the path of the file which contains the custom colors.
    /// </summary>
    private static readonly string CustomColorFile = Path.Combine(AppContext.BaseDirectory, "CustomColors.json");

    #region Server

    /// <summary>
    /// Loads the server list (if available).
    /// </summary>
    /// <returns>The list with the server entries.</returns>
    public static async Task<List<ServerEntry>> LoadServerListAsync()
    {
        if (!File.Exists(ServerListFile))
            return [];

        return await Helper.LoadJsonAsync<List<ServerEntry>>(ServerListFile);
    }

    /// <summary>
    /// Checks if the desired server already exists.
    /// </summary>
    /// <param name="server">The server which should be checked.</param>
    /// <returns><see langword="true"/> when the server already exists, otherwise <see langword="false"/>.</returns>
    public static async Task<bool> ServerExistsAsync(ServerEntry server)
    {
        var serverList = await LoadServerListAsync();

        return serverList
            .Where(w => w.Id != server.Id)
            .Any(a => a.Name.Equals(server.Name, StringComparison.InvariantCultureIgnoreCase) &&
                      a.DefaultDatabase.Equals(server.DefaultDatabase,
                          StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Adds the desired server to the server list.
    /// </summary>
    /// <param name="server">The desired server.</param>
    /// <returns>The awaitable task.</returns>
    public static async Task AddServerAsync(ServerEntry server)
    {
        var serverList = await LoadServerListAsync();

        // Create the unique id
        server.Id = Guid.NewGuid();
        serverList.Add(server);

        await SaveServerListAsync(serverList);
    }

    /// <summary>
    /// Deletes the desires server (removes it from the list).
    /// </summary>
    /// <param name="server">The server which should be deleted.</param>
    /// <returns>The awaitable task.</returns>
    public static async Task DeleteServerAsync(ServerEntry server)
    {
        var serverList = await LoadServerListAsync();

        // Remove the server
        var deleteEntry = serverList.FirstOrDefault(f => f.Id == server.Id);
        if (deleteEntry == null)
            return;

        serverList.Remove(server);

        await SaveServerListAsync(serverList);
    }

    /// <summary>
    /// Saves the server list.
    /// </summary>
    /// <param name="serverList">The list with the server.</param>
    /// <returns>The awaitable task.</returns>
    public static async Task SaveServerListAsync(List<ServerEntry> serverList)
    {
        await Helper.SaveJsonFileAsync(serverList, ServerListFile);
    }

    #endregion

    #region Settings / Options

    /// <summary>
    /// Loads the options.
    /// </summary>
    /// <returns>The options.</returns>
    public static async Task<OptionDto> LoadOptionsAsync()
    {
        if (!File.Exists(OptionsFile))
            return new OptionDto();

        return await Helper.LoadJsonAsync<OptionDto>(OptionsFile);
    }

    /// <summary>
    /// Saves the options.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <returns>The awaitable task.</returns>
    public static async Task SaveOptionsAsync(OptionDto options)
    {
        await Helper.SaveJsonFileAsync(options, OptionsFile);
    }
    #endregion

    #region Colors

    /// <summary>
    /// Loads the list with the available colors (provided and custom colors).
    /// </summary>
    /// <returns>The list with the colors.</returns>
    public static async Task<List<ColorEntry>> LoadColorsAsync()
    {
        var result = ThemeManager.Current.ColorSchemes.Select(s => new ColorEntry(s, false)).ToList();

        // Load the custom colors (if exists).
        if (File.Exists(CustomColorFile))
        {
            try
            {
                result.AddRange(await LoadCustomColors());
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error while loading the custom colors.");
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if the specified color already exists.
    /// </summary>
    /// <param name="color">The color which should be checked.</param>
    /// <returns>The awaitable task.</returns>
    public static async Task<bool> ColorAlreadyExists(ColorEntry color)
    {
        // Load the custom colors
        var customColors = await LoadCustomColors();

        return customColors.Any(a => a.Name.Equals(color.Name));
    }

    /// <summary>
    /// Saves the custom color.
    /// </summary>
    /// <param name="color">The color which should be saved.</param>
    /// <returns>The awaitable task.</returns>
    public static async Task SaveCustomColorAsync(ColorEntry color)
    {
        // Load the custom colors
        var customColors = await LoadCustomColors();
        customColors.Add(color);

        await Helper.SaveJsonFileAsync(customColors, CustomColorFile);
    }

    /// <summary>
    /// Deletes the desires custom color.
    /// </summary>
    /// <param name="color">The color which should be deleted.</param>
    /// <returns>The awaitable task.</returns>
    public static async Task DeleteCustomColorAsync(ColorEntry color)
    {
        // Load the custom colors
        var customColors = await LoadCustomColors();

        var delColor = customColors.FirstOrDefault(f => f.Name.Equals(color.Name) &&
                                                        f.Color.Equals(color.Color));
        if (delColor == null)
            return;

        customColors.Remove(delColor);

        await Helper.SaveJsonFileAsync(customColors, CustomColorFile);
    }

    /// <summary>
    /// Loads the custom colors.
    /// </summary>
    /// <returns>The list with the custom colors.</returns>
    private static async Task<List<ColorEntry>> LoadCustomColors()
    {
        if (!File.Exists(CustomColorFile))
            return [];

        return await Helper.LoadJsonAsync<List<ColorEntry>>(CustomColorFile);
    }
    #endregion
}