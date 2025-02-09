using ControlzEx.Theming;
using MsSql.ClassGenerator.Business;
using MsSql.ClassGenerator.Common;
using System.Windows;
using ICSharpCode.AvalonEdit;
using System.Windows.Media;

namespace MsSql.ClassGenerator.Ui;

/// <summary>
/// Provides several helper methods for the UI.
/// </summary>
internal static class UiHelper
{
    #region Color theming
    /// <summary>
    /// Sets the color scheme
    /// Init the avalon editor.
    /// </summary>
    /// <param name="colorScheme">The scheme which should be set</param>
    /// <returns>The awaitable task.</returns>
    public static async Task SetColorThemeAsync(string colorScheme)
    {
        var customColors = await SettingsManager.LoadColorsAsync();
        if (customColors.Select(s => s.Name).Contains(colorScheme, StringComparer.OrdinalIgnoreCase))
        {
            var customColor = customColors.FirstOrDefault(f => f.Name.Equals(colorScheme, StringComparison.OrdinalIgnoreCase));
            if (customColor == null)
                return;

            var newTheme = new Theme("AppTheme", "AppTheme", "Dark", customColor.ColorValue.ToHex(), customColor.ColorValue,
                new SolidColorBrush(customColor.ColorValue), true, false);
            ThemeManager.Current.ChangeTheme(Application.Current, newTheme);
        }
        else
        {
            var schemeName =
                ThemeManager.Current.ColorSchemes.FirstOrDefault(f =>
                    f.Equals(colorScheme, StringComparison.OrdinalIgnoreCase)) ?? "Cyan";
            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, schemeName);
        }
    }
    #endregion

    /// <summary>
    /// Init the avalon editor.
    /// </summary>
    /// <param name="editor">The editor.</param>
    public static void InitAvalonEditor(this TextEditor editor)
    {
        editor.Options.HighlightCurrentLine = true;
        editor.Options.ConvertTabsToSpaces = true; // We hate tabs...
        editor.Foreground = new SolidColorBrush(Colors.White);
    }
}