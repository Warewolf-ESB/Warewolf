using System.Activities.Presentation.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Dev2.Activities.Designers2.Core;
using Dev2.TO;

namespace Dev2.Activities.Designers2.SqlBulkInsert
{
    public class SqlBulkInsertDesignerViewModel : ActivityCollectionDesignerViewModel<DataColumnMapping>
    {
        public SqlBulkInsertDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();

            dynamic mi = ModelItem;
            ModelItemCollection = mi.InputMappings;

            Databases = new ObservableCollection<string>();
            Tables = new ObservableCollection<string>();
        }

        public override string CollectionName { get { return "InputMappings"; } }

        public ObservableCollection<string> Databases { get; private set; }

        public ObservableCollection<string> Tables { get; private set; }

        protected override void OnModelItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "Database":
                    LoadTables();
                    break;

                case "TableName":
                    LoadTableColumns();
                    break;
            }
        }

        void LoadTables()
        {
        }

        void LoadTableColumns()
        {
        }
    }
}