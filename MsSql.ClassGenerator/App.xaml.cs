using MsSql.ClassGenerator.Core.Common;
using MsSql.ClassGenerator.Model;
using MsSql.ClassGenerator.Properties;
using MsSql.ClassGenerator.Ui;
using MsSql.ClassGenerator.Ui.View;
using Serilog;
using System.Windows;

namespace MsSql.ClassGenerator;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Occurs when the app was started.
    /// </summary>
    /// <param name="sender">The <see cref="App"/>.</param>
    /// <param name="e">The event arguments.</param>
    /// <exception cref="NotImplementedException"></exception>
    private async void App_OnStartup(object sender, StartupEventArgs e)
    {
        try
        {
            // Extract the arguments
            e.Args.ExtractArguments(out Arguments arguments);

            // Init the logger.
            Helper.InitLog(arguments.LogLevel, false);

            // Show the window.
            var mainWindow = new MainWindow(arguments);
            mainWindow.Show();

            // Set the color scheme
            await UiHelper.SetColorThemeAsync(Settings.Default.UiColor);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "A fatal error has occurred.");
        }
    }
}