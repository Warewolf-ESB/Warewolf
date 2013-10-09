using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.SqlBulkInsert;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Activities.Designers.Tests.SqlBulkInsert
{
    internal class TestSqlBulkInsertDesignerViewModel : SqlBulkInsertDesignerViewModel
    {
        public TestSqlBulkInsertDesignerViewModel(ModelItem modelItem, IEnvironmentModel environmentModel)
            : base(modelItem, environmentModel)
        {
        }

        public DbSource Database { get { return GetProperty<DbSource>(); } set { SetProperty(value); } }
    }
}