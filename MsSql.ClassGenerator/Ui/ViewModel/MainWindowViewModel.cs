using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32;
using MsSql.ClassGenerator.Business;
using MsSql.ClassGenerator.Common;
using MsSql.ClassGenerator.Common.Enums;
using MsSql.ClassGenerator.Core.Business;
using MsSql.ClassGenerator.Core.Common;
using MsSql.ClassGenerator.Core.Data;
using MsSql.ClassGenerator.Core.Model;
using MsSql.ClassGenerator.Model;
using MsSql.ClassGenerator.Ui.View;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;

namespace MsSql.ClassGenerator.Ui.ViewModel;

/// <summary>
/// Interaction logic for the <see cref="View.MainWindow"/>.
/// </summary>
internal sealed partial class MainWindowViewModel : ViewModelBase
{
    #region Variables
    /// <summary>
    /// Gets the default app title.
    /// </summary>
    private const string AppTitleDefault = "MsSql.ClassGenerator";

    /// <summary>
    /// The instance for the interaction with the tables.
    /// </summary>
    private TableManager? _tableManager;

    /// <summary>
    /// Contains the generated EF Key Code.
    /// </summary>
    private EfKeyCodeResult _efKeyCode = new();

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
    /// The list with the tables.
    /// </summary>
    /// <remarks>
    /// This list is needed for the filtering.
    /// </remarks>
    private List<TableColumnDto> _originTables = [];

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
        _originColumns = value == null ? [] : value.Columns;

