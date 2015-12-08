using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Data;
using Dev2.Data.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Storage;
using WarewolfParserInterop;

namespace Dev2
{
    public static class ExecutionEnvironmentUtils
    {
        public static string GetXmlOutputFromEnvironment(IDSFDataObject dataObject,string dataList,int update)
        {
            var environment = dataObject.Environment;
            var dataListTO = new DataListTO(dataList);
            StringBuilder result = new StringBuilder("<DataList>");
            var scalarOutputs = dataListTO.Outputs.Where(s => !DataListUtil.IsValueRecordset(s));
            var recSetOutputs = dataListTO.Outputs.Where(DataListUtil.IsValueRecordset);
            var groupedRecSets = recSetOutputs.GroupBy(DataListUtil.ExtractRecordsetNameFromValue);
            foreach (var groupedRecSet in groupedRecSets)
            {
                var i = 1;
                var warewolfListIterators = new WarewolfListIterator();
                Dictionary<string, IWarewolfIterator> iterators = new Dictionary<string, IWarewolfIterator>();
                foreach (var name in groupedRecSet)
                {
                    var warewolfEvalResult = WarewolfDataEvaluationCommon.WarewolfEvalResult.NewWarewolfAtomResult(DataASTMutable.WarewolfAtom.Nothing);
                    try
                    {
                        warewolfEvalResult = environment.Eval(name, update,false);
                    }
                    // ReSharper disable once RESP510236
                    // ReSharper disable once RESP510241
                    catch(Exception e)
                    {
                        Dev2Logger.Log.Debug("Null Variable",e);
                    }
                    var warewolfIterator = new WarewolfIterator(warewolfEvalResult);
                    iterators.Add(DataListUtil.ExtractFieldNameFromValue(name), warewolfIterator);
                    warewolfListIterators.AddVariableToIterateOn(warewolfIterator);
                }
                while (warewolfListIterators.HasMoreData())
                {
                    result.Append("<");
                    result.Append(groupedRecSet.Key);
                    result.Append(string.Format(" Index=\"{0}\">", i));
                    foreach (var namedIterator in iterators)
                    {
                        try
                        {
                            var value = warewolfListIterators.FetchNextValue(namedIterator.Value);
                            result.Append("<");
                            result.Append(namedIterator.Key);
                            result.Append(">");
                            result.Append(value);
                            result.Append("</");
                            result.Append(namedIterator.Key);
                            result.Append(">");
                        }
                        catch(Exception e)
                        {
                            Dev2Logger.Log.Debug(e.Message,e);
                        }
                    }
                    result.Append("</");
                    result.Append(groupedRecSet.Key);
                    result.Append(">");
                    i++;
                }

            }

            foreach (var output in scalarOutputs)
            {
                var evalResult = environment.Eval(DataListUtil.AddBracketsToValueIfNotExist(output), update,false);
                if (evalResult.IsWarewolfAtomResult)
                {
                    var scalarResult = evalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                    if (scalarResult != null && !scalarResult.Item.IsNothing)
                    {
                        result.Append("<");
                        result.Append(output);
                        result.Append(">");
                        result.Append(scalarResult.Item);
                        result.Append("</");
                        result.Append(output);
                        result.Append(">");
                    }
                }
            }

            result.Append("</DataList>");


            return result.ToString();
        }

