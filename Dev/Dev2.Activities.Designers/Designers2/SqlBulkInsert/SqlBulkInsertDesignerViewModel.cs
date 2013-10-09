using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
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
            Tables = new ObservableCollection<DbTable>();

            EditDatabaseCommand = new RelayCommand(o => EditDatabase(), o => CanEditDatabase);
            RefreshTablesCommand = new RelayCommand(o => LoadDatabaseTables(), o => CanEditDatabase);

            LoadDatabases();
        }

        public override string CollectionName { get { return "InputMappings"; } }

        public ObservableCollection<DbSource> Databases { get; private set; }

        public ObservableCollection<DbTable> Tables { get; private set; }

        public bool CanEditDatabase { get { return Database != null; } }

        public ICommand EditDatabaseCommand { get; private set; }

        public ICommand RefreshTablesCommand { get; private set; }

        // DO NOT bind to these properties - these are here for convenience only!!!
        DbSource Database { get { return GetProperty<DbSource>(); } }
        string TableName { get { return GetProperty<string>(); } }

        protected override void OnModelItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "Database":
                    LoadDatabaseTables();
                    break;

                case "TableName":
                    LoadTableColumns();
                    break;
            }
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
            var dbSource = Database;
            //if(Database == _newDbSource)
            //{
            //    CreateDatabase();
            //}

            Tables.Clear();
            var tables = GetDatabaseTables();
            foreach(var table in tables)
            {
                Tables.Add(table);
            }
        }

        void LoadTableColumns()
        {
            var table = Tables.FirstOrDefault(t => t.TableName == TableName);
            if(table == null)
            {
                return;
            }

            ModelItemCollection.Clear();
            foreach(var mapping in table.Columns.Select(column => new DataColumnMapping { OutputColumn = column }))
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

        void SetDatabase(DbSource dbSource)
        {
            ModelItem.SetProperty("Database", dbSource);
        }

        IEnumerable<DbTable> GetDatabaseTables()
        {
            dynamic request = new UnlimitedObject();
            request.Service = "GetDatabaseTablesService";
            request.Database = JsonConvert.SerializeObject(Database);
            request.TableName = TableName;

            var workspaceID = _environmentModel.Connection.WorkspaceID;

            var result = _environmentModel.Connection.ExecuteCommand(request.XmlString, workspaceID, GlobalConstants.NullDataListID);

            return JsonConvert.DeserializeObject<List<DbTable>>(result);
        }
    }
}