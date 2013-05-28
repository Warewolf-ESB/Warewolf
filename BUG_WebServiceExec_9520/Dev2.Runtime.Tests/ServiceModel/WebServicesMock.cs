using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Tests.Runtime.ServiceModel
{
    public class WebServicesMock : WebServices
    {
        public new Service DeserializeService(string args)
        {
            return base.DeserializeService(args);
        }

        public new Service DeserializeService(XElement xml, ResourceType resourceType)
        {
            return base.DeserializeService(xml, resourceType);
        }
    }
}
