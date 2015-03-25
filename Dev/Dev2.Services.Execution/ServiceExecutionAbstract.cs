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
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Unlimited.Framework.Converters.Graph;
using Warewolf.Storage;

namespace Dev2.Services.Execution
{
    public abstract class ServiceExecutionAbstract<TService, TSource> : IServiceExecution
        where TService : Service, new()
        where TSource : Resource, new()
    {
        // Plugins need to handle formatting inside the RemoteObjectHandler 
        // and NOT here otherwise serialization issues occur!
        public readonly ErrorResultTO ErrorResult;

        /// <summary>
        ///     Construction for ServiceExecution
        /// </summary>
        /// <param name="dataObj">DataObject to execute against</param>
        /// <param name="handlesOutputFormatting">
        ///     Does the ServiceExecution handle its own output formatting i.e. is it formatted
        ///     as part of its execution or must it be formatted before merging into the Datalist
        /// </param>
        /// <param name="requiresFormatting">
        ///     Has the execution been put into a DataList already or must its payload be put into the
        ///     DataList
        /// </param>
        protected ServiceExecutionAbstract(IDSFDataObject dataObj, bool handlesOutputFormatting = true,
            bool requiresFormatting = true)
        {
            ErrorResult = new ErrorResultTO();
            DataObj = dataObj;
            HandlesOutputFormatting = handlesOutputFormatting;
            RequiresFormatting = requiresFormatting;
            if (DataObj.ResourceID != Guid.Empty || !string.IsNullOrEmpty(dataObj.ServiceName))
            {
                CreateService(ResourceCatalog.Instance);
            }
        }

        public bool HandlesOutputFormatting { get; private set; }
        public bool RequiresFormatting { get; set; }
        public TSource Source { get; set; }
        public string InstanceOutputDefintions { get; set; }
        public IDSFDataObject DataObj { get; set; }

        public abstract void BeforeExecution(ErrorResultTO errors);

        public virtual Guid Execute(out ErrorResultTO errors)
        {
            //This execution will throw errors from the constructor
            errors = new ErrorResultTO();
            errors.MergeErrors(ErrorResult);
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            ExecuteImpl(compiler, out errors);
            return DataObj.DataListID;
        }

        public abstract void AfterExecution(ErrorResultTO errors);

        protected void CreateService(ResourceCatalog catalog)
        {
            if (!GetService(catalog)) return;
            GetSource(catalog);
        }

        private void GetSource(ResourceCatalog catalog)
        {
            Source = catalog.GetResource<TSource>(GlobalConstants.ServerWorkspaceID, Service.Source.ResourceID) ??
                     catalog.GetResource<TSource>(GlobalConstants.ServerWorkspaceID, Service.Source.ResourceName);
            if (Source == null)
            {
                ErrorResult.AddError(string.Format("Error retrieving DBSource for resource ID:{0} and Name:{1}",
                    Service.Source.ResourceID, Service.Source.ResourceName));
            }
        }

        protected virtual bool GetService(ResourceCatalog catalog)
        {
            Service = catalog.GetResource<TService>(GlobalConstants.ServerWorkspaceID, DataObj.ResourceID) ??
                      catalog.GetResource<TService>(GlobalConstants.ServerWorkspaceID, DataObj.ServiceName);
            if (Service == null)
            {
                ErrorResult.AddError(string.Format("Error loading resource with ID:{0}", DataObj.ResourceID));
                return false;
            }
            return true;
        }

        protected abstract object ExecuteService(List<MethodParameter> methodParameters, out ErrorResultTO errors, IOutputFormatter formater);

        #region ExecuteImpl

        public TService Service { get; set; }
        public string InstanceInputDefinitions { get; set; }

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
            catch (Exception)
            {
                if (HandlesOutputFormatting)
                {
                    errors.AddError(
                        string.Format("Output format in service action {0} is invalid. Please edit and remap.",
                            Service.ResourceName));
                    return;
                }
            }

            if (HandlesOutputFormatting && outputFormatter == null)
            {
                errors.AddError(string.Format("Output format in service action {0} is invalid.", Service.ResourceName));
                return;
            }

            #endregion

            try
            {
                ErrorResultTO invokeErrors;

                var itrs = new List<string>(5);
                IWarewolfListIterator itrCollection = new WarewolfListIterator(DataObj.Environment);
                ServiceMethod method = Service.Method;
                List<MethodParameter> inputs = method.Parameters;
                if (inputs.Count == 0)
                {
                    ExecuteServiceAndMergeResultIntoDataList(outputFormatter,out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                }
                else
                {
                    #region Build iterators for each ServiceActionInput
                    var inputDefs = DataListFactory.CreateInputParser().Parse(InstanceInputDefinitions);
                    foreach (MethodParameter sai in inputs)
                    {
                        string val = sai.Name;
                        string toInject = "NULL";

                        if (val != null)
                        {
                            var sai1 = sai;
                            var dev2Definitions = inputDefs.Where(definition => definition.Name == sai1.Name);
                            var definitions = dev2Definitions as IDev2Definition[] ?? dev2Definitions.ToArray();
                            if (definitions.Count() == 1)
                            {
                                toInject = Data.Util.DataListUtil.AddBracketsToValueIfNotExist(definitions[0].RawValue);    
                            }
                            
                        }
                        else if (!sai.EmptyToNull)
                        {
                            toInject = sai.DefaultValue;
                        }
                        itrCollection.AddVariableToIterateOn(toInject);
                        itrs.Add(toInject);
                    }

                    #endregion

                    while (itrCollection.HasMoreData())
                    {
                        ExecuteServiceAndMergeResultIntoDataList(outputFormatter, compiler, itrCollection, itrs,
                            out invokeErrors);
                        errors.MergeErrors(invokeErrors);
                    }
                }
            }
            finally
            {
                var disposable = Service as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }

                // ensure errors bubble up ;)
                errors.MergeErrors(ErrorResult);
            }
        }

