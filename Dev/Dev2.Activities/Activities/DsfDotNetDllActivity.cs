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
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Framework.Converters.Graph;
using Warewolf.Core;
using WarewolfParserInterop;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dev2.Activities
{
    [ToolDescriptorInfo("DotNetDll", "DotNet DLL Connector", ToolType.Native, "6AEB1038-6332-46F9-8BDD-641DE4EA038E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfDotNetDllActivity : DsfActivity
    {
        public IPluginAction Method { get; set; }
        public INamespaceItem Namespace { get; set; }

        public IOutputDescription OutputDescription { get; set; }

        public DsfDotNetDllActivity()
        {
            Type = "DotNet DLL Connector";
            DisplayName = "DotNet DLL Connector";
        }


        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();
            if (Namespace == null)
            {
                errors.AddError("No Namespace Selected.");
                return;
            }
            if (Method == null)
            {
                errors.AddError("No Method Selected.");
                return;
            }
            ExecuteService(update, out errors, Method, Namespace, dataObject, OutputFormatterFactory.CreateOutputFormatter(OutputDescription));
        }

        protected void ExecuteService(int update, out ErrorResultTO errors, IPluginAction method, INamespaceItem namespaceItem, IDSFDataObject dataObject, IOutputFormatter formater = null)
        {
            errors = new ErrorResultTO();

            var args = new PluginInvokeArgs
            {
                AssemblyLocation = Namespace.AssemblyLocation,
                AssemblyName = Namespace.AssemblyName, 
                Fullname = namespaceItem.FullName,
                Method = method.Method,
                Parameters = method.Inputs.Select(a=>new MethodParameter{EmptyToNull = a.EmptyIsNull,IsRequired = a.RequiredField,Name = a.Name,Value = a.Value,Type = a.TypeName}).ToList(),
                OutputFormatter = formater
            };

            try
            {
                var result = PluginServiceExecutionFactory.InvokePlugin(args).ToString();
                PushXmlIntoEnvironment(result,update,dataObject);
            }
            catch (Exception e)
            {
                errors.AddError(e.Message);
            }
        }

        public void PushXmlIntoEnvironment(string input, int update,IDSFDataObject dataObj)
        {

            if (input != string.Empty)
            {
                try
                {
                    XmlDocument xDoc = new XmlDocument();
                    input = string.Format("<Tmp{0}>{1}</Tmp{0}>", Guid.NewGuid().ToString("N"), input);
                    xDoc.LoadXml(input);

                    if (xDoc.DocumentElement != null)
                    {
                        XmlNodeList children = xDoc.DocumentElement.ChildNodes;
                        IDictionary<string, int> indexCache = new Dictionary<string, int>();
                        var outputDefs = Outputs.Select(a => new Dev2Definition(a.MappedFrom, a.MappedTo, "", a.RecordSetName, true, "", true, a.MappedTo) as IDev2Definition).ToList();
                        TryConvert(children, outputDefs, indexCache,  update,dataObj);
                    }
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e.Message, e);                 
                }
            }
        }

        void TryConvert(XmlNodeList children, IList<IDev2Definition> outputDefs, IDictionary<string, int> indexCache, int update, IDSFDataObject dataObj, int level = 0)
        {
            // spin through each element in the XML
            foreach (XmlNode c in children)
            {
                if (c.Name != GlobalConstants.NaughtyTextNode)
                {
                    // scalars and recordset fetch
                    if (level > 0)
                    {
                        var c1 = c;
                        var recSetName = outputDefs.Where(definition => definition.RecordSetName == c1.Name);
                        var dev2Definitions = recSetName as IDev2Definition[] ?? recSetName.ToArray();
                        if (dev2Definitions.Length != 0)
                        {
                            // fetch recordset index
                            int fetchIdx;
                            var idx = indexCache.TryGetValue(c.Name, out fetchIdx) ? fetchIdx : 1;
                            // process recordset
                            var nl = c.ChildNodes;
                            foreach (XmlNode subc in nl)
                            {
                                // Extract column being mapped to ;)
                                foreach (var definition in dev2Definitions)
                                {
                                    if (definition.MapsTo == subc.Name || definition.Name == subc.Name)
                                    {
                                        dataObj.Environment.AssignWithFrame(new AssignValue(definition.RawValue, subc.InnerXml), update);
                                    }
                                }
                            }
                            // update this recordset index
                            dataObj.Environment.CommitAssign();
                            indexCache[c.Name] = ++idx;
                        }
                        else
                        {
                            var scalarName = outputDefs.FirstOrDefault(definition => definition.Name == c1.Name);
                            if (scalarName != null)
                            {
                                dataObj.Environment.AssignWithFrame(new AssignValue(DataListUtil.AddBracketsToValueIfNotExist(scalarName.RawValue), UnescapeRawXml(c1.InnerXml)), update);
                            }
                        }
                    }
                    else
                    {
                        if (level == 0)
                        {
                            // Only recurse if we're at the first level!!
                            TryConvert(c.ChildNodes, outputDefs, indexCache, update,dataObj, ++level);
                        }
                    }
                }
            }
        }

        string UnescapeRawXml(string innerXml)
        {
            if (innerXml.StartsWith("&lt;") && innerXml.EndsWith("&gt;"))
            {
                return new StringBuilder(innerXml).Unescape().ToString();
            }
            return innerXml;
        }
        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

    }
}
