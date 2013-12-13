using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DynamicServices;
using System;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;
using Dev2.Runtime.ESB.Management;

namespace Dev2.Runtime.ESB.Execution
{
    /// <summary>
    /// Execute an internal or management service
    /// </summary>
    public class InternalServiceContainer : EsbExecutionContainer
    {

        public InternalServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request)
            : base(sa, dataObj, theWorkspace, esbChannel, request)
        {
            
        }

        public override Guid Execute(out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            var invokeErrors = new ErrorResultTO();
            Guid result = GlobalConstants.NullDataListID;

            try
            {
                EsbManagementServiceLocator emsl = new EsbManagementServiceLocator();
                IEsbManagementEndpoint eme = emsl.LocateManagementService(ServiceAction.Name);

                if(eme != null)
                {
                    var res = eme.Execute(Request.Args, TheWorkspace);
                    Request.ExecuteResult = res; 
                    errors.MergeErrors(invokeErrors);
                    result = DataObject.DataListID;
                }
                else
                {
                    errors.AddError("Could not locate management service [ " + ServiceAction.ServiceName + " ]");
                }
            }
            catch (Exception ex)
            {
                errors.AddError(ex.Message);
            }

            return result;
        }
    }
}
