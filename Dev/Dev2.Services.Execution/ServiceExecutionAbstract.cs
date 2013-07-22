using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Services.Execution
{
    public abstract class ServiceExecutionAbstract<TService, TSource> : IServiceExecution
                                                                                         where TService : Service, new()
                                                                                         where TSource:Resource,new()
                                                                                        
    {
        // Plugins need to handle formatting inside the RemoteObjectHandler 
        // and NOT here otherwise serialization issues occur!
        public IDSFDataObject DataObj { get; set; }
        public bool HandlesOutputFormatting { get; private set; }
        readonly ErrorResultTO _errorResult;

        protected ServiceExecutionAbstract(IDSFDataObject dataObj, bool handlesOutputFormatting = true)
        {
            _errorResult = new ErrorResultTO();
            DataObj = dataObj;
            HandlesOutputFormatting = handlesOutputFormatting;
            DataListID = dataObj.DataListID;
            if(DataObj.ResourceID != Guid.Empty)
            {
                CreateService(ResourceCatalog.Instance);
            }
        }

        public Guid DataListID { get; set; }

        public virtual Guid Execute(out ErrorResultTO errors)
        {
            //This execution will throw errors from the constructor
            errors = new ErrorResultTO();
            errors.MergeErrors(_errorResult);
            ExecuteImpl(DataListFactory.CreateDataListCompiler(), out errors);
            return DataObj.DataListID;
        }

        protected void CreateService(ResourceCatalog catalog)
        {
            if (!GetService(catalog)) return;
            GetSource(catalog);
        }

        void GetSource(ResourceCatalog catalog)
        {
            try
            {
                Source = catalog.GetResource<TSource>(DataObj.WorkspaceID, Service.Source.ResourceID);
            }
            catch(Exception)
            {
                Source = catalog.GetResource<TSource>(DataObj.WorkspaceID, Service.Source.ResourceName);
            }
            if(Source == null)
            {
                _errorResult.AddError(string.Format("Error retrieving DBSource for resource ID:{0} and Name:{1}", Service.Source.ResourceID, Service.Source.ResourceName));
            }
        }

        protected virtual bool GetService(ResourceCatalog catalog)
        {
            try
            {
                Service = catalog.GetResource<TService>(DataObj.WorkspaceID, DataObj.ResourceID);
            }
            catch(Exception)
            {
                Service = catalog.GetResource<TService>(DataObj.WorkspaceID, DataObj.ServiceName);
            }
            if(Service == null)
            {
                _errorResult.AddError(string.Format("Error loading resource with ID:{0}", DataObj.ResourceID));
                return false;
            }
            return true;
        }

        public TSource Source { get; set; }

        protected abstract object ExecuteService();

        #region ExecuteImpl

        protected void ExecuteImpl(IDataListCompiler compiler, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            #region Create OutputFormatter

            var outputFormatter = GetOutputFormatter(Service);
            if(HandlesOutputFormatting && outputFormatter == null)
            {
                errors.AddError(string.Format("Output format in service action {0} is invalid.", Service.ResourceName));
                return;
            }

            #endregion

            try
            {
                ErrorResultTO invokeErrors;

                var itrs = new List<IDev2DataListEvaluateIterator>(5);
                var itrCollection = Dev2ValueObjectFactory.CreateIteratorCollection();
                ServiceMethod method = Service.Method;
                List<MethodParameter> inputs = method.Parameters;
                if(inputs.Count == 0)
                {
                    ExecuteServiceAndMergeResultIntoDataList(outputFormatter, compiler, itrCollection, itrs, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                }
                else
                {
                    #region Build iterators for each ServiceActionInput

                    foreach(var sai in inputs)
                    {
                        var val = sai.Name;
                        var toInject = "NULL";

                        if(val != null)
                        {
                            toInject = DataListUtil.AddBracketsToValueIfNotExist(sai.Name);
                        }
                        else if(!sai.EmptyToNull)
                        {
                            toInject = sai.DefaultValue;
                        }

                        var expressionEntry = compiler.Evaluate(DataListID, enActionType.User, toInject, false, out invokeErrors);
                        errors.MergeErrors(invokeErrors);
                        var expressionIterator = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionEntry);
                        itrCollection.AddIterator(expressionIterator);
                        itrs.Add(expressionIterator);
                    }

                    #endregion

                    while(itrCollection.HasMoreData())
                    {
                        ExecuteServiceAndMergeResultIntoDataList(outputFormatter, compiler, itrCollection, itrs, out invokeErrors);
                        errors.MergeErrors(invokeErrors);
                    }
                }
            }
            finally
            {
                var disposable = Service as IDisposable;
                if(disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        public TService Service { get; set; }

        #endregion

        #region ExecuteServiceAndMergeResultIntoDataList

        void ExecuteServiceAndMergeResultIntoDataList(IOutputFormatter outputFormatter, IDataListCompiler compiler, IDev2IteratorCollection itrCollection, IEnumerable<IDev2DataListEvaluateIterator> itrs, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            ErrorResultTO invokeErrors;

            var response = ExecuteService(Service.Method.Parameters, itrCollection, itrs, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            if(invokeErrors.HasErrors())
            {
                return;
            }

            MergeResultIntoDataList(compiler, outputFormatter, response, out invokeErrors);
            errors.MergeErrors(invokeErrors);
        }

        #endregion

        #region ExecuteService

        object ExecuteService(IList<MethodParameter> methodParameters, IDev2IteratorCollection itrCollection, IEnumerable<IDev2DataListEvaluateIterator> itrs, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            if (methodParameters.Any())
            {
                // Loop iterators 
                var pos = 0;
                foreach(var itr in itrs)
                {
                    var injectVal = itrCollection.FetchNextRow(itr);
                    var param = methodParameters[pos];
                    if(param != null)
                    {
                        param.Value = param.EmptyToNull && (injectVal == null || string.Compare(injectVal.TheValue, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0)
                                          ? null : injectVal.TheValue;
                    }
                    pos++;
                }
            }

            try
            {
                return ExecuteService();
            }
            catch(Exception ex)
            {
                errors.AddError(string.Format("Service Execution Error: {0}", ex.Message));
            }
            return null;
        }

        #endregion

        #region MergeResultIntoDataList

        void MergeResultIntoDataList(IDataListCompiler compiler, IOutputFormatter outputFormatter, object result, out ErrorResultTO errors)
        {
                // Format the XML data
                var formattedPayload = outputFormatter != null ? outputFormatter.Format(result).ToString() : result.ToString();
            
                // Create a shape from the service action outputs
                var dlShape = compiler.ShapeDev2DefinitionsToDataList(Service.OutputSpecification, enDev2ArgumentType.Output, false, out errors);
                errors.MergeErrors(errors);

                // Push formatted data into a datalist using the shape from the service action outputs
                var tmpID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), formattedPayload, dlShape, out errors);
                errors.MergeErrors(errors);

                // Attach a parent ID to the newly created datalist
                compiler.SetParentID(tmpID, DataListID);

                // Merge each result into the datalist ;)
                compiler.Merge(DataListID, tmpID, enDataListMergeTypes.Union, enTranslationDepth.Data_With_Blank_OverWrite, false, out errors);

                errors.MergeErrors(errors);
                compiler.ForceDeleteDataListByID(tmpID); // clean up ;)
            
        }

        #endregion

        #region GetOutputFormatter

        protected virtual IOutputFormatter GetOutputFormatter(TService service)
        {
            if(!HandlesOutputFormatting)
            {
                return null;
            }

             return OutputFormatterFactory.CreateOutputFormatter(service.OutputDescription, "root");
        }

        #endregion
    }
}
