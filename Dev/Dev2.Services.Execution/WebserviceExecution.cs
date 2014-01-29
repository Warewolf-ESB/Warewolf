using System;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Services.Execution
{
    public class WebserviceExecution : ServiceExecutionAbstract<WebService, WebSource>
    {

        #region Constuctors

        public WebserviceExecution(IDSFDataObject dataObj, bool handlesFormatting)
            : base(dataObj, handlesFormatting)
        {
        }

        #endregion

        #region Execute

        public override void BeforeExecution(ErrorResultTO errors)
        {
        }

        public override void AfterExecution(ErrorResultTO errors)
        {
        }

        protected virtual void ExecuteWebRequest(WebService service, out ErrorResultTO errors)
        {
            WebServices.ExecuteRequest(service, true, out errors);
        }

        protected override object ExecuteService(out ErrorResultTO errors)
        {
            Service.Source = Source;
            ExecuteWebRequest(Service, out errors);
            string result = String.IsNullOrEmpty(Service.JsonPath) || String.IsNullOrEmpty(Service.JsonPathResult) ? Scrubber.Scrub(Service.RequestResponse) : Scrubber.Scrub(Service.JsonPathResult);
            Service.RequestResponse = null;
            return result;
        }
        #endregion

    }
}