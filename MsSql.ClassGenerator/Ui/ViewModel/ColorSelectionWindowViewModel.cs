using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MsSql.ClassGenerator.Model;

namespace MsSql.ClassGenerator.Ui.ViewModel;

internal sealed partial class ColorSelectionWindowViewModel : ViewModelBase
{
    /// <summary>
    /// The action to close the window.
    /// </summary>
    private Action<bool>? _closeWindow;

    /// <summary>
    /// Gets or sets the list with the available colors.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ColorEntry> _colors = [];

    /// <summary>
    /// Gets or sets the selected color.
    /// </summary>
    [ObservableProperty]
    private ColorEntry? _selectedColor;

    public async void InitViewModel(Action<bool> closeWindow)
    {
        _closeWindow = closeWindow;
    }
}