using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

namespace MsSql.ClassGenerator.Core.Common;

/// <summary>
/// Provides several helper functions.
/// </summary>
public static class Helper
{
    /// <summary>
    /// Init the logger.
    /// </summary>
    /// <param name="logLevel">The desired log level.</param>
    /// <param name="withConsole"><see langword="true"/> to log also to the console, otherwise <see langword="false"/>.</param>
    public static void InitLog(LogEventLevel logLevel, bool withConsole)
    {
        // Template
        const string template = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        // Init the logger
        if (withConsole)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(logLevel) // Set the desired min. log level
                .WriteTo.Console(outputTemplate: template) // Add the console sink
                .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "log", "log_.log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: template) // Add the file sink
                .CreateLogger();
        }
        else
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(logLevel) // Set the desired min. log level
                .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "log", "log_.log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: template) // Add the file sink
                .CreateLogger();
        }
    }

    /// <summary>
    /// Load the content of a JSON formatted file.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="filepath">The path of the file which contains the data.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Will be thrown when the specified filepath is empty.</exception>
    /// <exception cref="FileNotFoundException">Will be thrown when the specified file doesn't exist.</exception>
    public static async Task<T> LoadJsonAsync<T>(string filepath) where T : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filepath);

        if (!File.Exists(filepath))
            throw new FileNotFoundException($"The specified file '{filepath}' doesn't exist.", filepath);

        var content = await File.ReadAllTextAsync(filepath);

        return JsonConvert.DeserializeObject<T>(content) ?? new T();
    }
}
