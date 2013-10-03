using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
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
    // BUG 9619 - 2013.06.05 - TWR - Refactored
    public class WebServiceContainer : EsbExecutionContainer
    {
        public WebServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
        }

        protected WebService CreateService()
        {
            var sourceXml = XElement.Parse(ServiceAction.Source.ResourceDefinition);
            var serviceXml = ServiceAction.Parent.xmlData;

            return new WebService(serviceXml) { Source = new WebSource(sourceXml) };
        }

        protected virtual void ExecuteWebRequest(WebService service)
        {
            WebServices.ExecuteRequest(service);
        }

        public override Guid Execute(out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            try
            {
                var invokeWebService = CreateService();

                ErrorResultTO invokeErrors;
                BindInputs(out invokeErrors, ref invokeWebService);
                errors.MergeErrors(invokeErrors);

                WebServices.ExecuteRequest(invokeWebService);

                var result = Scrubber.Scrub(invokeWebService.RequestResponse);

                var outputFormatter = GetOutputFormatter();

                PushResultsIntoDataList(result, outputFormatter, out invokeErrors);
                errors.MergeErrors(invokeErrors);

            }
            catch (Exception e)
            {
                errors.AddError(e.Message);
            }

            return Guid.Empty;
        }

        #region Input Binding

        /// <summary>
        /// Binds the inputs. - A wee bit silly, but this is a TU emergency port ;)
        /// </summary>
        /// <param name="errors">The errors.</param>
        /// <param name="invokeWebService">The invoke web service.</param>
        void BindInputs(out ErrorResultTO errors, ref WebService invokeWebService)
        {
            errors = new ErrorResultTO();
            var compiler = DataListFactory.CreateDataListCompiler();
            var itrs = new List<IDev2DataListEvaluateIterator>(5);
            var itrCollection = Dev2ValueObjectFactory.CreateIteratorCollection();

            if(ServiceAction.ServiceActionInputs.Count == 0)
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

                    ErrorResultTO invokeErrors;
                    var expressionEntry = compiler.Evaluate(DataObject.DataListID,
                                                            DataList.Contract.enActionType.User, toInject, false,
                                                            out invokeErrors);
                    
                    errors.MergeErrors(invokeErrors);
                    var expressionIterator = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionEntry);
                    itrCollection.AddIterator(expressionIterator);
                    itrs.Add(expressionIterator);
                }

                // Loop iterators 
                var pos = 0;
                foreach(var itr in itrs)
                {
                    var injectVal = itrCollection.FetchNextRow(itr);
                    var sai = invokeWebService.Method.Parameters[pos];
                    var param = invokeWebService.Method.Parameters[pos];
                    if(param != null)
                    {
                        param.Value = sai.EmptyToNull && (injectVal == null || string.Compare(injectVal.TheValue, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0)
                                          ? null : injectVal.TheValue;
                    }
                    pos++;
                }

                #endregion

            }
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

        #region MergeResultIntoDataList

        void PushResultsIntoDataList(string result, IOutputFormatter outputFormatter, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            ErrorResultTO invokeErrors;

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            var formattedPayload = outputFormatter != null ? outputFormatter.Format(result).ToString() : result;


            // 1st create table shape ;)
            var targetShape = compiler.GenerateDataListFromDefs(ServiceAction.OutputSpecification, enDev2ArgumentType.Output, false, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            var shapeDataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), targetShape, string.Empty, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            // TODO : Convert to DataTable ;)
            var shapeDataList = compiler.FetchBinaryDataList(shapeDataListID, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            if (shapeDataList != null)
            {
                var userEntries = shapeDataList.FetchAllEntries();

                foreach (var key in userEntries)
                {
                    DataTable tmpTbl = new DataTable();
                    // TODO : Process each userEntry and build up a DataTable
                    
                    
                    // Flush to DataList ;)
                    compiler.PopulateDataList(DataListFormat.CreateFormat(GlobalConstants._DATATABLE), tmpTbl, DataObject.DataListID, out invokeErrors);
                    errors.MergeErrors(invokeErrors);

                }
            }

            compiler.DeleteDataListByID(shapeDataListID);
        }

        #endregion

    }
}
