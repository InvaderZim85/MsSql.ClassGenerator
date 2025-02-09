using System.Windows;
using MsSql.ClassGenerator.Core.Common;
using MsSql.ClassGenerator.Model;
using MsSql.ClassGenerator.Ui.View;

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
    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        e.Args.ExtractArguments(out Arguments arguments);

        Helper.InitLog(arguments.LogLevel, false);

        var mainWindow = new MainWindow(arguments);
        mainWindow.Show();
    }
}