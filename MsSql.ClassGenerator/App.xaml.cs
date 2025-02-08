using System.Windows;
using MsSql.ClassGenerator.Model.Internal;
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
        var mainWindow = new MainWindow(new Arguments());
        mainWindow.Show();
    }
}