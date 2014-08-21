using System.Xml.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Tests.Runtime.ServiceModel
{
    public class WebServicesMock : WebServices
    {
        public WebServicesMock()
        {
        }

        public WebServicesMock(IResourceCatalog resourceCatalog, WebExecuteString webExecute)
            : base(resourceCatalog, webExecute)
        {
        }

        public new Service DeserializeService(string args)
        {
            return base.DeserializeService(args);
        }

        public new Service DeserializeService(XElement xml, ResourceType resourceType)
        {
            return base.DeserializeService(xml, resourceType);
        }

        public bool FetchRecordsetAddFields { get; set; }
        public int FetchRecordsetHitCount { get; set; }
        public override RecordsetList FetchRecordset(WebService service, bool addFields)
        {
            FetchRecordsetHitCount++;
            FetchRecordsetAddFields = addFields;
            return base.FetchRecordset(service, addFields);
        }
    }
}
