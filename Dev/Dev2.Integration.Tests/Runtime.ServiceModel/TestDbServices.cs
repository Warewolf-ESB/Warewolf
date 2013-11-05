using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Dev2RuntimeServiceModel = Dev2.Runtime.ServiceModel;

namespace Dev2.Integration.Tests.Runtime.ServiceModel
{
    public class TestDbServices : Dev2RuntimeServiceModel.Services
    {
        public SqlDatabaseBroker Broker { get; private set; }
        public int FetchRecordsetHitCount { get; set; }
        public bool FetchRecordsetAddFields { get; set; }

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

        public override Recordset FetchRecordset(DbService service, bool addFields)
        {
            FetchRecordsetHitCount++;
            FetchRecordsetAddFields = addFields;
            return service.Recordset;
        }
    }
}