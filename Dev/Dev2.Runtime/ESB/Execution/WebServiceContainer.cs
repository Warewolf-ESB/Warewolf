using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ESB.Execution
{
    /// <summary>
    /// Webservice Execution Container
    /// </summary>
    public class WebServiceContainer : EsbExecutionContainer
    {

        public WebServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
        }

        public override Guid Execute(out ErrorResultTO errors)
        {
            var result = DataObject.DataListID;
            ExecuteImpl(out errors);
            return result;
        }

        protected virtual void ExecuteWebRequest(WebService service)
        {
            WebServices.ExecuteRequest(service);
        }

        #region ExecuteImpl

        void ExecuteImpl(out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            #region Create WebService

            WebService service;
            try
            {
                var sourceXml = XElement.Parse(ServiceAction.Source.ResourceDefinition);
                var serviceXml = ServiceAction.Parent.xmlData;
                service = new WebService(serviceXml) { Source = new WebSource(sourceXml) };
            }
            catch(Exception ex)
            {
                errors.AddError(string.Format("WebService Deserialization Failed: {0}", ex.Message));
                return;
            }

            #endregion

            #region Create OutputFormatter

            var outputFormatter = GetOutputFormatter();
            if(outputFormatter == null)
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
                    ExecuteAndMerge(service, outputFormatter, compiler, itrCollection, itrs, out invokeErrors);
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
                        ExecuteAndMerge(service, outputFormatter, compiler, itrCollection, itrs, out invokeErrors);
                    }
                }
            }
            finally
            {
                service.Dispose();
            }
        }

        #endregion

        #region ExecuteAndMerge

        void ExecuteAndMerge(WebService service, IOutputFormatter outputFormatter, IDataListCompiler compiler, IDev2IteratorCollection itrCollection, IEnumerable<IDev2DataListEvaluateIterator> itrs, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            ErrorResultTO invokeErrors;

            var response = ExecuteWebService(service, itrCollection, itrs, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            if(invokeErrors.HasErrors())
            {
                return;
            }

            MergeResponseIntoDataList(response, outputFormatter, compiler, out invokeErrors);
            errors.MergeErrors(invokeErrors);
        }

        #endregion

        #region ExecuteWebService

        string ExecuteWebService(WebService service, IDev2IteratorCollection itrCollection, IEnumerable<IDev2DataListEvaluateIterator> itrs, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            if(ServiceAction.ServiceActionInputs.Any())
            {
                // Loop iterators 
                var pos = 0;
                foreach(var itr in itrs)
                {
                    var injectVal = itrCollection.FetchNextRow(itr);
                    var sai = ServiceAction.ServiceActionInputs[pos];
                    var param = service.Method.Parameters.FirstOrDefault(p => p.Name == sai.Name);
                    if(param != null)
                    {
                        param.Value = sai.EmptyToNull && (injectVal == null || string.Compare(injectVal.TheValue, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0)
                                          ? null : injectVal.TheValue;
                    }
                    pos++;
                }
            }

            string result = null;
            try
            {
                ExecuteWebRequest(service);
                result = Scrubber.Scrub(service.RequestResponse);
                service.RequestResponse = null;
            }
            catch(Exception ex)
            {
                errors.AddError(string.Format("WebService Error: {0}", ex.Message));
            }

            return result;
        }

        #endregion

        #region MergeResponseIntoDataList

        void MergeResponseIntoDataList(string response, IOutputFormatter outputFormatter, IDataListCompiler compiler, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            ErrorResultTO invokeErrors;

            var formattedPayload = outputFormatter.Format(response).ToString();

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
