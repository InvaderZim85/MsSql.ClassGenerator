using MsSql.ClassGenerator.Core.Model;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Serilog;
using System.Reflection;

namespace MsSql.ClassGenerator.Core.Business;

/// <summary>
/// Provides the functions for the update.
/// </summary>
public static class UpdateHelper
{
    /// <summary>
    /// Contains the URL of the latest release of the app.
    /// </summary>
    public const string GitHupUrl = "https://github.com/InvaderZim85/MsSql.ClassGenerator/releases/latest";

    /// <summary>
    /// Contains the URL of the latest release for the REST call.
    /// </summary>
    private const string GitHupApiUrl =
        "https://api.github.com/repos/InvaderZim85/MsSql.ClassGenerator/releases/latest";

    /// <summary>
    /// Loads the release info of the latest release to determine if there is a new version.
    /// </summary>
    /// <param name="callback">Will be executed when there is a new version.</param>
    /// <returns>The awaitable task.</returns>
    public static async Task LoadReleaseInfoAsync(Action<ReleaseInfo> callback)
    {
        try
        {
            var client = new RestClient(GitHupApiUrl,
                configureSerialization: s => s.UseNewtonsoftJson());

            client.AddDefaultHeader("accept", "application/vnd.github.v3+json");

            var request = new RestRequest();
            var response = await client.GetAsync<ReleaseInfo>(request);

            // This method also checks if the response is null
            if (IsNewVersionAvailable(response))
                callback(response!);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error while loading the latest release info.");
        }
    }

    /// <summary>
    /// Checks if an update is available
    /// </summary>
    /// <param name="releaseInfo">The infos of the latest release</param>
    /// <returns><see langword="true"/> when there is a new version, otherwise <see langword="false"/></returns>
    private static bool IsNewVersionAvailable(ReleaseInfo? releaseInfo)
    {
        if (releaseInfo == null)
            return false;

        if (!Version.TryParse(releaseInfo.TagName.Replace("v", ""), out var releaseVersion))
        {
            Log.Warning("Can't determine version of the latest release. Tag value: {value}", releaseInfo.TagName);
            return false;
        }

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        if (currentVersion == null)
            return false;

        releaseInfo.CurrentVersion = currentVersion;
        releaseInfo.NewVersion = releaseVersion;

        return releaseInfo.UpdateAvailable;
    }
}