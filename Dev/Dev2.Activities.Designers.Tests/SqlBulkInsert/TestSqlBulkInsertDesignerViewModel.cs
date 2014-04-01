using Caliburn.Micro;
using Dev2.Activities.Designers2.SqlBulkInsert;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Dev2.TO;
using System.Activities.Presentation.Model;
using System.Collections.Generic;

namespace Dev2.Activities.Designers.Tests.SqlBulkInsert
{
    internal class TestSqlBulkInsertDesignerViewModel : SqlBulkInsertDesignerViewModel
    {
        public TestSqlBulkInsertDesignerViewModel(ModelItem modelItem, IEnvironmentModel environmentModel, IEventAggregator eventPublisher)
            : base(modelItem, new TestAsyncWorker(), environmentModel, eventPublisher)
        {
        }

        public DbSource Database { get { return GetProperty<DbSource>(); } set { SetProperty(value); } }

        public string TableName { get { return GetProperty<string>(); } set { SetProperty(value); } }

        public IList<DataColumnMapping> InputMappings
        {
            get { return GetProperty<IList<DataColumnMapping>>(); }
            set
            {
                if(value != null)
                {
                    SetProperty(value);
                }
            }
        }

        public int OnSelectedDatabaseChangedHitCount { get; private set; }
        protected override void OnSelectedDatabaseChanged()
        {
            OnSelectedDatabaseChangedHitCount++;
            base.OnSelectedDatabaseChanged();
        }

        public int OnSelectedTableChangedHitCount { get; private set; }
        protected override void OnSelectedTableChanged()
        {
            OnSelectedTableChangedHitCount++;
            base.OnSelectedTableChanged();
        }

        public void TestAddToCollection(IEnumerable<string> source, bool overWrite)
        {
            base.AddToCollection(source, overWrite);
        }
    }
}