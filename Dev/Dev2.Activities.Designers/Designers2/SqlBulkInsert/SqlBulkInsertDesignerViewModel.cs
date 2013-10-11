using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Providers.Errors;
using Dev2.Providers.Logs;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.TO;
using Newtonsoft.Json;
using Unlimited.Framework;

namespace Dev2.Activities.Designers2.SqlBulkInsert
{
    public class SqlBulkInsertDesignerViewModel : ActivityCollectionDesignerViewModel<DataColumnMapping>
    {
        readonly IEventAggregator _eventPublisher;
        readonly IEnvironmentModel _environmentModel;
        readonly DbSource _newDbSource;
        readonly bool _isInitializing;

        static readonly IEnumerable<DbTable> EmptyDbTables = new DbTable[0];
        static readonly IEnumerable<DbColumn> EmptyDbColumns = new DbColumn[0];

        static IEnvironmentModel GetActiveEnvironment()
        {
            // TODO: Fix
            return EnvironmentRepository.Instance.Source;
        }

        public SqlBulkInsertDesignerViewModel(ModelItem modelItem)
            : this(modelItem, GetActiveEnvironment(), EventPublishers.Aggregator)
        {
        }

        public SqlBulkInsertDesignerViewModel(ModelItem modelItem, IEnvironmentModel environmentModel, IEventAggregator eventPublisher)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("environmentModel", environmentModel);
            _environmentModel = environmentModel;
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _eventPublisher = eventPublisher;

            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();

            dynamic mi = ModelItem;
            ModelItemCollection = mi.InputMappings;

            _newDbSource = new DbSource
            {
                ResourceName = "New Database Source..."
            };

            Databases = new ObservableCollection<DbSource>();
            Tables = new ObservableCollection<DbTable>();

            EditDatabaseCommand = new RelayCommand(o => EditDbSource(), o => CanEditDatabase);
            RefreshTablesCommand = new RelayCommand(o => LoadDatabaseTables(SelectedDatabase), o => CanEditDatabase);

            _isInitializing = true;
            try
            {
                LoadDatabases();

                SetSelectedDatabase(Database);
                SetSelectedTable(TableName);
            }
            finally
            {
                _isInitializing = false;
            }
        }

        public override string CollectionName { get { return "InputMappings"; } }

        public ObservableCollection<DbSource> Databases { get; private set; }

        public ObservableCollection<DbTable> Tables { get; private set; }

        public bool CanEditDatabase { get { return SelectedDatabase != null; } }

        public ICommand EditDatabaseCommand { get; private set; }

        public ICommand RefreshTablesCommand { get; private set; }

        public bool IsRefreshing
        {
            get { return (bool)GetValue(IsRefreshingProperty); }
            set { SetValue(IsRefreshingProperty, value); }
        }

        public static readonly DependencyProperty IsRefreshingProperty =
            DependencyProperty.Register("IsRefreshing", typeof(bool), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(false));

