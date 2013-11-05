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