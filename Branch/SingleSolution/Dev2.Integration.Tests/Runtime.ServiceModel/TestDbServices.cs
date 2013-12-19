using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Dev2RuntimeServiceModel = Dev2.Runtime.ServiceModel;

namespace Dev2.Integration.Tests.Runtime.ServiceModel
{
    public class TestDbServices : Dev2RuntimeServiceModel.Services
    {
        public SqlDatabaseBroker Broker { get; private set; }

        public TestDbServices(IResourceCatalog resourceCatalog)
            : base(resourceCatalog)
        {
        }

        public TestDbServices()
        {
        }

        public TestDbServices(SqlDatabaseBroker broker)
        {
            Broker = broker;
        }

        protected override SqlDatabaseBroker CreateDatabaseBroker()
        {
            return Broker ?? base.CreateDatabaseBroker();
        }
    }
}