using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Tests.Runtime.ServiceModel
{
    public class ServicesMock : Dev2.Runtime.ServiceModel.Services
    {
        public int FetchRecordsetHitCount { get; set; }
        public bool FetchRecordsetUpdateFields { get; set; }

        public override Recordset FetchRecordset(Service service, bool updateFields)
        {
            FetchRecordsetHitCount++;
            FetchRecordsetUpdateFields = updateFields;
            return service.MethodRecordset;
        }
    }
}
