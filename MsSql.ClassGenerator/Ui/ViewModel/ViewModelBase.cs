using CommunityToolkit.Mvvm.ComponentModel;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Taskbar;
using Serilog;
using System.Runtime.CompilerServices;
using System.Windows;
using Timer = System.Timers.Timer;

namespace MsSql.ClassGenerator.Ui.ViewModel;

/// <summary>
/// Provides the base functions of a view model
/// </summary>
internal class ViewModelBase : ObservableObject
{
    /// <summary>
    /// The different error types
    /// </summary>
    public enum ErrorMessageType
    {
        /// <summary>
        /// General error
        /// </summary>
        General,

        /// <summary>
        /// Error while saving
        /// </summary>
        Load,

        /// <summary>
        /// Error while loading
        /// </summary>
        Save
    }

    /// <summary>
    /// The instance of the mah apps dialog coordinator
    /// </summary>
    private readonly IDialogCoordinator _dialogCoordinator;

    /// <summary>
    /// Contains the instance of the taskbar manager
    /// </summary>
    private static TaskbarManager? _taskbarInstance;

    /// <summary>
    /// The message timer
    /// </summary>
    private readonly Timer _messageTimer = new(TimeSpan.FromSeconds(10).TotalMilliseconds);

    /// <summary>
    /// Backing field for <see cref="InfoMessage"/>
    /// </summary>
    private string _infoMessage = string.Empty;

