using MahApps.Metro.Controls;
using MsSql.ClassGenerator.Model;
using MsSql.ClassGenerator.Ui.ViewModel;
using System.Windows;

namespace MsSql.ClassGenerator.Ui.View;

/// <summary>
/// Interaction logic for ServerEntryWindow.xaml
/// </summary>
public partial class ServerEntryWindow : MetroWindow
{
    /// <summary>
    /// Gets the server.
    /// </summary>
    public ServerEntry Server { get; private set; }
 
    /// <summary>
    /// Creates a new instance of the <see cref="ServerEntryWindow"/>.
    /// </summary>
    /// <param name="serverEntry">The entry which should be edited.</param>
    public ServerEntryWindow(ServerEntry? serverEntry = null)
    {
        InitializeComponent();

        Server = serverEntry ?? new ServerEntry();
    }

    /// <summary>
    /// Closes the window with the desired result.
    /// </summary>
    /// <param name="result">The desires result.</param>
    private void CloseWindow(bool result)
    {
        DialogResult = result;
    }

    /// <summary>
    /// Occurs when the window was loaded.
    /// </summary>
    /// <param name="sender">The <see cref="ServerEntryWindow"/>.</param>
    /// <param name="e">The event arguments.</param>
    private void ServerEntryWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ServerEntryWindowViewModel viewModel)
            viewModel.InitViewModel(Server, CloseWindow);
    }
}