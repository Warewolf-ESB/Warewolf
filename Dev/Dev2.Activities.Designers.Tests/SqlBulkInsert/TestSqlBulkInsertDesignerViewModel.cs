
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Activities.Designers2.SqlBulkInsert;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Dev2.TO;

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
