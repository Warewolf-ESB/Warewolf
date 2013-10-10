using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Preview;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.TO;
using Newtonsoft.Json;
using Unlimited.Framework;

namespace Dev2.Activities.Designers2.SqlBulkInsert
{
    public class SqlBulkInsertDesignerViewModel : ActivityCollectionDesignerViewModel<DataColumnMapping>
    {
        readonly IEnvironmentModel _environmentModel;
        readonly DbSource _newDbSource;

        static readonly IEnumerable<DbTable> EmptyDbTables = new DbTable[0];

        static IEnvironmentModel GetActiveEnvironment()
        {
            // TODO: Fix
            return EnvironmentRepository.Instance.Source;
        }

        public SqlBulkInsertDesignerViewModel(ModelItem modelItem)
            : this(modelItem, GetActiveEnvironment())
        {
        }

        public SqlBulkInsertDesignerViewModel(ModelItem modelItem, IEnvironmentModel environmentModel)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("environmentModel", environmentModel);
            _environmentModel = environmentModel;

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

            EditDatabaseCommand = new RelayCommand(o => EditDatabase(), o => CanEditDatabase);
            RefreshTablesCommand = new RelayCommand(o => LoadDatabaseTables(SelectedDatabase), o => CanEditDatabase);

            LoadDatabases();
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
        DbSource Database { set { SetProperty(value); } }
        string TableName { set { SetProperty(value); } }

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
            IsRefreshing = true;
            try
            {
                //if(dbSource == _newDbSource)
                //{
                //    CreateDatabase();
                //}

                // Save selection --> ComboBox binding clears selection when Tables collection is cleared
                var currentTableName = SelectedTable == null ? null : SelectedTable.TableName;

                Tables.Clear();
                var tables = GetDatabaseTables(dbSource);
                foreach(var table in tables)
                {
                    Tables.Add(table);
                }

                if(currentTableName != null)
                {
                    // Restore selection
                    SelectedTable = Tables.FirstOrDefault(t => t.TableName == currentTableName);
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

        void EditDatabase()
        {
        }

        void CreateDatabase()
        {
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
    }
}