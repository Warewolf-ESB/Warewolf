using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Text;
using enActionType = Dev2.DataList.Contract.enActionType;

namespace Dev2.Runtime.ESB.Execution
{
    /// <summary>
    /// Execute a plugin ;)
    /// </summary>
    public class PluginServiceContainer : EsbExecutionContainer
    {

        public PluginServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            
        }

        public override Guid Execute(out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            Guid result = DataObject.DataListID;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO invokeErrors = new ErrorResultTO();
            
            try
            {

                // TODO : Fetch Iterators for each ServiceActionInput ;)
                IList<IDev2DataListEvaluateIterator> itrs = new List<IDev2DataListEvaluateIterator>(5);
                IDev2IteratorCollection itrCollection = Dev2ValueObjectFactory.CreateIteratorCollection();


                foreach(ServiceActionInput sai in ServiceAction.ServiceActionInputs)
                {
                    var val = sai.Source;
                    var toInject = AppServerStrings.NullConstant;

                    if (val != null)
                    {
                        toInject = DataListUtil.AddBracketsToValueIfNotExist(sai.Source);
                    }
                    else if (!sai.EmptyToNull)
                    {
                        toInject = sai.DefaultValue;
                    }

                    IBinaryDataListEntry expressionEntry = compiler.Evaluate(DataObject.DataListID, enActionType.User, toInject, false, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                    IDev2DataListEvaluateIterator expressionIterator = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionEntry);
                    itrCollection.AddIterator(expressionIterator);
                    itrs.Add(expressionIterator);
                }


                AppDomain tmpDomain = ServiceAction.PluginDomain;

                //Instantiate the Remote Oject handler which will allow cross application domain access
                var remoteHandler = (RemoteObjectHandler)tmpDomain.CreateInstanceFromAndUnwrap(typeof(IEsbChannel).Module.Name,typeof(RemoteObjectHandler).ToString());

                while(itrCollection.HasMoreData())
                {
                    var dataBuilder = new StringBuilder("<Args><Args>");

                    // Build via DataList evaluate ;)
                    int pos = 0;
                    foreach(IDev2DataListEvaluateIterator itr in itrs)
                    {
                        ServiceActionInput sai = ServiceAction.ServiceActionInputs[pos];
                        dataBuilder.Append("<Arg>");
                        dataBuilder.Append("<TypeOf>");
                        dataBuilder.Append(sai.NativeType);
                        dataBuilder.Append("</TypeOf>");
                        dataBuilder.Append("<Value>");
                        dataBuilder.Append(itrCollection.FetchNextRow(itr).TheValue); // Fetch value and assign
                        dataBuilder.Append("</Value>");
                        dataBuilder.Append("</Arg>");
                    }

                    dataBuilder.Append("</Args></Args>");

                    //2013.04.29: Ashley Lewis - PBI 8721 AssemblyName moved from source to service in the new framework
                    string exeValue = "";
                    if (ServiceAction.Source != null && ServiceAction.Source.AssemblyLocation != null)
                    {
                        if(string.IsNullOrEmpty(ServiceAction.Source.FullName))
                        {
                            //Old framework compatability
                            exeValue = (remoteHandler.RunPlugin(ServiceAction.Source.AssemblyLocation, ServiceAction.Source.AssemblyName,
                                ServiceAction.SourceMethod, dataBuilder.ToString(), ServiceAction.OutputDescription, true));
                        }
                        else
                        {
                            exeValue = (remoteHandler.RunPlugin(ServiceAction.Source.AssemblyLocation, ServiceAction.Source.FullName,
                                ServiceAction.SourceMethod, dataBuilder.ToString(), ServiceAction.OutputDescription, true));
                        }
                    }

                    // ReSharper disable RedundantAssignment

                    // ReSharper restore RedundantAssignment
                    // TODO : Now create a new dataList and merge the result into the current dataList ;)
                    string dlShape = compiler.ShapeDev2DefinitionsToDataList(ServiceAction.OutputSpecification, enDev2ArgumentType.Output, false, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                    invokeErrors.ClearErrors();

                    Guid tmpID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), exeValue, dlShape, out invokeErrors);

                    // Merge each result into the datalist ;)
                    compiler.Merge(DataObject.DataListID, tmpID, enDataListMergeTypes.Union, enTranslationDepth.Data_With_Blank_OverWrite, false, out invokeErrors);

                    //compiler.Shape(tmpID, enDev2ArgumentType.Output_Append_Style, ServiceAction.OutputSpecification, out errors);
                    errors.MergeErrors(invokeErrors);
                    compiler.ForceDeleteDataListByID(tmpID); // clean up ;)
                }

                //Unload the temporary application domain
                //AppDomain.Unload(tmpDomain);
            }
            catch (Exception ex)
            {
                errors.AddError(ex.Message);
            }

            return result;
        }
    }
}
