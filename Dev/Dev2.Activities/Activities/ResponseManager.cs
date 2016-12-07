using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.String.Json;
using WarewolfParserInterop;
// ReSharper disable ParameterTypeCanBeEnumerable.Local

namespace Dev2.Activities
{
    public class ResponseManager : IResponseManager
    {
        public bool IsObject { get; set; }
        public string ObjectName { get; set; }
        public IOutputDescription OutputDescription { get; set; }
        public ICollection<IServiceOutputMapping> Outputs { get; set; }

        public void PushResponseIntoEnvironment(string input, int update, IDSFDataObject dataObj,bool formatResult = true)
        {
            if (dataObj == null)
            {
                throw new ArgumentNullException(nameof(dataObj));
            }

            try
            {
                if (IsObject)
                {
                    var jContainer = JsonConvert.DeserializeObject(input) as JContainer;
                    dataObj.Environment.AddToJsonObjects(ObjectName, jContainer);
                }
                else
                {
                    if(Outputs==null || Outputs.Count == 0)
                    {
                        return;
                    }
                    IOutputFormatter formater = null;
                    if (OutputDescription != null)
                    {

                        int i = 0;
                        foreach (var serviceOutputMapping in Outputs)
                        {
                            OutputDescription.DataSourceShapes[0].Paths[i].OutputExpression = DataListUtil.AddBracketsToValueIfNotExist(serviceOutputMapping.MappedTo);
                            i++;
                        }
                        if (OutputDescription.DataSourceShapes.Count == 1 && OutputDescription.DataSourceShapes[0].Paths.All(a => a is StringPath))
                        {
                            var serviceOutputMapping = Outputs.First();
                            if (serviceOutputMapping != null)
                            {
                                dataObj.Environment.Assign(serviceOutputMapping.MappedTo, input, update);
                            }
                            return;
                        }
                        formater = OutputFormatterFactory.CreateOutputFormatter(OutputDescription);
                    }
                    if (!string.IsNullOrEmpty(input))
                    {
                        FormatForOutput(input, update, dataObj, formatResult, formater);
                    }
                }
            }
            catch (Exception e)
            {
                dataObj.Environment.AddError(e.Message);
                Dev2Logger.Error(e.Message, e);
            }
        }

        private void FormatForOutput(string input, int update, IDSFDataObject dataObj, bool formatResult, IOutputFormatter formater)
        {
            var formattedInput = input;
            if(formater != null && formatResult)
            {
                formattedInput = formater.Format(input).ToString();
            }

            XmlDocument xDoc = new XmlDocument();
            formattedInput = string.Format("<Tmp{0}>{1}</Tmp{0}>", Guid.NewGuid().ToString("N"), formattedInput);
            xDoc.LoadXml(formattedInput);

            if(xDoc.DocumentElement != null)
            {
                XmlNodeList children = xDoc.DocumentElement.ChildNodes;
                IDictionary<string, int> indexCache = new Dictionary<string, int>();
                var outputDefs =
                    Outputs.Select(
                        a =>
                            new Dev2Definition(a.MappedFrom, a.MappedTo, "", a.RecordSetName, true, "", true,
                                a.MappedTo) as IDev2Definition).ToList();
                TryConvert(children, outputDefs, indexCache, update, dataObj);
            }
        }

        public string UnescapeRawXml(string innerXml)
        {
            if (innerXml.StartsWith("&lt;") && innerXml.EndsWith("&gt;"))
            {
                return new StringBuilder(innerXml).Unescape().ToString();
            }
            return innerXml;
        }

        public void TryConvert(XmlNodeList children, IList<IDev2Definition> outputDefs, IDictionary<string, int> indexCache, int update, IDSFDataObject dataObj, int level = 0)
        {
            foreach (XmlNode c in children)
            {
                if (level > 0)
                {
                    var c1 = c;
                    var recSetName = outputDefs.Where(definition => definition.RecordSetName == c1.Name);
                    var dev2Definitions = recSetName as IDev2Definition[] ?? recSetName.ToArray();
                    if (dev2Definitions.Length != 0)
                    {
                        int fetchIdx;
                        var idx = indexCache.TryGetValue(c.Name, out fetchIdx) ? fetchIdx : 1;
                        var nl = c.ChildNodes;
                        foreach (XmlNode subc in nl)
                        {
                            foreach (var definition in dev2Definitions)
                            {
                                if (definition.MapsTo == subc.Name || definition.Name == subc.Name)
                                {
                                    if (update == 0)
                                    {
                                        update = fetchIdx;
                                    }
                                    dataObj.Environment.AssignWithFrame(new AssignValue(definition.RawValue, UnescapeRawXml(subc.InnerXml)), update);
                                }
                            }
                        }
                        dataObj.Environment.CommitAssign();
                        indexCache[c.Name] = ++idx;
                    }
                    else
                    {
                        MapScalarValue(outputDefs, update, dataObj, c1);
                    }
                }
                else
                {
                    if (level == 0)
                    {
                        TryConvert(c.ChildNodes, outputDefs, indexCache, update, dataObj, ++level);
                    }
                }
            }
        }

        private void MapScalarValue(IList<IDev2Definition> outputDefs, int update, IDSFDataObject dataObj, XmlNode c1)
        {
            var scalarName = outputDefs.FirstOrDefault(definition => definition.Name == c1.Name);
            if(scalarName != null)
            {
                dataObj.Environment.AssignWithFrame(new AssignValue(DataListUtil.AddBracketsToValueIfNotExist(scalarName.RawValue), UnescapeRawXml(c1.InnerXml)), update);
            }
        }
    }
}