        public DbSource SelectedDatabase
        {
            get { return (DbSource)GetValue(SelectedDatabaseProperty); }
            set { SetValue(SelectedDatabaseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedDatabase.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedDatabaseProperty =
            DependencyProperty.Register("SelectedDatabase", typeof(DbSource), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(null, OnSelectedDatabaseChanged));

        static void OnSelectedDatabaseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (SqlBulkInsertDesignerViewModel)d;
            var dbSource = (DbSource)e.NewValue;
            viewModel.Database = dbSource;
            viewModel.LoadDatabaseTables(dbSource);
        }

        public DbTable SelectedTable
        {
            get { return (DbTable)GetValue(SelectedTableProperty); }
            set { SetValue(SelectedTableProperty, value); }
        }

        public static readonly DependencyProperty SelectedTableProperty =
            DependencyProperty.Register("SelectedTable", typeof(DbTable), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(null, OnSelectedTableChanged));

        static void OnSelectedTableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (SqlBulkInsertDesignerViewModel)d;
            var dbTable = (DbTable)e.NewValue;
            viewModel.TableName = dbTable == null ? null : dbTable.TableName;
            viewModel.LoadTableColumns(viewModel.SelectedDatabase, dbTable);
        }

        public bool IsSelectedDatabaseFocused
        {
            get { return (bool)GetValue(IsSelectedDatabaseFocusedProperty); }
            set { SetValue(IsSelectedDatabaseFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedDatabaseFocusedProperty =
            DependencyProperty.Register("IsSelectedDatabaseFocused", typeof(bool), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(false));

        public bool IsSelectedTableFocused
        {
            get { return (bool)GetValue(IsSelectedTableFocusedProperty); }
            set { SetValue(IsSelectedTableFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedTableFocusedProperty =
            DependencyProperty.Register("IsSelectedTableFocused", typeof(bool), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(false));

        public bool IsBatchSizeFocused
        {
            get { return (bool)GetValue(IsBatchSizeFocusedProperty); }
            set { SetValue(IsBatchSizeFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsBatchSizeFocusedProperty =
            DependencyProperty.Register("IsBatchSizeFocused", typeof(bool), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(false));

        public bool IsTimeoutFocused
        {
            get { return (bool)GetValue(IsTimeoutFocusedProperty); }
            set { SetValue(IsTimeoutFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsTimeoutFocusedProperty =
            DependencyProperty.Register("IsTimeoutFocused", typeof(bool), typeof(SqlBulkInsertDesignerViewModel), new PropertyMetadata(false));       

        #region DO NOT bind to these properties - these are here for internal view model use only!!!

        DbSource Database
        {
            get { return GetProperty<DbSource>(); }
            set
            {
                if(!_isInitializing)
                {
                    SetProperty(value);
                }
            }
        }

        string TableName
        {
            get { return GetProperty<string>(); }
            set
            {
                if(!_isInitializing)
                {
                    SetProperty(value);
                }
            }
        }

        string BatchSize { get { return GetProperty<string>(); } }

        string Timeout { get { return GetProperty<string>(); } }

        #endregion

        void LoadDatabases()
        {
            if(!_isInitializing)
            {
                Databases.Clear();
                Tables.Clear();
                ModelItemCollection.Clear();
            }

            Databases.Add(_newDbSource);

            var sources = ResourceRepository.FindSourcesByType(_environmentModel, enSourceType.SqlDatabase)
                .Select(o => new DbSource(o.xmlData))
                .OrderBy(r => r.ResourceName);

            foreach(var dbSource in sources)
            {
                Databases.Add(dbSource);
            }
        }

        void LoadDatabaseTables(DbSource dbSource)
        {
            if(dbSource == _newDbSource)
            {
                CreateDbSource();
                return;
            }

            // Save selection --> ComboBox binding clears selection when Tables collection is cleared
            var selectedTableName = GetTableName(SelectedTable);

            IsRefreshing = true;
            try
            {
                Tables.Clear();
                var tables = GetDatabaseTables(dbSource);
                foreach(var table in tables)
                {
                    Tables.Add(table);
                }
            }
            finally
            {
                IsRefreshing = false;

                // Restore selection or select first in list
                var selectedTable = Tables.FirstOrDefault(t => t.TableName == selectedTableName) ?? Tables.FirstOrDefault();
                SelectedTable = selectedTable;
            }
        }

        void LoadTableColumns(DbSource dbSource, DbTable dbTable)
        {
            if(IsRefreshing)
            {
                return;
            }

            if(dbTable == null)
            {
                ModelItemCollection.Clear();
                return;
            }

            var oldColumns = ModelItemCollection.Select(mi => (DataColumnMapping)mi.GetCurrentValue()).ToList();
            ModelItemCollection.Clear();

            IsRefreshing = true;
            try
            {
                var columns = GetDatabaseTableColumns(dbSource, dbTable);
                foreach(var mapping in columns.Select(column => new DataColumnMapping { OutputColumn = column }))
                {
                    var oldColumn = oldColumns.FirstOrDefault(c => c.OutputColumn.ColumnName == mapping.OutputColumn.ColumnName);
                    if(oldColumn != null)
                    {
                        mapping.InputColumn = oldColumn.InputColumn;
                    }

                    ModelItemCollection.Add(mapping);
                }
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        void EditDbSource()
        {
            if(SelectedDatabase != null)
            {
                var selectedDatabase = SelectedDatabase;
                var selectedTableName = GetTableName(SelectedTable);
                var resourceModel = _environmentModel.ResourceRepository.FindSingle(c => c.ResourceName == SelectedDatabase.ResourceName);
                if(resourceModel != null)
                {
                    Logger.TraceInfo("Publish message of type - " + typeof(ShowEditResourceWizardMessage));
                    _eventPublisher.Publish(new ShowEditResourceWizardMessage(resourceModel));
                    LoadDatabases();

                    SetSelectedDatabase(selectedDatabase);
                    SetSelectedTable(selectedTableName);
                }
            }
        }

        void CreateDbSource()
        {
            Logger.TraceInfo("Publish message of type - " + typeof(ShowNewResourceWizard));
            _eventPublisher.Publish(new ShowNewResourceWizard("DbSource"));
            LoadDatabases();
        }

        IEnumerable<DbTable> GetDatabaseTables(DbSource dbSource)
        {
            if(dbSource == null)
            {
                return EmptyDbTables;
            }

            dynamic request = new UnlimitedObject();
            request.Service = "GetDatabaseTablesService";
            request.Database = JsonConvert.SerializeObject(dbSource);

            var workspaceID = _environmentModel.Connection.WorkspaceID;

            var result = _environmentModel.Connection.ExecuteCommand(request.XmlString, workspaceID, GlobalConstants.NullDataListID);

            var tables = JsonConvert.DeserializeObject<List<DbTable>>(result);
            return tables ?? EmptyDbTables;
        }

        IEnumerable<DbColumn> GetDatabaseTableColumns(DbSource dbSource, DbTable dbTable)
        {
            if(dbSource == null || dbTable == null)
            {
                return EmptyDbColumns;
            }

            dynamic request = new UnlimitedObject();
            request.Service = "GetDatabaseColumnsForTableService";
            request.Database = JsonConvert.SerializeObject(dbSource);
            request.TableName = JsonConvert.SerializeObject(dbTable.TableName);

            var workspaceID = _environmentModel.Connection.WorkspaceID;

            var result = _environmentModel.Connection.ExecuteCommand(request.XmlString, workspaceID, GlobalConstants.NullDataListID);

            var tables = JsonConvert.DeserializeObject<List<DbColumn>>(result);
            return tables ?? EmptyDbColumns;
        }

        void SetSelectedDatabase(DbSource dbSource)
        {
            SelectedDatabase = dbSource == null ? null : Databases.FirstOrDefault(d => d.ResourceID == dbSource.ResourceID);
        }

        void SetSelectedTable(string tableName)
        {
            SelectedTable = Tables.FirstOrDefault(t => t.TableName == tableName);
        }

        static string GetTableName(DbTable table)
        {
            return table == null ? null : table.TableName;
        }

        public override void Validate()
        {
            base.Validate();

            var errors = Errors ?? new List<IActionableErrorInfo>();
            if(SelectedDatabase == null)
            {
                errors.Add(new ActionableErrorInfo(() => IsSelectedDatabaseFocused = true) { ErrorType = ErrorType.Critical, Message = "A database must be selected." });
            }
            if(SelectedTable == null)
            {
                errors.Add(new ActionableErrorInfo(() => IsSelectedTableFocused = true) { ErrorType = ErrorType.Critical, Message = "A table must be selected." });
            }

            var batchSize = BatchSize;
            if(!string.IsNullOrEmpty(batchSize))
            {
                int value;
                if(!int.TryParse(batchSize, out value) || value <= 0)
                {
                    errors.Add(new ActionableErrorInfo(() => IsBatchSizeFocused = true) { ErrorType = ErrorType.Critical, Message = "Batch size must be a number greater than zero." });
                }
            }

            var timeout = Timeout;
            if(!string.IsNullOrEmpty(timeout))
            {
                int value;
                if(!int.TryParse(timeout, out value) || value <= 0)
                {
                    errors.Add(new ActionableErrorInfo(() => IsTimeoutFocused = true) { ErrorType = ErrorType.Critical, Message = "Timeout must be a number greater than zero." });
                }
            }

            Errors = errors.Count == 0 ? null : errors;
        }
    }
}