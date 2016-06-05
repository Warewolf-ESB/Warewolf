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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.String.Json;
using WarewolfParserInterop;

namespace Dev2.Activities
{
    public class WebResponseManager : IWebResponseManager
    {
        public bool IsObject { get; set; }
        public string ObjectName { get; set; }
        public IOutputDescription OutputDescription { get; set; }
        public ICollection<IServiceOutputMapping> Outputs { get; set; }

        public void PushResponseIntoEnvironment(string input, int update, IDSFDataObject dataObj)
        {
            try
            {
                if (IsObject)
                {
                    var jContainer = JsonConvert.DeserializeObject(input) as JObject;
                    dataObj.Environment.AddToJsonObjects(ObjectName, jContainer);
                }
                else
                {

                    if (OutputDescription == null)
                    {
                        dataObj.Environment.AddError("There are no outputs");
                        return;
                    }
                    int i = 0;
                    foreach (var serviceOutputMapping in Outputs)
                    {
                        OutputDescription.DataSourceShapes[0].Paths[i].OutputExpression = DataListUtil.AddBracketsToValueIfNotExist(serviceOutputMapping.MappedFrom);
                        i++;
                    }
                    if (OutputDescription.DataSourceShapes.Count == 1 && OutputDescription.DataSourceShapes[0].Paths.All(a => a is StringPath))
                    {
                        dataObj.Environment.Assign(Outputs.First().MappedTo, input, update);
                        return;
                    }
                    var formater = OutputFormatterFactory.CreateOutputFormatter(OutputDescription);
                    if (string.IsNullOrEmpty(input))
                    {
                        dataObj.Environment.AddError("No Web Response received");
                    }
                    else
                    {
                        input = formater.Format(input).ToString();

                        string toLoad = DataListUtil.StripCrap(input); // clean up the rubish ;)
                        XmlDocument xDoc = new XmlDocument();
                        toLoad = string.Format("<Tmp{0}>{1}</Tmp{0}>", Guid.NewGuid().ToString("N"), toLoad);
                        xDoc.LoadXml(toLoad);

                        if (xDoc.DocumentElement != null)
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
                }
            }
            catch (Exception e)
            {
                dataObj.Environment.AddError(e.Message);
                Dev2Logger.Error(e.Message, e);
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
            // spin through each element in the XML
            foreach (XmlNode c in children)
            {
                // scalars and recordset fetch
                if (level > 0)
                {
                    var c1 = c;

                    var nameToMatch = c1.Name;
                    var useValue = false;
                    if (c.Name == GlobalConstants.NaughtyTextNode)
                    {
                        if (c1.ParentNode != null)
                        {
                            nameToMatch = c1.ParentNode.Name;
                            useValue = true;
                        }
                    }
                    var scalarName = outputDefs.FirstOrDefault(definition => definition.Name == nameToMatch);
                    if (scalarName != null)
                    {
                        var value = UnescapeRawXml(c1.InnerXml);
                        if (useValue)
                        {
                            value = UnescapeRawXml(c1.Value);
                        }
                        dataObj.Environment.AssignWithFrame(new AssignValue(DataListUtil.AddBracketsToValueIfNotExist(scalarName.MapsTo), value), update);
                    }
                }
                else
                {
                    if (level == 0)
                    {
                        // Only recurse if we're at the first level!!
                        TryConvert(c.ChildNodes, outputDefs, indexCache, update, dataObj, ++level);
                    }
                }
            }
        }
    }
}