        public static string GetJsonOutputFromEnvironment(IDSFDataObject dataObject,string dataList,int update)
        {
            var environment = dataObject.Environment;
            var dataListTO = new DataListTO(dataList);
            StringBuilder result = new StringBuilder("{");
            var keyCnt = 0;
            var scalarOutputs = dataListTO.Outputs.Where(s => !DataListUtil.IsValueRecordset(s));
            var recSetOutputs = dataListTO.Outputs.Where(DataListUtil.IsValueRecordset);
            var groupedRecSets = recSetOutputs.GroupBy(DataListUtil.ExtractRecordsetNameFromValue);
            var recSets = groupedRecSets as IGrouping<string, string>[] ?? groupedRecSets.ToArray();
            foreach (var groupedRecSet in recSets)
            {
                var i = 0;
                var warewolfListIterators = new WarewolfListIterator();
                Dictionary<string, IWarewolfIterator> iterators = new Dictionary<string, IWarewolfIterator>();
                foreach (var name in groupedRecSet)
                {
                    var warewolfIterator = new WarewolfIterator(environment.Eval(name, update,false));
                    iterators.Add(DataListUtil.ExtractFieldNameFromValue(name), warewolfIterator);
                    warewolfListIterators.AddVariableToIterateOn(warewolfIterator);

                }
                result.Append("\"");
                result.Append(groupedRecSet.Key);
                result.Append("\" : [");
                
                while (warewolfListIterators.HasMoreData())
                {
                    int colIdx = 0;
                    result.Append("{");
                    foreach (var namedIterator in iterators)
                    {
                        try
                        {
                            var value = warewolfListIterators.FetchNextValue(namedIterator.Value);
                            result.Append("\"");
                            result.Append(namedIterator.Key);
                            result.Append("\":\"");
                            result.Append(value);
                            result.Append("\"");
                            colIdx++;
                            if (colIdx < iterators.Count)
                            {
                                result.Append(",");
                            }
                        }
                        catch(Exception e)
                        {
                            Dev2Logger.Log.Debug(e.Message,e);
                            colIdx++;
                        }
                        
                    }
                    if (warewolfListIterators.HasMoreData())
                    {
                        result = new StringBuilder(result.ToString().TrimEnd(','));
                        result.Append("}");
                        result.Append(",");
                    }
                }
                result.Append("}");
                result.Append("]");
                i++;
                if (i <= recSets.Length)
                {
                    result.Append(",");
                }
            }

            var scalars = scalarOutputs as string[] ?? scalarOutputs.ToArray();
            foreach (var output in scalars)
            {
                var evalResult = environment.Eval(DataListUtil.AddBracketsToValueIfNotExist(output), update,false);
                if (evalResult.IsWarewolfAtomResult)
                {
                    var scalarResult = evalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                    if (scalarResult != null && !scalarResult.Item.IsNothing)
                    {
                        result.Append("\"");
                        result.Append(output);
                        result.Append("\":\"");
                        result.Append(scalarResult.Item);
                        result.Append("\"");

                    }
                }                
                keyCnt++;
                if (keyCnt < scalars.Length)
                {
                    result.Append(",");
                }
            }
            var jsonOutputFromEnvironment = result.ToString();
            jsonOutputFromEnvironment = jsonOutputFromEnvironment.TrimEnd(',');
            jsonOutputFromEnvironment += "}";
            return jsonOutputFromEnvironment;
        }

        public static void UpdateEnvironmentFromXmlPayload(IDSFDataObject dataObject, StringBuilder rawPayload, string dataList, int update)
        {

            string toLoad = DataListUtil.StripCrap(rawPayload.ToString()); // clean up the rubish ;)
            XmlDocument xDoc = new XmlDocument();
            toLoad = string.Format("<Tmp{0}>{1}</Tmp{0}>", Guid.NewGuid().ToString("N"), toLoad);
            xDoc.LoadXml(toLoad);
            if(dataList != null)
            {
                dataList = dataList.Replace("ADL>", "DataList>").Replace("root>","DataList>");
                if (xDoc.DocumentElement != null)
                {
                    XmlNodeList children = xDoc.DocumentElement.ChildNodes;
                    var dataListTO = new DataListTO(dataList,true);
                    TryConvert(dataObject, children, dataListTO.Inputs, update);
                }
            }
        }

        public static void UpdateEnvironmentFromInputPayload(IDSFDataObject dataObject, StringBuilder rawPayload, string dataList,int update)
        {
            string toLoad = DataListUtil.StripCrap(rawPayload.ToString()); // clean up the rubish ;)
            if(toLoad.IsJSON())
            {
                XNode node = JsonConvert.DeserializeXNode(toLoad, "Root");
                if(node == null)
                {
                    return;
                }
                toLoad = node.ToString();
            }
            XmlDocument xDoc = new XmlDocument();
            toLoad = string.Format("<Tmp{0}>{1}</Tmp{0}>", Guid.NewGuid().ToString("N"), toLoad);
            xDoc.LoadXml(toLoad);
            dataList = dataList.Replace("ADL>", "DataList>").Replace("root>", "DataList>");
            if (xDoc.DocumentElement != null)
            {
                XmlNodeList children = xDoc.DocumentElement.ChildNodes;
                var dataListTO = new DataListTO(dataList);
                TryConvert(dataObject, children, dataListTO.Inputs, update);
            }
        }
        
