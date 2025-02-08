using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsSql.ClassGenerator.Business;
using MsSql.ClassGenerator.Core.Data;
using MsSql.ClassGenerator.Model;
using System.Collections.ObjectModel;

namespace MsSql.ClassGenerator.Ui.ViewModel;

internal sealed partial class ServerEntryWindowViewModel : ViewModelBase
{
    /// <summary>
    /// The default database entry
    /// </summary>
    private const string DefaultEntry = "<Select database>";

    /// <summary>
    /// The action to close the window
    /// </summary>
    private Action<bool>? _closeWindow;

    /// <summary>
    /// The selected server
    /// </summary>
    [ObservableProperty]
    private ServerEntry _selectedServer = new();

    /// <summary>
    /// The list with the databases
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _databaseList = [];

    /// <summary>
    /// Gets or sets the selected server.
    /// </summary>
    [ObservableProperty]
    private string _selectedDatabase = string.Empty;

    /// <summary>
    /// Occurs when the user selects another database.
    /// </summary>
    /// <param name="value">The selected database.</param>
    partial void OnSelectedDatabaseChanged(string value)
    {
        AutoConnectEnabled = !string.IsNullOrWhiteSpace(value) && !value.Equals(DefaultEntry);
    }

    /// <summary>
    /// The value which indicates if the select button is enabled
    /// </summary>
    [ObservableProperty]
    private bool _buttonSelectedEnabled;

    /// <summary>
    /// The value which indicates if the auto connect checkbox is enabled
    /// </summary>
    [ObservableProperty]
    private bool _autoConnectEnabled;

    /// <summary>
    /// Init the view model.
    /// </summary>
    /// <param name="server">The server which should be edited.</param>
    /// <param name="closeWindow">The action to close the window.</param>
    public void InitViewModel(ServerEntry server, Action<bool> closeWindow)
    {
        SelectedServer = server;
        _closeWindow = closeWindow;
    }

    /// <summary>
    /// Occurs when the user hits the connect button and creates a connection to the desired server to load all available databases.
    /// </summary>
    /// <returns>The awaitable task.</returns>
    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedServer.Name))
            return;

        DatabaseList.Clear();

        try
        {
            var baseRepo = new BaseRepo(SelectedServer.Name);
            var databases = await baseRepo.LoadDatabasesAsync();

            DatabaseList = [DefaultEntry];
            foreach (var database in databases)
            {
                DatabaseList.Add(database);
            }

            ButtonSelectedEnabled = databases.Count > 0;

            if (!string.IsNullOrEmpty(SelectedServer.DefaultDatabase))
            {
                SelectedDatabase = DatabaseList.FirstOrDefault(f => f.Equals(SelectedServer.DefaultDatabase)) ??
                                   DefaultEntry;
            }
            else
                SelectedDatabase = DefaultEntry;
        }
        catch (Exception ex)
        {
            LogError(ex);
            ShowInfoMessage("Error: Can't connect to server!");
        }
    }

    /// <summary>
    /// Occurs when the user hits the OK button.
    /// </summary>
    /// <remarks>
    /// Saves the current server and closes the window.
    /// </remarks>
    [RelayCommand]
    private async Task SetDataAsync()
    {
        if (string.IsNullOrEmpty(SelectedServer.Name))
        {
            ShowInfoMessage("Please specify a server...");
            return;
        }

        // Check name / database
        if (await SettingsManager.ServerExistsAsync(SelectedServer))
        {
            ShowInfoMessage("Server / Database already exists...");
            return;
        }

        SelectedServer.DefaultDatabase = SelectedDatabase;

        // Save the server if it's new
        await SettingsManager.AddServerAsync(SelectedServer);

        _closeWindow?.Invoke(true);
    }

    /// <summary>
    /// Closes the window.
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        _closeWindow?.Invoke(false);
    }
}