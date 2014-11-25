
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Unlimited.Framework.Converters.Graph;

namespace Dev2.Services.Execution
{
    public abstract class ServiceExecutionAbstract<TService, TSource> : IServiceExecution
        where TService : Service, new()
        where TSource : Resource, new()
    {
        // Plugins need to handle formatting inside the RemoteObjectHandler 
        // and NOT here otherwise serialization issues occur!
        public string InstanceOutputDefintions { get; set; }
        public IDSFDataObject DataObj { get; set; }
        public bool HandlesOutputFormatting { get; private set; }
        public bool RequiresFormatting { get; set; }
        public readonly ErrorResultTO ErrorResult;

        /// <summary>
        /// Construction for ServiceExecution
        /// </summary>
        /// <param name="dataObj">DataObject to execute against</param>
        /// <param name="handlesOutputFormatting">Does the ServiceExecution handle its own output formatting i.e. is it formatted as part of its execution or must it be formatted before merging into the Datalist</param>
        /// <param name="requiresFormatting">Has the execution been put into a DataList already or must its payload be put into the DataList</param>
        protected ServiceExecutionAbstract(IDSFDataObject dataObj, bool handlesOutputFormatting = true, bool requiresFormatting = true)
        {
            ErrorResult = new ErrorResultTO();
            DataObj = dataObj;
            HandlesOutputFormatting = handlesOutputFormatting;
            RequiresFormatting = requiresFormatting;
            if(DataObj.ResourceID != Guid.Empty || !string.IsNullOrEmpty(dataObj.ServiceName))
            {
                CreateService(ResourceCatalog.Instance);
            }
        }

        public abstract void BeforeExecution(ErrorResultTO errors);
        public virtual Guid Execute(out ErrorResultTO errors)
        {
            //This execution will throw errors from the constructor
            errors = new ErrorResultTO();
            errors.MergeErrors(ErrorResult);
            var compiler = DataListFactory.CreateDataListCompiler();
            ExecuteImpl(compiler, out errors);
            return DataObj.DataListID;
        }
        public abstract void AfterExecution(ErrorResultTO errors);
        protected void CreateService(ResourceCatalog catalog)
        {
            if(!GetService(catalog)) return;
            GetSource(catalog);
        }

        void GetSource(ResourceCatalog catalog)
        {
            Source = catalog.GetResource<TSource>(GlobalConstants.ServerWorkspaceID, Service.Source.ResourceID) ?? catalog.GetResource<TSource>(GlobalConstants.ServerWorkspaceID, Service.Source.ResourceName);
            if(Source == null)
            {
                ErrorResult.AddError(string.Format("Error retrieving DBSource for resource ID:{0} and Name:{1}", Service.Source.ResourceID, Service.Source.ResourceName));
            }
        }

        protected virtual bool GetService(ResourceCatalog catalog)
        {
            Service = catalog.GetResource<TService>(GlobalConstants.ServerWorkspaceID, DataObj.ResourceID) ?? catalog.GetResource<TService>(GlobalConstants.ServerWorkspaceID, DataObj.ServiceName);
            if(Service == null)
            {
                ErrorResult.AddError(string.Format("Error loading resource with ID:{0}", DataObj.ResourceID));
                return false;
            }
            return true;
        }

        public TSource Source { get; set; }

        protected abstract object ExecuteService(out ErrorResultTO errors, IOutputFormatter formater = null);

        #region ExecuteImpl

        protected void ExecuteImpl(IDataListCompiler compiler, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            #region Create OutputFormatter
            // ReSharper disable RedundantAssignment
            IOutputFormatter outputFormatter = null;
            // ReSharper restore RedundantAssignment

            try
            {
                outputFormatter = GetOutputFormatter(Service);
            }
            catch(Exception)
            {
                if(HandlesOutputFormatting)
                {
                    errors.AddError(string.Format("Output format in service action {0} is invalid. Please edit and remap.", Service.ResourceName));
                    return;
                }
            }

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

                        var expressionEntry = compiler.Evaluate(DataObj.DataListID, enActionType.User, toInject, false, out invokeErrors);
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

                // ensure errors bubble up ;)
                errors.MergeErrors(ErrorResult);
            }
        }

