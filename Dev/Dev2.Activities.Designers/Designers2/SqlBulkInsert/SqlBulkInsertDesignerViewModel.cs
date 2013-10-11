using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Preview;
using Dev2.Common;
using Dev2.DynamicServices;
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

        static readonly IEnumerable<DbTable> EmptyDbTables = new DbTable[0];

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

            PreviewViewModel = new PreviewViewModel
            {
                InputsVisibility = Visibility.Collapsed,
            };
            PreviewViewModel.PreviewRequested += DoPreview;

            _newDbSource = new DbSource
            {
                ResourceName = "New Database Source..."
            };

            Databases = new ObservableCollection<DbSource>();
            Tables = new ObservableCollection<DbTable>();

            EditDatabaseCommand = new RelayCommand(o => EditDbSource(), o => CanEditDatabase);
            RefreshTablesCommand = new RelayCommand(o => LoadDatabaseTables(SelectedDatabase), o => CanEditDatabase);

            LoadDatabases();

            SetSelectedDatabase(Database);
            SetSelectedTable(TableName);
        }

        public override string CollectionName { get { return "InputMappings"; } }

        public PreviewViewModel PreviewViewModel { get; private set; }

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
            viewModel.LoadTableColumns(dbTable);
        }

        // DO NOT bind to these properties - these are here for convenience only!!!
        DbSource Database { get { return GetProperty<DbSource>(); } set { SetProperty(value); } }
        string TableName { get { return GetProperty<string>(); } set { SetProperty(value); } }

        void LoadDatabases()
        {
            Databases.Clear();
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

            IsRefreshing = true;
            try
            {
                // Save selection --> ComboBox binding clears selection when Tables collection is cleared
                var selectedTable = SelectedTable;

                Tables.Clear();
                var tables = GetDatabaseTables(dbSource);
                foreach(var table in tables)
                {
                    Tables.Add(table);
                }

                if(selectedTable != null)
                {
                    // Restore selection
                    SetSelectedTable(selectedTable);
                }
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        void LoadTableColumns(DbTable dbTable)
        {
            ModelItemCollection.Clear();

            if(dbTable == null)
            {
                return;
            }

            foreach(var mapping in dbTable.Columns.Select(column => new DataColumnMapping { OutputColumn = column }))
            {
                ModelItemCollection.Add(mapping);
            }
        }

        void EditDbSource()
        {
            if(SelectedDatabase != null)
            {
                var selectedDatabase = SelectedDatabase;
                var selectedTable = SelectedTable;
                var resourceModel = _environmentModel.ResourceRepository.FindSingle(c => c.ResourceName == SelectedDatabase.ResourceName);
                if(resourceModel != null)
                {
                    Logger.TraceInfo("Publish message of type - " + typeof(ShowEditResourceWizardMessage));
                    _eventPublisher.Publish(new ShowEditResourceWizardMessage(resourceModel));
                    LoadDatabases();

                    SetSelectedDatabase(selectedDatabase);
                    SetSelectedTable(selectedTable);
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

            return JsonConvert.DeserializeObject<List<DbTable>>(result);
        }

        void DoPreview(object sender, PreviewRequestedEventArgs e)
        {
        }

        void SetSelectedDatabase(DbSource dbSource)
        {
            SelectedDatabase = dbSource == null ? null : Databases.FirstOrDefault(d => d.ResourceID == dbSource.ResourceID);
        }

        void SetSelectedTable(DbTable table)
        {
            SetSelectedTable(table == null ? null : table.TableName);
        }

        void SetSelectedTable(string tableName)
        {
            SelectedTable = Tables.FirstOrDefault(t => t.TableName == tableName);
        }
    }
}