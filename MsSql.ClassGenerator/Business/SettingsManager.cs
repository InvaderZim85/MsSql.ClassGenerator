using MsSql.ClassGenerator.Core.Common;
using MsSql.ClassGenerator.Model;
using System.IO;

namespace MsSql.ClassGenerator.Business;

/// <summary>
/// Provides several helper functions.
/// </summary>
public static class SettingsManager
{
    /// <summary>
    /// Contains the path of the file which contains the server information.
    /// </summary>
    private static readonly string ServerListFile = Path.Combine(AppContext.BaseDirectory, "ServerList.json");

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

        return serverList.Any(a => a.Name.Equals(server.Name, StringComparison.InvariantCultureIgnoreCase) &&
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
        serverList.Add(server);

        await Helper.SaveJsonFileAsync(serverList, ServerListFile);
    }
}