        public TService Service { get; set; }

        #endregion

        #region ExecuteServiceAndMergeResultIntoDataList

        void ExecuteServiceAndMergeResultIntoDataList(IOutputFormatter outputFormatter, IDataListCompiler compiler, IDev2IteratorCollection itrCollection, IEnumerable<IDev2DataListEvaluateIterator> itrs, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            ErrorResultTO invokeErrors;

            var response = ExecuteService(Service.Method.Parameters, itrCollection, itrs, out invokeErrors, outputFormatter);
            errors.MergeErrors(invokeErrors);
            if(errors.HasErrors())
            {
                return;
            }

            // TODO : This needs to move to the other side of the Marshaled Invoke
            MergeResultIntoDataList(compiler, outputFormatter, response, out invokeErrors);
            errors.MergeErrors(invokeErrors);
        }

        #endregion

        #region ExecuteService

        object ExecuteService(IList<MethodParameter> methodParameters, IDev2IteratorCollection itrCollection, IEnumerable<IDev2DataListEvaluateIterator> itrs, out ErrorResultTO errors, IOutputFormatter formater = null)
        {
            errors = new ErrorResultTO();
            if(methodParameters.Any())
            {
                // Loop iterators 
                var pos = 0;
                foreach(var itr in itrs)
                {
                    var injectVal = itrCollection.FetchNextRow(itr);
                    var param = methodParameters[pos];
                    if(param != null)
                    {
                        string theValue;
                        try
                        {
                            theValue = injectVal.TheValue;
                        }
                        catch(Exception)
                        {
                            theValue = "";
                        }

                        param.Value = param.EmptyToNull && (injectVal == null || string.Compare(theValue, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0)
                                          ? null : theValue;
                    }
                    pos++;
                }
            }

            try
            {
                ErrorResultTO invokeErrors;
                var result = ExecuteService(out invokeErrors, formater);
                errors.MergeErrors(invokeErrors);
                return result;
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
            errors = new ErrorResultTO();

            // NOTE : This is only used by Plugin Services and is 1 of 4 locations that now needs to be updated should the DataList or execution model change ;)

            // Format the XML data
            if(RequiresFormatting)
            {
                if(result == null)
                {
                    return;
                }

                try
                {

                    errors = new ErrorResultTO();
                    ErrorResultTO invokeErrors;
                    var formattedPayload = result.ToString();

                    if(!HandlesOutputFormatting)
                    {
                        formattedPayload = outputFormatter != null ? outputFormatter.Format(result).ToString() : result.ToString();
                    }

                    // Create a shape from the service action outputs
                    var dlShape = compiler.ShapeDev2DefinitionsToDataList(Service.OutputSpecification, enDev2ArgumentType.Output, false, out invokeErrors);
                    errors.MergeErrors(invokeErrors);

                    // Push formatted data into a datalist using the shape from the service action outputs
                    var shapeDataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), formattedPayload.ToStringBuilder(), dlShape, out invokeErrors);
                    errors.MergeErrors(invokeErrors);

                    // This merge op is killing the alias data....
                    // We need to account for alias ops too ;)
                    compiler.SetParentID(shapeDataListID, DataObj.DataListID);

                    compiler.PopulateDataList(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), InstanceOutputDefintions, InstanceOutputDefintions, shapeDataListID, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                }
                catch(Exception)
                {
                    errors.AddError("Data Format Error : It is likely that you tested with one format yet the service is returning another. IE you tested with XML and it now returns JSON");
                }
            }

        }

        #endregion

        #region GetOutputFormatter

        protected virtual IOutputFormatter GetOutputFormatter(TService service)
        {
            return OutputFormatterFactory.CreateOutputFormatter(service.OutputDescription, "root");
        }

        #endregion
    }
}
