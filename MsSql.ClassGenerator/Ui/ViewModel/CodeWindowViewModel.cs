using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsSql.ClassGenerator.Core.Model;

namespace MsSql.ClassGenerator.Ui.ViewModel;

/// <summary>
/// Interaction logic for <see cref="View.CodeWindow"/>.
/// </summary>
internal sealed partial class CodeWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Contains the ef key code.
    /// </summary>
    private EfKeyCodeResult _efKeyCode = new();

    /// <summary>
    /// Gets or sets the info.
    /// </summary>
    [ObservableProperty]
    private string _info = "Info";

    /// <summary>
    /// Init the view model.
    /// </summary>
    /// <param name="efKeyCode">The ef key code.</param>
    public void InitViewModel(EfKeyCodeResult efKeyCode)
    {
        _efKeyCode = efKeyCode;

        Info = $"Tables with multiple keys: {efKeyCode.TableCount:N0}";
    }

    /// <summary>
    /// Occurs when the user hits the copy button.
    /// </summary>
    /// <remarks>
    /// Copies the content of the editor to the clipboard.
    /// </remarks>
    [RelayCommand]
    private void CopyCode()
    {
        if (_efKeyCode.IsEmpty)
            return;

        CopyToClipboard(_efKeyCode.Code);
    }
}