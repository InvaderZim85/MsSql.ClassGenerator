using MahApps.Metro.Controls;

namespace MsSql.ClassGenerator.Ui.View;

/// <summary>
/// Interaction logic for CodeWindow.xaml
/// </summary>
public partial class CodeWindow : MetroWindow
{
    /// <summary>
    /// Contains the code which should be shown.
    /// </summary>
    private readonly string _code;

    /// <summary>
    /// Creates a new instance of the <see cref="CodeWindow"/>.
    /// </summary>
    /// <param name="code">The code which should be shown.</param>
    public CodeWindow(string code)
    {
        InitializeComponent();

        _code = code;
    }
}