        #endregion

        #region ExecuteServiceAndMergeResultIntoDataList

        private void ExecuteServiceAndMergeResultIntoDataList(IOutputFormatter outputFormatter,
            IDataListCompiler compiler, IWarewolfListIterator itrCollection,
            IEnumerable<string> itrs, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            ErrorResultTO invokeErrors;

            object response = ExecuteService(Service.Method.Parameters, itrCollection, itrs, out invokeErrors,
                outputFormatter);
            errors.MergeErrors(invokeErrors);
            if (errors.HasErrors())
            {
                return;
            }

            MergeResultIntoDataList(compiler, outputFormatter, response, out invokeErrors);
            errors.MergeErrors(invokeErrors);
        }
        
        private void ExecuteServiceAndMergeResultIntoDataList(IOutputFormatter outputFormatter, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            ErrorResultTO invokeErrors;

            ExecuteService(Service.Method.Parameters, out invokeErrors,outputFormatter);
            errors.MergeErrors(invokeErrors);
            if (errors.HasErrors())
            {
                return;
            }

            
            errors.MergeErrors(invokeErrors);
        }

        #endregion

        #region ExecuteService

        private object ExecuteService(IList<MethodParameter> methodParameters, IWarewolfListIterator itrCollection,
            IEnumerable<string> itrs, out ErrorResultTO errors, IOutputFormatter formater = null)
        {
            errors = new ErrorResultTO();
            if (methodParameters.Any())
            {
                // Loop iterators 
                int pos = 0;
                foreach (string itr in itrs)
                {
                    string injectVal = itrCollection.FetchNextValue(itr);
                    MethodParameter param = methodParameters[pos];
                    

                        param.Value = param.EmptyToNull &&
                                      (injectVal == null ||
                                       string.Compare(injectVal, string.Empty,
                                           StringComparison.InvariantCultureIgnoreCase) == 0)
                            ? null
                            : injectVal;
                    
                    pos++;
                }
            }

            try
            {
                ErrorResultTO invokeErrors;
                ExecuteService(methodParameters,out invokeErrors, formater);
                errors.MergeErrors(invokeErrors);
                return Guid.NewGuid();
            }
            catch (Exception ex)
            {
                errors.AddError(string.Format("Service Execution Error: {0}", ex.Message));
            }
            return null;
        }
        
        private void ExecuteService(IEnumerable<MethodParameter> methodParameters, out ErrorResultTO errors, IOutputFormatter formater = null)
        {
            errors = new ErrorResultTO();
            try
            {
                var parameters = methodParameters as IList<MethodParameter> ?? methodParameters.ToList();
                string result;
                if (parameters.Any())
                {
                    result = ExecuteService(parameters.ToList(), out errors, formater).ToString();
                }
                else
                {
                    ErrorResultTO invokeErrors;
                    result = ExecuteService(new List<MethodParameter>(), out invokeErrors, formater).ToString();
                    errors.MergeErrors(invokeErrors);
                }
                if (!HandlesOutputFormatting)
                {
                    var formattedPayload = formater != null
                            ? formater.Format(result).ToString()
                            : result;
                    PushXmlIntoEnvironment(formattedPayload);
                }
            }
            catch (Exception ex)
            {
                errors.AddError(string.Format("Service Execution Error: {0}", ex.Message));
            }
        }

        #endregion

        #region MergeResultIntoDataList

