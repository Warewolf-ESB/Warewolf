
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Runtime.ServiceModel.Esb.Brokers;

namespace Dev2.Tests.Runtime.ServiceModel.Esb.Brokers
{
    public class TestDatabaseBroker : AbstractDatabaseBroker<TestDbServer>
    {
        public TestDatabaseBroker()
        {
        }

        public TestDatabaseBroker(TestDbServer dbServer)
        {
            DbServer = dbServer;
        }

        public TestDbServer DbServer { get; private set; }

        protected override TestDbServer CreateDbServer()
        {
            return DbServer ?? (DbServer = base.CreateDbServer());
        }
    }
}