        public static void UpdateEnvironmentFromOutputPayload(IDSFDataObject dataObject, StringBuilder rawPayload, string dataList, int update)
        {
            var toLoad = DataListUtil.StripCrap(rawPayload.ToString()); // clean up the rubish ;)
            var xDoc = new XmlDocument();
            toLoad = string.Format("<Tmp{0}>{1}</Tmp{0}>", Guid.NewGuid().ToString("N"), toLoad);
            xDoc.LoadXml(toLoad);
            dataList = dataList.Replace("ADL>", "DataList>").Replace("root>", "DataList>");
            if (xDoc.DocumentElement != null)
            {
                XmlNodeList children = xDoc.DocumentElement.ChildNodes;
                var dataListTO = new DataListTO(dataList);
                TryConvert(dataObject, children, dataListTO.Outputs, update);
            }
        }

        static void TryConvert(IDSFDataObject dataObject, XmlNodeList children, List<string> inputDefs, int update, int level = 0)
        {
            try
            {
                // spin through each element in the XML
            foreach (XmlNode c in children)
            {
                    if(c.Name != GlobalConstants.NaughtyTextNode)
                {
                        if(level > 0)
                    {
                        var c1 = c;
                        var scalars = inputDefs.Where(definition => definition == c1.Name);
                        var recSets = inputDefs.Where(definition => DataListUtil.ExtractRecordsetNameFromValue(definition) == c1.Name);
                        var scalarDefs = scalars as string[] ?? scalars.ToArray();
                        var recSetDefs = recSets as string[] ?? recSets.ToArray();
                            if(recSetDefs.Length != 0)
                        {
                            var nl = c.ChildNodes;
                                foreach(XmlNode subc in nl)
                            {
                                    foreach(var definition in recSetDefs)
                                {
                                        if(DataListUtil.IsValueRecordset(definition))
                                    {
                                            if(DataListUtil.ExtractFieldNameFromValue(definition) == subc.Name)
                                        {
                                            var recSetAppend = DataListUtil.ReplaceRecordsetIndexWithBlank(definition);
                                            var a = subc.InnerXml;
                                            a = RemoveXMLPrefix(a);
                                                dataObject.Environment.AssignWithFrame(new AssignValue(recSetAppend, a),update);
                                     
                                        }
                                    }
                                }
                            }
                        }
                            if(scalarDefs.Length != 0)
                        {
                            // fetch recordset index
                            // process recordset
                            var a = c.InnerXml;
                            a = RemoveXMLPrefix(a);
                            dataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(c.Name), a,update);
                        }
                    }
                    else
                    {
                            if(level == 0)
                        {
                                TryConvert(dataObject, c.ChildNodes, inputDefs, update, ++level);
                        }
                    }
                }
            }
            }
            finally
            {
                dataObject.Environment.CommitAssign();
            }
        }
        // ReSharper disable once InconsistentNaming
        static string RemoveXMLPrefix(string a)
        {
            if (a.StartsWith(GlobalConstants.XMLPrefix))
            {
                a = a.Replace(GlobalConstants.XMLPrefix, "");
                a = Encoding.UTF8.GetString(Convert.FromBase64String(a));
            }
            return a;
        }
        public static string GetXmlInputFromEnvironment(IDSFDataObject dataObject, string dataList, int update)
        {
            var environment = dataObject.Environment;
            var dataListTO = new DataListTO(dataList);
            StringBuilder result = new StringBuilder("<DataList>");
            var scalarOutputs = dataListTO.Inputs.Where(s => !DataListUtil.IsValueRecordset(s));
            var recSetOutputs = dataListTO.Inputs.Where(DataListUtil.IsValueRecordset);
            var groupedRecSets = recSetOutputs.GroupBy(DataListUtil.ExtractRecordsetNameFromValue);
            foreach (var groupedRecSet in groupedRecSets)
            {
                var i = 1;
                var warewolfListIterators = new WarewolfListIterator();
                var iterators = new Dictionary<string, IWarewolfIterator>();
                foreach (var name in groupedRecSet)
                {
                    var warewolfIterator = new WarewolfIterator(environment.Eval(name, update));
                    iterators.Add(DataListUtil.ExtractFieldNameFromValue(name), warewolfIterator);
                    warewolfListIterators.AddVariableToIterateOn(warewolfIterator);

                }
                while (warewolfListIterators.HasMoreData())
                {
                    result.Append("<");
                    result.Append(groupedRecSet.Key);
                    result.Append(string.Format(" Index=\"{0}\">", i));
                    foreach (var namedIterator in iterators)
                    {
                        var value = warewolfListIterators.FetchNextValue(namedIterator.Value);
                        result.Append("<");
                        result.Append(namedIterator.Key);
                        result.Append(">");
                        result.Append(value);
                        result.Append("</");
                        result.Append(namedIterator.Key);
                        result.Append(">");
                    }
                    result.Append("</");
                    result.Append(groupedRecSet.Key);
                    result.Append(">");
                    i++;
                }

            }


            foreach (var output in scalarOutputs)
            {
                var evalResult = environment.Eval(DataListUtil.AddBracketsToValueIfNotExist(output), update);
                if (evalResult.IsWarewolfAtomResult)
                {
                    var scalarResult = evalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                    if (scalarResult != null && !scalarResult.Item.IsNothing)
                    {
                        result.Append("<");
                        result.Append(output);
                        result.Append(">");
                        result.Append(scalarResult.Item);
                        result.Append("</");
                        result.Append(output);
                        result.Append(">");
                    }
                }
            }

            result.Append("</DataList>");


            return result.ToString();
        }

