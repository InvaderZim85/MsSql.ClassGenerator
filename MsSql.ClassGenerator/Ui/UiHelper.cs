using ICSharpCode.AvalonEdit;
using System.Windows.Media;

namespace MsSql.ClassGenerator.Ui;

/// <summary>
/// Provides several helper methods for the UI.
/// </summary>
internal static class UiHelper
{

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