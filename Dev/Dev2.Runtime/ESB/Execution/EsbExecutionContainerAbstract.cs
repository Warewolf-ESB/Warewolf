using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ESB.Execution
{
    // BUG 9619 - 2013.06.05 - TWR - Created
    public abstract class EsbExecutionContainerAbstract<TService> : EsbExecutionContainer
        where TService : Service
    {
        // Plugins need to handle formatting inside the RemoteObjectHandler 
        // and NOT here otherwise serialization issues occur!
        public bool HandlesOutputFormatting { get; private set; }

        protected EsbExecutionContainerAbstract(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, bool handlesOutputFormatting = true)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            HandlesOutputFormatting = handlesOutputFormatting;
        }

        public override Guid Execute(out ErrorResultTO errors)
        {
            var result = DataObject.DataListID;
            ExecuteImpl(out errors);
            return result;
        }

        protected abstract TService CreateService(XElement serviceXml, XElement sourceXml);
        protected abstract object ExecuteService(TService service);

        #region ExecuteImpl

        void ExecuteImpl(out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            #region Create service

            TService service;
            try
            {
                var sourceXml = XElement.Parse(ServiceAction.Source.ResourceDefinition);
                var serviceXml = ServiceAction.Parent.xmlData;
                service = CreateService(serviceXml, sourceXml);
            }
            catch(Exception ex)
            {
                errors.AddError(string.Format("Service Deserialization Failed: {0}", ex.Message));
                return;
            }

            #endregion

            #region Create OutputFormatter

            var outputFormatter = GetOutputFormatter();
            if(HandlesOutputFormatting && outputFormatter == null)
            {
                errors.AddError(string.Format("Output format in service action {0} is invalid.", ServiceAction.Name));
                return;
            }

            #endregion

            try
            {
                ErrorResultTO invokeErrors;

                var compiler = DataListFactory.CreateDataListCompiler();
                var itrs = new List<IDev2DataListEvaluateIterator>(5);
                var itrCollection = Dev2ValueObjectFactory.CreateIteratorCollection();

                if(ServiceAction.ServiceActionInputs.Count == 0)
                {
                    ExecuteServiceAndMergeResultIntoDataList(service, outputFormatter, compiler, itrCollection, itrs, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                }
                else
                {
                    #region Build iterators for each ServiceActionInput

                    foreach(var sai in ServiceAction.ServiceActionInputs)
                    {
                        var val = sai.Source;
                        var toInject = AppServerStrings.NullConstant;

                        if(val != null)
                        {
                            toInject = DataListUtil.AddBracketsToValueIfNotExist(sai.Source);
                        }
                        else if(!sai.EmptyToNull)
                        {
                            toInject = sai.DefaultValue;
                        }

                        var expressionEntry = compiler.Evaluate(DataObject.DataListID, DataList.Contract.enActionType.User, toInject, false, out invokeErrors);
                        errors.MergeErrors(invokeErrors);
                        var expressionIterator = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionEntry);
                        itrCollection.AddIterator(expressionIterator);
                        itrs.Add(expressionIterator);
                    }

                    #endregion

                    while(itrCollection.HasMoreData())
                    {
                        ExecuteServiceAndMergeResultIntoDataList(service, outputFormatter, compiler, itrCollection, itrs, out invokeErrors);
                        errors.MergeErrors(invokeErrors);
                    }
                }
            }
            finally
            {
                var disposable = service as IDisposable;
                if(disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        #endregion

        #region ExecuteServiceAndMergeResultIntoDataList

        void ExecuteServiceAndMergeResultIntoDataList(TService service, IOutputFormatter outputFormatter, IDataListCompiler compiler, IDev2IteratorCollection itrCollection, IEnumerable<IDev2DataListEvaluateIterator> itrs, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            ErrorResultTO invokeErrors;

            var response = ExecuteService(service, itrCollection, itrs, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            if (errors.HasErrors())
            {
                return;
            }

            MergeResultIntoDataList(response, outputFormatter, compiler, out invokeErrors);
            errors.MergeErrors(invokeErrors);
        }

        #endregion

        #region ExecuteService

        object ExecuteService(TService service, IDev2IteratorCollection itrCollection, IEnumerable<IDev2DataListEvaluateIterator> itrs, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            if(ServiceAction.ServiceActionInputs.Any())
            {
                // Loop iterators 
                var pos = 0;
                foreach(var itr in itrs)
                {
                    var injectVal = itrCollection.FetchNextRow(itr);
                    var sai = service.Method.Parameters[pos];
                    var param = service.Method.Parameters[pos];
                    if(param != null)
                    {
                        param.Value = sai.EmptyToNull && (injectVal == null || string.Compare(injectVal.TheValue, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0)
                                          ? null : injectVal.TheValue;
                    }
                    pos++;
                }
            }

            try
            {
                return ExecuteService(service);
            }
            catch(Exception ex)
            {
                errors.AddError(string.Format("Service Execution Error: {0}", ex.Message));
            }
            return null;
        }

        #endregion

        #region MergeResultIntoDataList

        void MergeResultIntoDataList(object result, IOutputFormatter outputFormatter, IDataListCompiler compiler, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            ErrorResultTO invokeErrors;

            var formattedPayload = outputFormatter != null ? outputFormatter.Format(result).ToString() : result.ToString();

            // Create a shape from the service action outputs
            var dlShape = compiler.ShapeDev2DefinitionsToDataList(ServiceAction.OutputSpecification, enDev2ArgumentType.Output, false, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            // Push formatted data into a datalist using the shape from the service action outputs
            var tmpID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), formattedPayload, dlShape, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            // Attach a parent ID to the newly created datalist
            compiler.SetParentID(tmpID, DataObject.DataListID);

            // Merge each result into the datalist ;)
            compiler.Merge(DataObject.DataListID, tmpID, enDataListMergeTypes.Union, enTranslationDepth.Data_With_Blank_OverWrite, false, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            compiler.ForceDeleteDataListByID(tmpID); // clean up 
        }

        #endregion

        #region GetOutputFormatter

        IOutputFormatter GetOutputFormatter()
        {
            if(!HandlesOutputFormatting)
            {
                return null;
            }

            var outputDescription = ServiceAction.OutputDescription;
            var outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();

            if(outputDescriptionSerializationService == null)
            {
                return null;
            }

            var outputDescriptionInstance = outputDescriptionSerializationService.Deserialize(outputDescription);

            return outputDescriptionInstance == null ? null : OutputFormatterFactory.CreateOutputFormatter(outputDescriptionInstance);
        }

        #endregion
    }
}