        public static string GetSwaggerOutputForService(IResource resource, string dataList, string webServerUrl)
        {
            if(resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            if(string.IsNullOrEmpty(dataList))
            {
                throw new ArgumentNullException("dataList");
            }
            Uri url;
            Uri.TryCreate(webServerUrl, UriKind.RelativeOrAbsolute, out url);
            List<JObject> parameters;
            bool isScalarInputOnly;
            var jsonSwaggerInfoObject = BuildJsonSwaggerInfoObject(resource);
            var definitionObject = GetParametersDefinition(out parameters, dataList, out isScalarInputOnly);
            var parametersForSwagger = isScalarInputOnly ? (JToken)new JArray(parameters) : new JArray(new JObject { { "name", "DataList" } , {"in","query"},{"required",true},{"schema",new JObject{{"$ref","#/definitions/DataList"}}}});
            var jsonSwaggerPathObject = BuildJsonSwaggerPathObject(url.AbsolutePath, parametersForSwagger);
            var jsonSwaggerResponsesObject = BuildJsonSwaggerResponsesObject();
            var jsonSwaggerObject = BuildJsonSwaggerObject(jsonSwaggerInfoObject, jsonSwaggerPathObject, jsonSwaggerResponsesObject, definitionObject,url.Scheme);
            var resultString = GetSerializedSwaggerObject(jsonSwaggerObject);
            return resultString;
        }

        static JToken GetParametersDefinition(out List<JObject> parameters, string dataList, out bool isScalarInputOnly)
        {
            var dataListTO = new DataListTO(dataList);
            var scalarInputs = dataListTO.Inputs.Where(s => !DataListUtil.IsValueRecordset(s));
            var recSetInputs = dataListTO.Inputs.Where(DataListUtil.IsValueRecordset).ToList();
            var scalarOutputs = dataListTO.Outputs.Where(s => !DataListUtil.IsValueRecordset(s));
            var recSetOutputs = dataListTO.Outputs.Where(DataListUtil.IsValueRecordset);
            parameters = null;
            isScalarInputOnly = true;
            var dataListSchema = new Dictionary<string, Schema>
            {
                {
                    "Output", new Schema
                    {
                        Type = "object",
                        Properties = BuildDefinition(scalarOutputs, recSetOutputs)
                    }
                }
            };

            if(recSetInputs.Any())
            {
                dataListSchema.Add("DataList", new Schema
                {
                    Type = "object",
                    Properties = BuildDefinition(scalarInputs, recSetInputs)
                });
                isScalarInputOnly = false;
            }
            else
            {
                parameters = scalarInputs.Select(scalarInput => new JObject
                {
                    { "name", scalarInput }, { "in", "query" }, { "required", true }, { "type", "string" }
                }).ToList();
    }
            var serialized = JsonConvert.SerializeObject(dataListSchema);
            JToken des = JsonConvert.DeserializeObject(serialized) as JToken;
            var definitionObject = des;
            return definitionObject;
        }

        static string GetSerializedSwaggerObject(JObject jsonSwaggerObject)
        {
            var converter = new JsonSerializer();
            StringBuilder result = new StringBuilder();
            var jsonTextWriter = new JsonTextWriter(new StringWriter(result)) { Formatting = Newtonsoft.Json.Formatting.Indented };
            converter.Serialize(jsonTextWriter, jsonSwaggerObject);
            jsonTextWriter.Flush();
            var resultString = Regex.Replace(result.ToString(), @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
            return resultString;
        }

        static JObject BuildJsonSwaggerObject(JObject jsonSwaggerInfoObject, JObject jsonSwaggerPathObject, JObject jsonSwaggerResponsesObject, JToken definitionObject,string scheme)
        {
            var jsonSwaggerObject = new JObject
            {
                { "swagger", new JValue(2) },
                { "info", jsonSwaggerInfoObject },
                { "host", new JValue(EnvironmentVariables.PublicWebServerUri) },
                { "basePath", new JValue("/") },
                { "schemes", new JArray(scheme) },
                { "produces", new JArray("application/json","application/xml") },
                { "paths", jsonSwaggerPathObject },
                { "responses", jsonSwaggerResponsesObject },
                { "definitions", definitionObject }
            };
            return jsonSwaggerObject;
        }

        static JObject BuildJsonSwaggerResponsesObject()
        {
            var jsonSwaggerResponsesObject = new JObject
            {
                {
                    "200", new JObject
                    {
                        {
                            "schema", new JObject
                            {
                                { "$ref", "#/definition/Output" }
                            }
                        }
                    }
                }
            };
            return jsonSwaggerResponsesObject;
        }

        static JObject BuildJsonSwaggerPathObject(string path, JToken parametersForSwagger)
        {
            var jsonSwaggerPathObject = new JObject
            {
                { "serviceName", new JValue(path) },
                {
                    "get", new JObject
                    {
                        { "summary", new JValue("") },
                        { "description", new JValue("") },
                        { "parameters", parametersForSwagger }
                    }
                }
            };
            return jsonSwaggerPathObject;
        }

        static JObject BuildJsonSwaggerInfoObject(IResource resource)
        {
            var versionValue = resource.VersionInfo!=null ? new JValue(resource.VersionInfo.VersionNumber) : new JValue("1.0.0");
            var jsonSwaggerInfoObject = new JObject
            {
                { "title", new JValue(resource.ResourceName) },
                { "description", new JValue("") },
                { "version", versionValue }
            };
            return jsonSwaggerInfoObject;
        }

        static Dictionary<string,Schema> BuildDefinition(IEnumerable<string> scalars, IEnumerable<string> recSets)
        {
            var groupedRecSets = recSets.GroupBy(DataListUtil.ExtractRecordsetNameFromValue);
            var recSetItems = scalars.ToDictionary(scalarInput => scalarInput, scalarInput => new Schema { Type = "string" });
            foreach(var groupedRecSet in groupedRecSets)
            {
                var recSetName = groupedRecSet.Key;
                var propObject = BuildPropertyDefinition(groupedRecSet);

                var recObject = new Schema
                {
                    Type = "object",
                    Properties = propObject
                };
                recSetItems.Add(recSetName,recObject);                
            }
            return recSetItems;
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        static Dictionary<string,Schema> BuildPropertyDefinition(IGrouping<string, string> groupedRecSet)
        {
            return groupedRecSet.ToDictionary(DataListUtil.ExtractFieldNameOnlyFromValue, name => new Schema { Type = "string" });
        }
    }

    public class Schema
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, Schema> Properties { get; set; }

    }


}