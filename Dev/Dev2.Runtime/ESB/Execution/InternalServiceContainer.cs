using System.Collections.Generic;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DynamicServices;
using System;
using Dev2.Workspaces;
using Dev2.Runtime.ESB.Management;

namespace Dev2.Runtime.ESB.Execution
{
    /// <summary>
    /// Execute an internal or management service
    /// </summary>
    public class InternalServiceContainer : EsbExecutionContainer
    {

        public InternalServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            
        }

        public override Guid Execute(out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            var invokeErrors = new ErrorResultTO();
            Guid result = GlobalConstants.NullDataListID;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            try
            {
                EsbManagementServiceLocator emsl = new EsbManagementServiceLocator();

                IDictionary<string, string> data = new Dictionary<string, string>();

                IBinaryDataList bdl = compiler.FetchBinaryDataList(DataObject.DataListID, out invokeErrors);
                errors.MergeErrors(invokeErrors);

                if (!invokeErrors.HasErrors())
                {
                    foreach (IBinaryDataListEntry entry in bdl.FetchScalarEntries())
                    {
                        IBinaryDataListItem itm = entry.FetchScalar();
                        if (itm != null)
                        {
                            data[itm.FieldName] = itm.TheValue;
                        }
                    }
                }

                IEsbManagementEndpoint eme = emsl.LocateManagementService(ServiceAction.Name);

                if(eme != null)
                {
                    string res = eme.Execute(data, TheWorkspace);
                    compiler.UpsertSystemTag(DataObject.DataListID, enSystemTag.ManagmentServicePayload, res, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                    result = DataObject.DataListID;

                    //if (!compiler.HasErrors(DataObject.DataListID) && !errors.HasErrors())
                    //{
                        
                    //}
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
