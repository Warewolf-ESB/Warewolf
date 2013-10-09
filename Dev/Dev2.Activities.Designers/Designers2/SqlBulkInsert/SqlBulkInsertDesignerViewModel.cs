using System.Activities.Presentation.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.DynamicServices;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.TO;

namespace Dev2.Activities.Designers2.SqlBulkInsert
{
    public class SqlBulkInsertDesignerViewModel : ActivityCollectionDesignerViewModel<DataColumnMapping>
    {
        readonly IEnvironmentModel _environmentModel;
        readonly DbSource _newDbSource;
        bool _canEditDatabase;

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

            _newDbSource = new DbSource
            {
                ResourceName = "New Database Source..."
            };

            Databases = new ObservableCollection<DbSource>();
            Tables = new ObservableCollection<string>();

            EditDatabaseCommand = new RelayCommand(o => EditDatabase(), o => _canEditDatabase);
            RefreshTablesCommand = new RelayCommand(o => LoadDatabaseTables(), o => _canEditDatabase);

            LoadDatabases();
        }

        public override string CollectionName { get { return "InputMappings"; } }

        public ObservableCollection<DbSource> Databases { get; private set; }

        public ObservableCollection<string> Tables { get; private set; }

        public ICommand EditDatabaseCommand { get; private set; }

        public ICommand RefreshTablesCommand { get; private set; }

        // DO NOT bind to these properties - these are here for convenience only!!!
        DbSource Database { get { return GetProperty<DbSource>(); } }

        protected override void OnModelItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "Database":
                    LoadDatabaseTables();
                    UpdateUIState();
                    break;

                case "TableName":
                    LoadTableColumns();
                    UpdateUIState();
                    break;
            }
        }

        void UpdateUIState()
        {
            _canEditDatabase = Database != null;
        }

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

        void LoadDatabaseTables()
        {
            if(Database == _newDbSource)
            {
                CreateDatabase();
                SetDatabase(null);
            }
        }

        void LoadTableColumns()
        {
        }

        void EditDatabase()
        {
        }

        void CreateDatabase()
        {
        }

        void SetDatabase(DbSource dbSource)
        {
            ModelItem.SetProperty("Database", dbSource);
        }
    }
}