using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
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
            if(service != null)
            {
                ErrorResultTO error;
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                //Evaluate the request headers
                IBinaryDataListEntry dlEntry = compiler.Evaluate(DataObj.DataListID, enActionType.User, service.RequestHeaders, false, out error);
                IBinaryDataListItem binaryDataListItem = dlEntry.FetchScalar();
                service.RequestHeaders = binaryDataListItem.TheValue;
                //Evaluate the request body
                IBinaryDataListEntry bodyDlEntry = compiler.Evaluate(DataObj.DataListID, enActionType.User, service.RequestBody, false, out error);
                IBinaryDataListItem bodyBinaryDataListItem = bodyDlEntry.FetchScalar();
                service.RequestBody = bodyBinaryDataListItem.TheValue;
            }
            WebServices.ExecuteRequest(service, true, out errors);
        }

        protected override object ExecuteService(out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            Service.Source = Source;
            ExecuteWebRequest(Service, out errors);
            var result = Scrubber.Scrub(Service.RequestResponse);
            Service.RequestResponse = null;
            return result;
        }
        #endregion

    }
}