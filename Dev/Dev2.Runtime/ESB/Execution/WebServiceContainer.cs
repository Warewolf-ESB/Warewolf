using System;
using System.Xml.Linq;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Execution;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Execution
{
    // BUG 9619 - 2013.06.05 - TWR - Refactored
    public class WebServiceContainer : EsbExecutionContainer
    {
        protected IServiceExecution WebserviceExecution;

        public WebServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            WebserviceExecution = new WebserviceExecution(dataObj, true);
        }

        public WebServiceContainer(IServiceExecution webServiceExecution)
            : base(webServiceExecution)
        {
            WebserviceExecution = webServiceExecution;
        }

        public override Guid Execute(out ErrorResultTO errors)
        {
            WebserviceExecution.InstanceOutputDefintions = InstanceOutputDefinition;
            var result = WebserviceExecution.Execute(out errors);
            return result;
        }
    }
}
