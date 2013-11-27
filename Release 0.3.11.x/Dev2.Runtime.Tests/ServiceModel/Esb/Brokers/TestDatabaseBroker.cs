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