        FilterColumns();
    }

    /// <summary>
    /// The list with the columns.
    /// </summary>
    /// <remarks>
    /// This list is needed for the filtering.
    /// </remarks>
    private List<TableColumnDto> _originColumns = [];

    /// <summary>
    /// Gets or sets the list with the columns.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<TableColumnDto> _columns = [];

    /// <summary>
    /// Gets or sets the table filter.
    /// </summary>
    [ObservableProperty]
    private string _filterTable = string.Empty;

    /// <summary>
    /// Occurs when the user changes the value of the table filter.
    /// </summary>
    /// <param name="value">The new value.</param>
    partial void OnFilterTableChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            FilterTables();
    }

    /// <summary>
    /// Gets or sets the column filter.
    /// </summary>
    [ObservableProperty]
    private string _filterColumn = string.Empty;

    /// <summary>
    /// Occurs when the user changed the value of the column filter.
    /// </summary>
    /// <param name="value">The new value.</param>
    partial void OnFilterColumnChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            FilterColumns();
    }

    /// <summary>
    /// Gets or sets the table header.
    /// </summary>
    [ObservableProperty]
    private string _headerTables = "Tables";

    /// <summary>
    /// Gets or sets the column header.
    /// </summary>
    [ObservableProperty]
    private string _headerColumns = "Columns";
    #endregion

    #region Behaviour / Various

    /// <summary>
    /// Gets or sets the value which indicates whether a connection was created.
    /// </summary>
    [ObservableProperty]
    private bool _isConnected;

    /// <summary>
    /// Gets or sets the application title.
    /// </summary>
    [ObservableProperty]
    private string _appTitle = AppTitleDefault;

    /// <summary>
    /// Gets or sets the value which indicates whether the button "Show EF Key Code" should be enabled.
    /// </summary>
    [ObservableProperty]
    private bool _buttonShowEfKeyCodeEnabled;

    /// <summary>
    /// Gets or sets the information (connection status).
    /// </summary>
    [ObservableProperty]
    private string _info = "Not connected.";

    /// <summary>
    /// Gets or sets the version number.
    /// </summary>
    [ObservableProperty]
    private string _versionInfo = "Version";

    /// <summary>
    /// Gets or sets the visibility of the update button.
    /// </summary>
    [ObservableProperty]
    private Visibility _buttonUpdateVisibility = Visibility.Hidden;

    /// <summary>
    /// Gets or sets the update info.
    /// </summary>
    [ObservableProperty]
    private string _updateInfo = "Update available!";

    #endregion

    #endregion

    /// <summary>
    /// Init the view model.
    /// </summary>
    /// <param name="arguments">The provided arguments.</param>
    /// <returns>The awaitable task.</returns>
    public async Task InitViewModelAsync(Arguments arguments)
    {
        try
        {
            // Set the version
            VersionInfo = $"v{Assembly.GetExecutingAssembly().GetName().Version}";

            // Start the update check
            CheckUpdate();

            // Load the data
            ModifierList = Helper.GetModifierList().ToObservableCollection();
            SelectedModifier = ModifierList.FirstOrDefault() ?? "public";

            await LoadServerListAsync();

            // Set the options
            await LoadOptionsAsync();

            if (arguments.Empty)
                return;

            await AutoConnectAsync(arguments);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Load);
        }
    }

    /// <summary>
    /// Loads the saved options and sets the values accordingly.
    /// </summary>
    /// <returns>The awaitable task.</returns>
    private async Task LoadOptionsAsync()
    {
        var options = await SettingsManager.LoadOptionsAsync();

        CleanExportDirectory = options.CleanExportDirectory;
        SelectedModifier = ModifierList.FirstOrDefault(f => f.Equals(options.Modifier)) ??
                           ModifierList.FirstOrDefault() ?? "public";
        AddSealed = options.AddSealed;
        DbModel = options.DbModel;
        AddColumnAttribute = options.AddColumnAttribute;
        AddBackingField = options.AddBackingField;
        AddSetProperty = options.AddSetProperty;
        AddSummary = options.AddSummary;
        AddTableToSummary = options.AddTableToSummary;
    }

    /// <summary>
    /// Tries to connect to the specified server / database.
    /// </summary>
    /// <param name="arguments">The arguments with th needed information.</param>
    /// <returns>The awaitable task.</returns>
    private async Task AutoConnectAsync(Arguments arguments)
    {
        // Get the desired server
        SelectedServer = ServerList.FirstOrDefault(f =>
            f.Name.Equals(arguments.Server, StringComparison.InvariantCultureIgnoreCase) &&
            f.DefaultDatabase.Equals(arguments.Database, StringComparison.InvariantCultureIgnoreCase));

        if (SelectedServer == null)
        {
            var newServer =new ServerEntry
            {
                Name = arguments.Server,
                DefaultDatabase = arguments.Database,
                AutoConnect = true
            };

            // Save the new server
            await SettingsManager.AddServerAsync(newServer);

            ServerList.Add(newServer);
            SelectedServer = newServer;
        }

        await ConnectAsync();
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
        Reset();

        IsConnected = false;

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

            SetAppInfo();
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
        IsConnected = false;

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
            _originTables = [.._tableManager.Tables.Select(s => new TableColumnDto(s)).OrderBy(o => o.Name)];
            
            FilterTables();

            SetAppInfo();

            IsConnected = true;
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

        try
        {
            // Reload the server list
            await LoadServerListAsync();

            SelectedServer = ServerList.FirstOrDefault(f => f.Name.Equals(window.Server.Name) &&
                                                            f.DefaultDatabase.Equals(window.Server.DefaultDatabase));
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
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

        try
        {
            // Update the server list
            await SettingsManager.SaveServerListAsync([..ServerList]);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
    }

    /// <summary>
    /// Occurs when the user hits the delete button.
    /// </summary>
    /// <remarks>
    /// Deletes the selected server connection.
    /// </remarks>
    /// <returns>The awaitable task.</returns>
    [RelayCommand]
    private async Task DeleteConnectionAsync()
    {
        if (SelectedServer == null)
            return;

        if (await ShowQuestionAsync("Delete", $"Do you really want to delete the entry '{SelectedServer.Name}'?", "Yes",
                "No") != MessageDialogResult.Affirmative)
            return;

        try
        {
            await SettingsManager.DeleteServerAsync(SelectedServer);

            // Remove the selection
            SelectedServer = null;

            // Reload the server list
            await LoadServerListAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
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

    #region Table / Columns


    /// <summary>
    /// Occurs when the user hits enter while the filter textbox of the tables has the focus.
    /// </summary>
    /// <remarks>
    /// Filters the tables.
    /// </remarks>
    [RelayCommand]
    private void FilterTables()
    {
        Tables = FilterValues(_originTables, FilterTable);

        SelectedTable = Tables.FirstOrDefault();

        HeaderTables = Tables.Count == 1 ? "1 Table" : $"{Tables.Count} Tables";
    }

    /// <summary>
    /// Occurs when the user hits enter while the filter textbox of the tables has the focus.
    /// </summary>
    /// <remarks>
    /// Filters the columns.
    /// </remarks>
    [RelayCommand]
    private void FilterColumns()
    {
        Columns = FilterValues(_originColumns, FilterColumn);

        HeaderColumns = Columns.Count == 1 ? "1 Column" : $"{Columns.Count} Columns";
    }

    /// <summary>
    /// Filters the list according to the specified string.
    /// </summary>
    /// <param name="source">The original source list.</param>
    /// <param name="filter">The desired filter.</param>
    /// <returns>The observable collection</returns>
    private static ObservableCollection<TableColumnDto> FilterValues(List<TableColumnDto> source, string filter)
    {
        var tmpList = string.IsNullOrWhiteSpace(filter)
            ? source
            : [.. source.Where(w => w.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
                                (!string.IsNullOrWhiteSpace(w.Schema) && w.Schema.Contains(filter,
                                    StringComparison.InvariantCultureIgnoreCase)))];

        return tmpList.FirstOrDefault()?.Type == EntryType.Table
            ? tmpList.OrderBy(o => o.Name).ToObservableCollection()
            : tmpList.OrderBy(o => o.Position).ToObservableCollection();
    }
    #endregion

    #region Generation

    /// <summary>
    /// Occurs when the user hits the all / none selection button below the tables.
    /// </summary>
    /// <remarks>
    /// Sets the <i>Use</i> value of all tables.
    /// </remarks>
    /// <param name="type">The desired type.</param>
    [RelayCommand]
    private void SetTableSelection(SelectionType type)
    {
        SetSelection(_originTables, type);
    }

    /// <summary>
    /// Occurs when the user hits the all / none selection button below the tables.
    /// </summary>
    /// <remarks>
    /// Sets the <i>Use</i> value of all tables.
    /// </remarks>
    /// <param name="type">The desired type.</param>
    [RelayCommand]
    private void SetColumnSelection(SelectionType type)
    {
        SetSelection(_originColumns, type);
    }

    /// <summary>
    /// Sets the <i>Use</i> value of the entries.
    /// </summary>
    /// <param name="source">The source list.</param>
    /// <param name="type">The desired selection type.</param>
    private static void SetSelection(IEnumerable<TableColumnDto> source, SelectionType type)
    {
        foreach (var entry in source)
        {
            entry.Use = type == SelectionType.All;
        }
    }

    /// <summary>
    /// Occurs when the user hits the generate button (bottom left).
    /// </summary>
    /// <remarks>
    /// Starts the class generation for all desired tables / columns.
    /// </remarks>
    /// <returns>The awaitable task.</returns>
    [RelayCommand]
    private async Task GenerateClassesAsync()
    {
        ButtonShowEfKeyCodeEnabled = false;

        if (!ValidateInput(out var errorMessage))
        {
            await ShowMessageAsync("Generation", errorMessage);
            return;
        }

        var controller = await ShowProgressAsync("Generation", "Please wait while the classes are generated...");

        try
        {
            var options = GetOptions();
            var tables = _originTables.ToTableEntries();

            var classManager = new ClassManager();

            classManager.ProgressEvent += (_, msg) => controller.SetMessage(msg);

            await classManager.GenerateClassAsync(options, tables);

            // Save the current options
            await SaveOptionsAsync(options);

            ButtonShowEfKeyCodeEnabled = !_efKeyCode.IsEmpty;
            _efKeyCode = classManager.EfKeyCode;
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
    /// Occurs when the user hits the "Show EF Key Code" button.
    /// </summary>
    /// <remarks>
    /// Opens the code window with the generated code.
    /// </remarks>
    [RelayCommand]
    private void ShowEfKeyCode()
    {
        if (_efKeyCode.IsEmpty)
            return;

        var codeWindow = new CodeWindow(_efKeyCode)
        {
            Owner = GetMainWindow()
        };

        codeWindow.ShowDialog();
    }

    /// <summary>
    /// Validates the input.
    /// </summary>
    /// <param name="errorMessage">The error message which can be shown.</param>
    /// <returns><see langword="true"/> when everything is ok, otherwise <see langword="false"/>.</returns>
    private bool ValidateInput(out string errorMessage)
    {
        errorMessage = string.Empty;
        var errorList = new List<string>();

        if (string.IsNullOrWhiteSpace(OutputDirectory))
            errorList.Add("- Output directory is missing.");

        if (string.IsNullOrWhiteSpace(Namespace))
            errorList.Add("- Namespace is missing.");

        if (errorList.Count == 0) 
            return true;

        errorList.Insert(0, "One ore more settings are missing:");
        errorList.Insert(1, string.Empty);
        errorMessage = string.Join(Environment.NewLine, errorList);
        return false;

    }

    /// <summary>
    /// Gets the options which are needed for the class generation.
    /// </summary>
    /// <returns>The options.</returns>
    private ClassGeneratorOptions GetOptions()
    {
        return new ClassGeneratorOptions
        {
            Output = OutputDirectory,
            Namespace = Namespace,
            SealedClass = AddSealed,
            Modifier = SelectedModifier,
            DbModel = DbModel,
            AddColumnAttribute = AddColumnAttribute,
            WithBackingField = AddBackingField,
            AddSetProperty = AddSetProperty,
            AddSummary = AddSummary,
            AddTableNameToClassSummary = AddTableToSummary,
            EmptyOutputDirectoryBeforeExport = CleanExportDirectory
        };
    }

    /// <summary>
    /// Saves the current options.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <returns>The awaitable task.</returns>
    private static async Task SaveOptionsAsync(ClassGeneratorOptions options)
    {
        // Convert the options in the desired type.
        var tmpOptions = new OptionDto
        {
            CleanExportDirectory = options.EmptyOutputDirectoryBeforeExport,
            Modifier = options.Modifier,
            AddSealed = options.SealedClass,
            DbModel = options.DbModel,
            AddColumnAttribute = options.AddColumnAttribute,
            AddBackingField = options.WithBackingField,
            AddSetProperty = options.AddSetProperty,
            AddSummary = options.AddSummary,
            AddTableToSummary = options.AddTableNameToClassSummary
        };

        await SettingsManager.SaveOptionsAsync(tmpOptions);
    }
    #endregion

    #region Various

    /// <summary>
    /// Occurs when the user hits the update button (title bar).
    /// </summary>
    /// <remarks>
    /// Opens the GitHub page with the latest version.
    /// </remarks>
    [RelayCommand]
    private static void OpenGitHubPage()
    {
        Helper.OpenLink(UpdateHelper.GitHupUrl);
    }
    #endregion

    #endregion

    #region Various

    /// <summary>
    /// Sets the app title.
    /// </summary>
    private void SetAppInfo()
    {
        var info = string.Empty;

        if (SelectedServer != null)
            info += $"Server: '{SelectedServer.Name}'";

        if (!string.IsNullOrEmpty(SelectedDatabase))
            info += $" | Database: '{SelectedDatabase}'";

        AppTitle = $"{AppTitleDefault} -  {info}";
        Info = $"Connected - {info}";
    }

    /// <summary>
    /// Resets the settings / options.
    /// </summary>
    private void Reset()
    {
        OutputDirectory = string.Empty;
        Namespace = string.Empty;
        Columns.Clear();
        Tables.Clear();
    }

    /// <summary>
    /// Checks if a new version is available
    /// </summary>
    private void CheckUpdate()
    {
        // The "Forget()" method is used to let the async task run without waiting.
        // More information: https://docs.microsoft.com/en-us/answers/questions/186037/taskrun-without-wait.html
        // To use "Forget" you need the following nuget package: https://www.nuget.org/packages/Microsoft.VisualStudio.Threading/
        UpdateHelper.LoadReleaseInfoAsync(SetReleaseInfo).Forget();
    }

    /// <summary>
    /// Sets the release info and shows the update button
    /// </summary>
    /// <param name="releaseInfo">The infos of the latest release</param>
    private void SetReleaseInfo(ReleaseInfo releaseInfo)
    {
        ButtonUpdateVisibility = !string.IsNullOrWhiteSpace(releaseInfo.Name) ? Visibility.Visible : Visibility.Hidden;
        UpdateInfo = $"Update available! New version: v{releaseInfo.NewVersion}";
    }

    #endregion
}
