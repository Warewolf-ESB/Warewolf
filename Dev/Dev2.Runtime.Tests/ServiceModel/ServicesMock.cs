using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Tests.Runtime.ServiceModel
{
    public class ServicesMock : Dev2.Runtime.ServiceModel.Services
    {
        public int FetchRecordsetHitCount { get; set; }
        public bool FetchRecordsetAddFields { get; set; }

        public override Recordset FetchRecordset(Service service, bool addFields)
        {
            FetchRecordsetHitCount++;
            FetchRecordsetAddFields = addFields;
            return service.Recordset;
        }
    }
}
