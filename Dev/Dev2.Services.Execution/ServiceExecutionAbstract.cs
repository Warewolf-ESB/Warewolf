#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.DB;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Unlimited.Framework.Converters.Graph;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using WarewolfParserInterop;
using Dev2.Runtime.Interfaces;
using Warewolf.Data;

namespace Dev2.Services.Execution
{
    public abstract class ServiceExecutionAbstract<TService, TSource> : IServiceExecution
        where TService : Service, new()
        where TSource : Resource, new()
    {
        // Plugins need to handle formatting inside the RemoteObjectHandler 
        // and NOT here otherwise serialization issues occur ;)
        protected readonly ErrorResultTO _errorResult;

        protected ServiceExecutionAbstract(IDSFDataObject dataObj)
            : this(dataObj, true)
        {
        }

        protected ServiceExecutionAbstract(IDSFDataObject dataObj, bool handlesOutputFormatting)
        {
            _errorResult = new ErrorResultTO();
            DataObj = dataObj;
            HandlesOutputFormatting = handlesOutputFormatting;
            _catalog = ResourceCatalog.Instance;
            if (DataObj.ResourceID != Guid.Empty || !string.IsNullOrEmpty(dataObj.ServiceName))
            {
                CreateService(_catalog);
            }
        }

        bool HandlesOutputFormatting { get; }
        protected TSource Source { get; private set; }
        public string InstanceOutputDefintions { get; set; }
        public IDSFDataObject DataObj { get; set; }

        public abstract void BeforeExecution(ErrorResultTO errors);

        public virtual Guid Execute(out ErrorResultTO errors, int update)
        {
            //This execution will throw errors from the constructor
            errors = new ErrorResultTO();
            errors.MergeErrors(_errorResult);
            TryExecuteImpl(out errors, update);
            return DataObj.DataListID;
        }

        public abstract void AfterExecution(ErrorResultTO errors);

        void CreateService(IResourceCatalog catalog)
        {
            if (!GetService(catalog))
            {
                return;
            }

            GetSource(catalog);
        }

        void GetSource(IResourceCatalog catalog)
        {
            Source = catalog.GetResource<TSource>(GlobalConstants.ServerWorkspaceID, Service.Source.ResourceID) ??
                     catalog.GetResource<TSource>(GlobalConstants.ServerWorkspaceID, Service.Source.ResourceName);
            if (Source == null)
            {
                _errorResult.AddError(string.Format(ErrorResource.ErrorRetrievingDBSourceForResource,
                    Service.Source.ResourceID, Service.Source.ResourceName));
            }
        }

        readonly IResourceCatalog _catalog;
        public void GetSource(Guid sourceId)
        {

            if (Source == null)
            {
                var dbSources = _catalog.GetResourceList<DbSource>(GlobalConstants.ServerWorkspaceID);
                Source = dbSources.Cast<TSource>().FirstOrDefault(p => p.ResourceID.Equals(sourceId));
                if (Source == null)
                {
                    _errorResult.AddError(string.Format(ErrorResource.ErrorRetrievingDBSourceForResource,
                        Service?.Source?.ResourceID, Service?.Source?.ResourceName));
                }
            }
        }

        bool GetService(IResourceCatalog catalog)
        {
            Service = catalog.GetResource<TService>(GlobalConstants.ServerWorkspaceID, DataObj.ResourceID) ??
                      catalog.GetResource<TService>(GlobalConstants.ServerWorkspaceID, DataObj.ServiceName);
            if (Service == null)
            {
                _errorResult.AddError(string.Format(ErrorResource.ErrorLoadingResource, DataObj.ResourceID));
                return false;
            }
            return true;
        }

        protected abstract object ExecuteService(int update, out ErrorResultTO errors, IOutputFormatter formater);
        public TService Service { protected get; set; }
        public string InstanceInputDefinitions { get; set; }
        public ICollection<IServiceInput> Inputs { protected get; set; }
        public ICollection<IServiceOutputMapping> Outputs { protected get; set; }

        protected void TryExecuteImpl(out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();

            if (!GetOutputFormatter(out var outputFormatter) && HandlesOutputFormatting)
            {
                errors.AddError(
                    string.Format(ErrorResource.InvalidOutputFormat + "Please edit and remap.",
                        Service.ResourceName));
                return;
            }

            if (HandlesOutputFormatting && outputFormatter == null && !string.IsNullOrEmpty(InstanceOutputDefintions))
            {
                errors.AddError(string.Format(ErrorResource.InvalidOutputFormat, Service.ResourceName));
                return;
            }
            try
            {
                var itrs = new List<IWarewolfIterator>(5);
                IWarewolfListIterator itrCollection = new WarewolfListIterator();
                if (string.IsNullOrEmpty(InstanceInputDefinitions) && string.IsNullOrEmpty(InstanceOutputDefintions))
                {
                    MergeErrors(errors, update, outputFormatter, itrs, itrCollection);
                    return;
                }
                ExecuteImpl(errors, update, outputFormatter, itrs, itrCollection);
            }
            finally
            {
                var disposable = Service as IDisposable;
                disposable?.Dispose();

                // ensure errors bubble up
                errors.MergeErrors(_errorResult);
            }
        }

        private void ExecuteImpl(ErrorResultTO errors, int update, IOutputFormatter outputFormatter, List<IWarewolfIterator> itrs, IWarewolfListIterator itrCollection)
        {
            var method = Service.Method;
            var inputs = method.Parameters;
            if (inputs.Count == 0)
            {
                ExecuteService(out ErrorResultTO invokeErrors, update, outputFormatter);
                errors.MergeErrors(invokeErrors);
            }
            else
            {
                BuildParameterIterators(update, inputs, itrCollection, itrs);

                while (itrCollection.HasMoreData())
                {
                    ExecuteService(itrCollection, itrs, out ErrorResultTO invokeErrors, update, outputFormatter);
                    errors.MergeErrors(invokeErrors);
                }
            }
        }

        private bool GetOutputFormatter(out IOutputFormatter outputFormatter)
        {
            outputFormatter = null;
            try
            {
                if (!string.IsNullOrEmpty(InstanceOutputDefintions))
                {
                    outputFormatter = GetOutputFormatter(Service);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        void MergeErrors(ErrorResultTO errors, int update, IOutputFormatter outputFormatter, List<IWarewolfIterator> itrs, IWarewolfListIterator itrCollection)
        {
            ErrorResultTO invokeErrors;
            if (Inputs != null && Inputs.Count == 0)
            {
                ExecuteService(out invokeErrors, update, outputFormatter);
                errors.MergeErrors(invokeErrors);
            }
            else
            {
                BuildParameterIterators(update, null, itrCollection, itrs);

                while (itrCollection.HasMoreData())
                {
                    ExecuteService(itrCollection, itrs, out invokeErrors, update, outputFormatter);
                    errors.MergeErrors(invokeErrors);
                }
            }
        }

        void BuildParameterIterators(int update, IEnumerable<MethodParameter> inputs, IWarewolfListIterator itrCollection, ICollection<IWarewolfIterator> itrs)
        {
            if (string.IsNullOrEmpty(InstanceInputDefinitions))
            {
                if (Inputs != null)
                {
                    foreach (var sai in Inputs)
                    {
                        AddInput(update, itrCollection, itrs, sai);
                    }
                }
                return;
            }
            var inputDefs = DataListFactory.CreateInputParser().Parse(InstanceInputDefinitions);
            foreach (MethodParameter sai in inputs)
            {
                var val = sai.Name;
                var toInject = "NULL";

                if (val != null)
                {
                    var sai1 = sai;
                    var dev2Definitions = inputDefs.Where(definition => definition.Name == sai1.Name);
                    var definitions = dev2Definitions as IDev2Definition[] ?? dev2Definitions.ToArray();
                    if (definitions.Length == 1)
                    {
                        toInject = DataListUtil.IsEvaluated(definitions[0].RawValue) ? DataListUtil.AddBracketsToValueIfNotExist(definitions[0].RawValue) : definitions[0].RawValue;
                    }
                }
                else
                {
                    if (!sai.EmptyToNull)
                    {
                        toInject = sai.DefaultValue;
                    }
                }
                var paramIterator = new WarewolfIterator(DataObj.Environment.Eval(toInject, update));
                itrCollection.AddVariableToIterateOn(paramIterator);
                itrs.Add(paramIterator);
            }
        }

        void AddInput(int update, IWarewolfListIterator itrCollection, ICollection<IWarewolfIterator> itrs, IServiceInput sai)
        {
            var val = sai.Name;
            string toInject = null;

            if (val != null)
            {
                toInject = sai.Value;
            }
            else
            {
                if (!sai.EmptyIsNull)
                {
                    toInject = "";
                }
            }
            var paramIterator = new WarewolfIterator(DataObj.Environment.Eval(toInject, update));
            itrCollection.AddVariableToIterateOn(paramIterator);
            itrs.Add(paramIterator);
        }

        void ExecuteService(IWarewolfListIterator itrCollection,
            IEnumerable<IWarewolfIterator> itrs, out ErrorResultTO errors, int update, IOutputFormatter formater = null)
        {
            errors = new ErrorResultTO();
            if (Inputs.Any())
            {
                // Loop iterators 
                var pos = 0;
                foreach (var itr in itrs)
                {
                    var injectVal = itrCollection.FetchNextValue(itr);
                    var param = Inputs.ToList()[pos];
                    param.Value = param.EmptyIsNull &&
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
                ExecuteService(out var invokeErrors, update, formater);
                errors.MergeErrors(invokeErrors);
            }
            catch (Exception ex)
            {
                errors.AddError(string.Format(ErrorResource.ServiceExecutionError, ex.StackTrace));
            }
        }

        void ExecuteService(out ErrorResultTO errors, int update, IOutputFormatter formater = null)
        {
            errors = new ErrorResultTO();
            try
            {
                var parameters = new List<MethodParameter>();
                if (Service is WebService)
                {
                    var webService = Service as WebService;
                    if (!string.IsNullOrEmpty(webService.RequestBody))
                    {
                        var methodParameter = new MethodParameter
                        {
                            Name = DataListUtil.RemoveLanguageBrackets(webService.RequestBody),
                            Value = ExecutionEnvironment.WarewolfEvalResultToString(DataObj.Environment.Eval(webService.RequestBody, update))
                        };
                        parameters.Add(methodParameter);
                    }
                    if (!string.IsNullOrEmpty(webService.RequestHeaders))
                    {
                        var methodParameter = new MethodParameter
                        {
                            Name = DataListUtil.RemoveLanguageBrackets(webService.RequestHeaders),
                            Value = ExecutionEnvironment.WarewolfEvalResultToString(DataObj.Environment.Eval(webService.RequestHeaders, update))
                        };
                        parameters.Add(methodParameter);
                    }
                }
                string result;
                if (parameters.Any())
                {
                    result = ExecuteService(update, out errors, formater).ToString();
                }
                else
                {
                    result = ExecuteService(update, out var invokeErrors, formater).ToString();
                    errors.MergeErrors(invokeErrors);
                }
                if (!HandlesOutputFormatting)
                {
                    var formattedPayload = formater?.Format(result).ToString() ?? result;
                    PushXmlIntoEnvironment(formattedPayload, update);
                }
                else
                {
                    PushXmlIntoEnvironment(result, update);
                }
            }
            catch (Exception ex)
            {
                errors.AddError(string.Format(ErrorResource.ServiceExecutionError, ex.StackTrace));
            }
        }

        void PushXmlIntoEnvironment(string input, int update)
        {

            if (input != string.Empty)
            {
                try
                {
                    var toLoad = input.ToCleanXml(); // clean up the rubish ;)
                    var xDoc = new XmlDocument();
                    toLoad = string.Format("<Tmp{0}>{1}</Tmp{0}>", Guid.NewGuid().ToString("N"), toLoad);
                    xDoc.LoadXml(toLoad);

                    if (xDoc.DocumentElement != null)
                    {
                        var children = xDoc.DocumentElement.ChildNodes;

                        IDictionary<string, int> indexCache = new Dictionary<string, int>();

                        // BUG 9626 - 2013.06.11 - TWR: refactored for recursion
                        var outputDefs = DataListFactory.CreateOutputParser().Parse(InstanceOutputDefintions);
                        TryConvert(children, outputDefs, indexCache, update);
                    }
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e.Message, e, GlobalConstants.WarewolfError);
                }
            }
        }

        void TryConvert(XmlNodeList children, IList<IDev2Definition> outputDefs, IDictionary<string, int> indexCache, int update, int level = 0)
        {
            // spin through each element in the XML
            foreach (XmlNode c in children)
            {
                if (c.Name != GlobalConstants.NaughtyTextNode)
                {
                    Convert(outputDefs, indexCache, update, c, level);
                }
            }
        }

        void Convert(IList<IDev2Definition> outputDefs, IDictionary<string, int> indexCache, int update, XmlNode c, int level)
        {
            // scalars and recordset fetch
            if (level > 0)
            {
                Convert(outputDefs, indexCache, update, c);
            }
            else
            {
                if (level == 0)
                {
                    // Only recurse if we're at the first level!!
                    TryConvert(c.ChildNodes, outputDefs, indexCache, update, ++level);
                }
            }
        }

        void Convert(IList<IDev2Definition> outputDefs, IDictionary<string, int> indexCache, int update, XmlNode c)
        {
            var c1 = c;
            var recSetName = outputDefs.Where(definition => definition.RecordSetName == c1.Name);
            var dev2Definitions = recSetName as IDev2Definition[] ?? recSetName.ToArray();
            if (dev2Definitions.Length != 0)
            {
                // fetch recordset index
                var idx = indexCache.TryGetValue(c.Name, out int fetchIdx) ? fetchIdx : 1;
                // process recordset
                var nl = c.ChildNodes;
                foreach (XmlNode subc in nl)
                {
                    // Extract column being mapped to ;)
                    foreach (var definition in dev2Definitions)
                    {
                        AssignWithFrame(update, subc, definition);
                    }

                }
                // update this recordset index
                DataObj.Environment.CommitAssign();
                indexCache[c.Name] = ++idx;
            }
            else
            {
                var scalarName = outputDefs.FirstOrDefault(definition => definition.Name == c1.Name);
                if (scalarName != null)
                {
                    DataObj.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(scalarName.RawValue), UnescapeRawXml(c1.InnerXml), update);
                }
            }
        }

        private void AssignWithFrame(int update, XmlNode subc, IDev2Definition definition)
        {
            if (definition.MapsTo == subc.Name || definition.Name == subc.Name)
            {
                DataObj.Environment.AssignWithFrame(new AssignValue(definition.RawValue, subc.InnerXml), update);
            }
        }

        internal string ODBCParameterIterators(int update, string command)
        {
            var itrs = new List<IWarewolfIterator>(5);
            IWarewolfListIterator itrCollection = new WarewolfListIterator();

            if (string.IsNullOrEmpty(InstanceInputDefinitions))
            {
                var okay = new List<string>();
                var startindex = 0;
                while (command.IndexOf("[[", startindex, StringComparison.Ordinal) != -1)
                {
                    var first = command.IndexOf("[[", startindex, StringComparison.Ordinal);
                    var second = command.IndexOf("]]", first, StringComparison.Ordinal);
                    if (second != -1)
                    {
                        var val = command.Substring(first, (second - first) + 2);
                        okay.Add(val);

                        var toInject = val;
                        var paramIterator = new WarewolfIterator(DataObj.Environment.Eval(toInject, update));

                        itrCollection.AddVariableToIterateOn(paramIterator);
                        itrs.Add(paramIterator);
                        startindex = second;
                    }
                    else
                    {
                        startindex = command.Length - 1;
                    }
                }
                for (int i = 0; i < itrs.Count; i++)
                {
                    var vari = itrCollection.FetchNextValue(itrs[i]);
                    command = command.Replace(okay[i], vari);
                }
            }

            return command;
        }

        string UnescapeRawXml(string innerXml)
        {
            if (innerXml.StartsWith("&lt;") && innerXml.EndsWith("&gt;"))
            {
                return new StringBuilder(innerXml).Unescape().ToString();
            }
            return innerXml;
        }

        IOutputFormatter GetOutputFormatter(TService service) => OutputFormatterFactory.CreateOutputFormatter(service.OutputDescription, "root");

    }
}
