using MahApps.Metro.Controls;
using MsSql.ClassGenerator.Model;
using MsSql.ClassGenerator.Ui.ViewModel;
using System.Windows;
using Microsoft.VisualStudio.Threading;
using Serilog;

namespace MsSql.ClassGenerator.Ui.View;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MetroWindow
{
    /// <summary>
    /// Contains the arguments.
    /// </summary>
    private readonly Arguments _arguments;

    /// <summary>
    /// Creates a new instance of the <see cref="MainWindow"/>.
    /// </summary>
    /// <param name="arguments">The provided arguments.</param>
    public MainWindow(Arguments arguments)
    {
        InitializeComponent();

        _arguments = arguments;
    }

    /// <summary>
    /// Occurs when the window was loaded.
    /// </summary>
    /// <param name="sender">The <see cref="MainWindow"/>.</param>
    /// <param name="e">The event arguments.</param>
    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is MainWindowViewModel viewModel)
                viewModel.InitViewModelAsync(_arguments).Forget();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while initializing the main window.");
        }
    }
}