    /// <summary>
    /// Gets or sets the message which should be shown
    /// </summary>
    public string InfoMessage
    {
        get => _infoMessage;
        private set => SetProperty(ref _infoMessage, value);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ViewModelBase"/>
    /// </summary>
    protected ViewModelBase()
    {
        _dialogCoordinator = DialogCoordinator.Instance;

        _messageTimer.Elapsed += (_, _) =>
        {
            InfoMessage = string.Empty;
            _messageTimer.Stop();
        };

        // Init the taskbar
        if (TaskbarManager.IsPlatformSupported)
            _taskbarInstance = TaskbarManager.Instance;
    }

    /// <summary>
    /// Gets the main window
    /// </summary>
    /// <returns>The main window</returns>
    protected static Window GetMainWindow()
    {
        return Application.Current.MainWindow!;
    }

    /// <summary>
    /// Shows a message dialog
    /// </summary>
    /// <param name="title">The title of the dialog</param>
    /// <param name="message">The message of the dialog</param>
    /// <returns>The awaitable task</returns>
    protected async Task ShowMessageAsync(string title, string message)
    {
        SetTaskbarPause(true);

        await _dialogCoordinator.ShowMessageAsync(this, title, message);

        SetTaskbarPause(false);
    }

    /// <summary>
    /// Shows a question dialog with two buttons
    /// </summary>
    /// <param name="title">The title of the dialog</param>
    /// <param name="message">The message of the dialog</param>
    /// <param name="okButtonText">The text of the ok button (optional)</param>
    /// <param name="cancelButtonText">The text of the cancel button (optional)</param>
    /// <returns>The dialog result</returns>
    protected async Task<MessageDialogResult> ShowQuestionAsync(string title, string message, string okButtonText = "OK",
        string cancelButtonText = "Cancel")
    {
        SetTaskbarPause(true);

        var result = await _dialogCoordinator.ShowMessageAsync(this, title, message,
            MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
            {
                AffirmativeButtonText = okButtonText,
                NegativeButtonText = cancelButtonText
            });

        SetTaskbarPause(false);

        return result;
    }

    /// <summary>
    /// Shows an error message and logs the exception
    /// </summary>
    /// <param name="ex">The exception which was thrown</param>
    /// <param name="messageType">The desired message type</param>
    /// <param name="caller">The name of the method, which calls this method. Value will be filled automatically</param>
    /// <returns>The awaitable task</returns>
    protected async Task ShowErrorAsync(Exception ex, ErrorMessageType messageType,
        [CallerMemberName] string caller = "")
    {
        SetTaskbarError(true);

        LogError(ex, caller);

        var message = messageType switch
        {
            ErrorMessageType.Load => "An error has occurred while loading the data.",
            ErrorMessageType.Save => "An error has occurred while saving the data.",
            _ => "An error has occurred."
        };

        await _dialogCoordinator.ShowMessageAsync(this, "Error", message);

        SetTaskbarError(false);
    }

    /// <summary>
    /// Logs an error
    /// </summary>
    /// <param name="ex">The exception which was thrown</param>
    /// <param name="caller">The name of the method, which calls this method. Value will be filled automatically</param>
    protected static void LogError(Exception ex, [CallerMemberName] string caller = "")
    {
        Log.Error(ex, "An error has occurred. Caller: {caller}", caller);
    }

    /// <summary>
    /// Shows a progress dialog
    /// </summary>
    /// <param name="title">The title of the dialog</param>
    /// <param name="message">The message of the dialog</param>
    /// <param name="ctSource">The cancellation token source (optional)</param>
    /// <returns>The dialog controller</returns>
    protected async Task<ProgressDialogController> ShowProgressAsync(string title, string message, CancellationTokenSource? ctSource = default)
    {
        SetTaskbarIndeterminate(true);

        var controller = await _dialogCoordinator.ShowProgressAsync(this, title, message, ctSource != null);
        controller.SetIndeterminate();

        if (ctSource != null)
        {
            controller.Canceled += (_, _) => ctSource.Cancel();
        }

        controller.Closed += (_, _) =>
        {
            // Reset the taskbar when the controller was closed
            SetTaskbarIndeterminate(false);
        };

        return controller;
    }

    /// <summary>
    /// Shows an info message for 10 seconds
    /// </summary>
    /// <param name="message">The message which should be shown</param>
    protected void ShowInfoMessage(string message)
    {
        InfoMessage = message;
        _messageTimer.Start();
    }

    /// <summary>
    /// Copies the content to the clipboard
    /// </summary>
    /// <param name="content">The content which should be copied</param>
    protected static void CopyToClipboard(string content)
    {
        Clipboard.SetText(content);
    }

    #region Taskbar

    /// <summary>
    /// Sets the taskbar into an indeterminate state
    /// </summary>
    /// <param name="enable"><see langword="true"/> to set the indeterminate state, otherwise <see langword="false"/></param>
    public static void SetTaskbarIndeterminate(bool enable)
    {
        SetTaskbarState(enable, TaskbarProgressBarState.Indeterminate);
    }

    /// <summary>
    /// Sets the taskbar into an error state
    /// </summary>
    /// <param name="enable"><see langword="true"/> to set the indeterminate state, otherwise <see langword="false"/></param>
    public static void SetTaskbarError(bool enable)
    {
        SetTaskbarState(enable, TaskbarProgressBarState.Error);
    }

    /// <summary>
    /// Sets the taskbar into an pause state
    /// </summary>
    /// <param name="enable"><see langword="true"/> to set the indeterminate state, otherwise <see langword="false"/></param>
    public static void SetTaskbarPause(bool enable)
    {
        SetTaskbarState(enable, TaskbarProgressBarState.Paused);
    }

    /// <summary>
    /// Sets the taskbar state
    /// </summary>
    /// <param name="enabled"><see langword="true"/> to set the state, <see langword="false"/> to set <see cref="TaskbarProgressBarState.NoProgress"/></param>
    /// <param name="state">The desired state</param>
    private static void SetTaskbarState(bool enabled, TaskbarProgressBarState state)
    {
        try
        {
            _taskbarInstance?.SetProgressState(enabled ? state : TaskbarProgressBarState.NoProgress);
            switch (enabled)
            {
                case true when state != TaskbarProgressBarState.Indeterminate:
                    _taskbarInstance?.SetProgressValue(100, 100);
                    break;
                case false:
                    _taskbarInstance?.SetProgressValue(0, 0);
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Can't change taskbar state");
        }
    }
    #endregion
}