        private void MergeResultIntoDataList(IDataListCompiler compiler, IOutputFormatter outputFormatter, object result,
            out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            // NOTE : This is only used by Plugin Services and is 1 of 4 locations that now needs to be updated should the DataList or execution model change ;)

            // Format the XML data
            if (RequiresFormatting)
            {
                if (result == null)
                {
                    return;
                }

                try
                {
                    errors = new ErrorResultTO();
                    ErrorResultTO invokeErrors;
                    string formattedPayload = result.ToString();

                    if (!HandlesOutputFormatting)
                    {
                        formattedPayload = outputFormatter != null
                            ? outputFormatter.Format(result).ToString()
                            : result.ToString();
                    }

                    // Create a shape from the service action outputs
                    StringBuilder dlShape = compiler.ShapeDev2DefinitionsToDataList(Service.OutputSpecification,
                        enDev2ArgumentType.Output, false, out invokeErrors);
                    errors.MergeErrors(invokeErrors);

                    // Push formatted data into a datalist using the shape from the service action outputs
                    Guid shapeDataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
                        formattedPayload.ToStringBuilder(), dlShape, out invokeErrors);
                    errors.MergeErrors(invokeErrors);

                    // This merge op is killing the alias data....
                    // We need to account for alias ops too ;)
                    compiler.SetParentID(shapeDataListID, DataObj.DataListID);

                    compiler.PopulateDataList(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags),
                        InstanceOutputDefintions, InstanceOutputDefintions, shapeDataListID, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                }
                catch (Exception)
                {
                    errors.AddError(
                        "Data Format Error : It is likely that you tested with one format yet the service is returning another. IE you tested with XML and it now returns JSON");
                }
            }
        }


        public void PushXmlIntoEnvironment(string input)
        {

            if (input != string.Empty)
            {
                try
                {
                    string toLoad = Data.Util.DataListUtil.StripCrap(input); // clean up the rubish ;)
                    XmlDocument xDoc = new XmlDocument();
                    toLoad = string.Format("<Tmp{0}>{1}</Tmp{0}>", Guid.NewGuid().ToString("N"), toLoad);
                    xDoc.LoadXml(toLoad);

                    if (xDoc.DocumentElement != null)
                    {
                        XmlNodeList children = xDoc.DocumentElement.ChildNodes;

                        IDictionary<string, int> indexCache = new Dictionary<string, int>();

                        // BUG 9626 - 2013.06.11 - TWR: refactored for recursion
                        var outputDefs = DataListFactory.CreateOutputParser().Parse(InstanceOutputDefintions);
                        TryConvert(children, outputDefs, indexCache);
                    }
                }
                catch (Exception e)
                {
                    Dev2Logger.Log.Error(e.Message,e);
                    // if use passed in empty input they only wanted the shape ;)
                    if (input.Length > 0)
                    {
                    }
                }
            }
        }
      void TryConvert(XmlNodeList children, IList<IDev2Definition> outputDefs, IDictionary<string, int> indexCache, int level = 0)
        {
            // spin through each element in the XML
            foreach(XmlNode c in children)
            {
                if(c.Name != GlobalConstants.NaughtyTextNode)
                {
                    // scalars and recordset fetch
                    WarewolfDataEvaluationCommon.WarewolfEvalResult warewolfEvalResult = null;
                    try
                    {
                        warewolfEvalResult = DataObj.Environment.Eval(DataListUtil.AddBracketsToValueIfNotExist(c.Name));
                        if (warewolfEvalResult.IsWarewolfAtomResult && level==0)
                        {
                            var checkNullResult = warewolfEvalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                            if (checkNullResult != null && checkNullResult.Item.IsNothing)
                            {
                                warewolfEvalResult = null;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Log.Error(e.Message,e);
                    }
                    if(warewolfEvalResult!=null)
                    {
                        if (warewolfEvalResult.IsWarewolfAtomListresult)
                        {
                            // fetch recordset index
                            int fetchIdx;
                            var idx = indexCache.TryGetValue(c.Name, out fetchIdx) ? fetchIdx : 1;
                            // process recordset
                            var nl = c.ChildNodes;
                            foreach(XmlNode subc in nl)
                            {
                                // Extract column being mapped to ;)

//                                var theCol = entry.Columns.FirstOrDefault(col => col.ColumnName == subc.Name);
//                                var dir = enDev2ColumnArgumentDirection.None;
//                                if(theCol != null)
//                                {
//                                    dir = theCol.ColumnIODirection;
//                                }
//
//                                if(CanMapValue(onlyMapInputs, dir))
//                                {
//                                    entry.TryPutRecordItemAtIndex(Dev2BinaryDataListFactory.CreateBinaryItem(subc.InnerXml, c.Name, subc.Name, idx), idx, out error);
//                                }
                                var displayExpression = Data.Util.DataListUtil.AddBracketsToValueIfNotExist(Data.Util.DataListUtil.CreateRecordsetDisplayValue(c.Name, subc.Name, idx.ToString()));
                                DataObj.Environment.Assign(displayExpression, subc.InnerXml);
                            }
                            // update this recordset index
                            indexCache[c.Name] = ++idx;
                        }
                        else if (warewolfEvalResult.IsWarewolfAtomResult)
                        {
                            DataObj.Environment.Assign(Data.Util.DataListUtil.AddBracketsToValueIfNotExist(c.Name), c.InnerXml);
                        }
//                        else if(CanMapValue(onlyMapInputs, entry.ColumnIODirection))
//                        {
//                            // process scalar
//                            entry.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(c.InnerXml, c.Name), out error);
//
//                            if(!string.IsNullOrEmpty(error))
//                            {
//                                errors.AddError(error);
//                            }
//                        }
                    }
                    else
                    {
                        if(level == 0)
                        {
                            // Only recurse if we're at the first level!!
                            TryConvert(c.ChildNodes,outputDefs, indexCache, ++level);
                        }                        
                    }
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
