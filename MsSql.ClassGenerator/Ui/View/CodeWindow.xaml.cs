using ICSharpCode.AvalonEdit.Search;
using MahApps.Metro.Controls;
using MsSql.ClassGenerator.Ui.ViewModel;
using System.Windows;
using MsSql.ClassGenerator.Core.Model;

namespace MsSql.ClassGenerator.Ui.View;

/// <summary>
/// Interaction logic for CodeWindow.xaml
/// </summary>
public partial class CodeWindow : MetroWindow
{
    /// <summary>
    /// Contains the code which should be shown.
    /// </summary>
    private readonly EfKeyCodeResult _efKeyCode;

    /// <summary>
    /// Creates a new instance of the <see cref="CodeWindow"/>.
    /// </summary>
    /// <param name="efKeyCode">The ef key code which should be shown.</param>
    public CodeWindow(EfKeyCodeResult efKeyCode)
    {
        InitializeComponent();

        _efKeyCode = efKeyCode;
    }

    /// <summary>
    /// Occurs when the window was loaded.
    /// </summary>
    /// <param name="sender">The <see cref="CodeWindow"/>.</param>
    /// <param name="e">The event arguments.</param>
    private void CodeWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Init the editor
        CodeEditor.InitAvalonEditor();

        // Add the search option (CTRL + F)
        SearchPanel.Install(CodeEditor);

        CodeEditor.Text = _efKeyCode.Code;

        if (DataContext is CodeWindowViewModel viewModel)
            viewModel.InitViewModel(_efKeyCode);
    }
}