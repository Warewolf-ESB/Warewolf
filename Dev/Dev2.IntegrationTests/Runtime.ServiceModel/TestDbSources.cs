
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Esb.Brokers;

namespace Dev2.Integration.Tests.Runtime.ServiceModel
{
    public class TestDbSources : DbSources
    {
        public SqlDatabaseBroker Broker { get; private set; }

        public TestDbSources()
        {
        }

        public TestDbSources(SqlDatabaseBroker broker)
        {
            Broker = broker;
        }

        protected override SqlDatabaseBroker CreateDatabaseBroker()
        {
            return Broker ?? base.CreateDatabaseBroker();
        }
    }
}
