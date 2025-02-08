using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MsSql.ClassGenerator.Business;
using MsSql.ClassGenerator.Common;
using MsSql.ClassGenerator.Core.Business;
using MsSql.ClassGenerator.Core.Common;
using MsSql.ClassGenerator.Core.Data;
using MsSql.ClassGenerator.Model;
using MsSql.ClassGenerator.Model.Internal;
using MsSql.ClassGenerator.Ui.View;
using System.Collections.ObjectModel;

namespace MsSql.ClassGenerator.Ui.ViewModel;

/// <summary>
/// Interaction logic for the <see cref="View.MainWindow"/>.
/// </summary>
internal sealed partial class MainWindowViewModel : ViewModelBase
{
    #region Variables

    /// <summary>
    /// Contains the provided arguments.
    /// </summary>
    private Arguments _arguments = new();

    /// <summary>
    /// The instance for the interaction with the tables.
    /// </summary>
    private TableManager? _tableManager;

    #endregion

    #region Properties

    #region Connection

    /// <summary>
    /// Gets the list with the server.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ServerEntry> _serverList = [];

    /// <summary>
    /// Gets or sets the selected server.
    /// </summary>
    [ObservableProperty]
    private ServerEntry? _selectedServer;

    /// <summary>
    /// Gets or sets the list with the databases.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _databaseList = [];

    /// <summary>
    /// Gets or sets the selected database.
    /// </summary>
    [ObservableProperty]
    private string _selectedDatabase = string.Empty;
    #endregion

    #region Options

    /// <summary>
    /// Gets or sets the path of the output directory.
    /// </summary>
    [ObservableProperty]
    private string _outputDirectory = string.Empty;

    /// <summary>
    /// Gets or sets the value which indicates whether the output directory should be cleaned before a new export.
    /// </summary>
    [ObservableProperty]
    private bool _cleanExportDirectory;

    /// <summary>
    /// Gets or sets the desired namespace.
    /// </summary>
    [ObservableProperty]
    private string _namespace = string.Empty;

    /// <summary>
    /// Gets or sets the list with the available modifier.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _modifierList = [];

    /// <summary>
    /// Gets or sets the selected modifier.
    /// </summary>
    [ObservableProperty]
    private string _selectedModifier = "public";

    /// <summary>
    /// Gets or sets the value which indicates whether the <c>sealed</c> modifier should be added to the classes.
    /// </summary>
    [ObservableProperty]
    private bool _addSealed;

    /// <summary>
    /// Gets or sets the value which indicates whether the classes can be used for <i>Entity Framework Core</i>.
    /// </summary>
    [ObservableProperty]
    private bool _dbModel;

    /// <summary>
    /// Gets or sets the value which indicates whether a column attribute should be added.
    /// </summary>
    [ObservableProperty]
    private bool _addColumnAttribute;

    /// <summary>
    /// Gets or sets the value which indicates whether a backing field should be generated.
    /// </summary>
    [ObservableProperty]
    private bool _addBackingField;

    /// <summary>
    /// Gets or sets the value which indicates whether the <c>SetProperty</c> method should be added.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: If this options is selected, the <see cref="AddBackingField"/> option will be automatically selected.
    /// </remarks>
    [ObservableProperty]
    private bool _addSetProperty;

    /// <summary>
    /// Occurs when the user changes the value of the <see cref="AddSetProperty"/> property.
    /// </summary>
    /// <param name="value">The new value.</param>
    partial void OnAddSetPropertyChanged(bool value)
    {
        if (value)
            AddBackingField = true;
    }

    /// <summary>
    /// Gets or sets the value which indicates whether a summary should be added to the class and each property.
    /// </summary>
    [ObservableProperty]
    private bool _addSummary;

    /// <summary>
    /// Gets or sets the value which indicates whether the table name should be added to the class summary.
    /// </summary>
    [ObservableProperty]
    private bool _addTableToSummary;
    #endregion

    #region Tables / Columns

    /// <summary>
    /// Gets the list with the tables.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<TableColumnDto> _tables = [];

    /// <summary>
    /// Gets or sets the selected table.
    /// </summary>
    [ObservableProperty]
    private TableColumnDto? _selectedTable;

    /// <summary>
    /// Occurs when the user selects another table.
    /// </summary>
    /// <param name="value">The selected table.</param>
    partial void OnSelectedTableChanged(TableColumnDto? value)
    {
        Columns = value == null ? [] : value.Columns
            .OrderBy(o => o.Position)
            .ToObservableCollection();
    }

    /// <summary>
    /// Gets or sets the list with the columns.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<TableColumnDto> _columns = [];
    #endregion

    #region Behaviour

