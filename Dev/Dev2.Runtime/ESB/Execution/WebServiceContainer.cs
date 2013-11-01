using System.Xml.Linq;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Execution
{
    // BUG 9619 - 2013.06.05 - TWR - Refactored
    public class WebServiceContainer : EsbExecutionContainerAbstract<WebService>
    {
        public WebServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
        }

        protected override WebService CreateService(XElement serviceXml, XElement sourceXml)
        {
            return new WebService(serviceXml) { Source = new WebSource(sourceXml) };
        }

        protected override object ExecuteService(WebService service, out ErrorResultTO errors)
        {
            ExecuteWebRequest(service, out errors);
            var result = Scrubber.Scrub(service.RequestResponse);
            service.RequestResponse = null;
            return result;
        }

        protected virtual void ExecuteWebRequest(WebService service, out ErrorResultTO errors)
        {
            WebServices.ExecuteRequest(service, false, out errors);
        }
    }
}