    /// <summary>
    /// Gets or sets the value which indicates whether a connection was created.
    /// </summary>
    [ObservableProperty]
    private bool _isConnected;

    #endregion

    #endregion

    /// <summary>
    /// Init the view model.
    /// </summary>
    /// <param name="arguments">The provided arguments.</param>
    public async void InitViewModel(Arguments arguments)
    {
        try
        {
            _arguments = arguments;
            ModifierList = Helper.GetModifierList().ToObservableCollection();
            SelectedModifier = ModifierList.FirstOrDefault() ?? "public";

            await LoadServerListAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Load);
        }
    }

    /// <summary>
    /// Loads and sets the server list.
    /// </summary>
    /// <returns>The awaitable task.</returns>
    private async Task LoadServerListAsync()
    {
        var serverList = await SettingsManager.LoadServerListAsync();
        ServerList = serverList.ToObservableCollection();
    }

    #region Commands

    #region Connection

    /// <summary>
    /// Occurs when the user hits the connect button.
    /// </summary>
    /// <returns>The awaitable task.</returns>
    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (SelectedServer == null)
            return;

        var controller = await ShowProgressAsync("Please wait",
            $"Please wait while connecting with '{SelectedServer.Name}'...");

        try
        {
            var baseRepo = new BaseRepo(SelectedServer.Name);
            var databases = await baseRepo.LoadDatabasesAsync();

            DatabaseList = databases.ToObservableCollection();

            if (!string.IsNullOrWhiteSpace(SelectedServer.DefaultDatabase))
            {
                SelectedDatabase = DatabaseList.FirstOrDefault(f => f.Equals(SelectedServer.DefaultDatabase)) ??
                                   string.Empty;
            }

            if (SelectedServer.AutoConnect)
                await SelectAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.General);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Occurs when the user hits the select button.
    /// </summary>
    /// <remarks>
    /// Tries to connect to the selected server / database and loads all available tables.
    /// </remarks>
    /// <returns>The awaitable task.</returns>
    [RelayCommand]
    private async Task SelectAsync()
    {
        if (SelectedServer == null || string.IsNullOrWhiteSpace(SelectedDatabase))
            return;

        var controller = await ShowProgressAsync("Please wait",
            $"Please wait while connecting to database '{SelectedDatabase}'...");

        try
        {
            _tableManager = new TableManager(SelectedServer.Name, SelectedDatabase);
            _tableManager.ProgressEvent += (_, msg) => controller.SetMessage(msg);

            // Load the tables
            await _tableManager.LoadTablesAsync();

            // Set the tables
            Tables = _tableManager.Tables
                .Select(s => new TableColumnDto(s))
                .OrderBy(o => o.Name)
                .ToObservableCollection();
            SelectedTable = Tables.FirstOrDefault();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.General);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Occurs when the user hits the add button.
    /// </summary>
    /// <remarks>
    /// Opens the server window.
    /// </remarks>
    /// <returns>The awaitable task.</returns>
    [RelayCommand]
    private async Task AddConnectionAsync()
    {
        var window = new ServerEntryWindow()
        {
            Owner = GetMainWindow()
        };

        if (window.ShowDialog() != true)
            return;

        // Reload the server list
        await LoadServerListAsync();

        SelectedServer = ServerList.FirstOrDefault(f => f.Name.Equals(window.Server.Name) &&
                                                        f.DefaultDatabase.Equals(window.Server.DefaultDatabase));
    }

    /// <summary>
    /// Occurs when the user hits the edit button.
    /// </summary>
    /// <remarks>
    /// Opens the server window.
    /// </remarks>
    /// <returns>The awaitable task.</returns>
    [RelayCommand]
    private async Task EditConnectionAsync()
    {
        if (SelectedServer == null)
            return;

        var window = new ServerEntryWindow(SelectedServer)
        {
            Owner = GetMainWindow()
        };

        if (window.ShowDialog() != true)
            return;

        // Reload the server list
        await LoadServerListAsync();

        SelectedServer = ServerList.FirstOrDefault(f => f.Name.Equals(window.Server.Name) &&
                                                        f.DefaultDatabase.Equals(window.Server.DefaultDatabase));
    }

    #endregion

    #region Options

    /// <summary>
    /// Occurs when the user hits the browse button.
    /// </summary>
    /// <remarks>
    /// Opens a folder browse dialog to select the desired output directory.
    /// </remarks>
    [RelayCommand]
    private void BrowseOutputDirectory()
    {
        var dialog = new OpenFolderDialog();
        if (dialog.ShowDialog() != true) 
            return;

        OutputDirectory = dialog.FolderName;
    }

    #endregion

    #endregion
}
