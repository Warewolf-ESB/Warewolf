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
using Dev2.Data.Binary_Objects;
using Dev2.Data.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Storage;
using WarewolfParserInterop;

namespace Dev2
{
    public static class ExecutionEnvironmentUtils
    {
        public static string GetXmlOutputFromEnvironment(IDSFDataObject dataObject,string dataList)
        {
            var xml = JsonConvert.DeserializeXNode(GetJsonForOutput(dataObject, dataList), "DataList");
            return xml.ToString();
        }

        private static string GetJsonForOutput(IDSFDataObject dataObject, string dataList)
        {
            var environment = dataObject.Environment;
            var fixedDataList = dataList.Replace(GlobalConstants.SerializableResourceQuote, "\"").Replace(GlobalConstants.SerializableResourceSingleQuote, "\'");
            var serializeXNode = JsonConvert.SerializeXNode(XDocument.Parse(fixedDataList), Newtonsoft.Json.Formatting.Indented, true);
            var deserializeObject = JsonConvert.DeserializeObject(serializeXNode) as JObject;
            if(deserializeObject != null)
            {
                var outputObj = new JObject();
                var props = deserializeObject.Properties().ToList();
                foreach(var prop in props)
                {
                    if(prop.Value != null && prop.Value.Type == JTokenType.Object)
                    {
                        var val = prop.Value as JObject;
                        if(val != null)
                        {
                            var jProperty = val.Properties().FirstOrDefault(property => property.Name == "@ColumnIODirection");
                            if(jProperty != null)
                            {
                                var propValue = jProperty.Value;
                                enDev2ColumnArgumentDirection ioDirection;
                                if(Enum.TryParse(propValue.ToString(), true, out ioDirection))
                                {
                                    if(ioDirection == enDev2ColumnArgumentDirection.Both || ioDirection == enDev2ColumnArgumentDirection.Output)
                                    {
                                        var objName = prop.Name;
                                        var isJson = val.Properties().FirstOrDefault(property => property.Name == "@IsJson");
                                        if(isJson != null && isJson.Value.ToString() == "True")
                                        {
                                            AddObjectsToOutput(environment, objName, outputObj);
                                        }
                                        else
                                        {
                                            if(prop.Value.Count() > 3)
                                            {
                                                AddRecordsetsToOutput(environment, objName, val, outputObj);
                                            }
                                            else
                                            {
                                                AddScalarsToOutput(prop, environment, objName, outputObj);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                var dataListString = outputObj.ToString(Newtonsoft.Json.Formatting.Indented);
                return dataListString;
            }
            return null;
        }

        private static void AddObjectsToOutput(IExecutionEnvironment environment, string objName, JObject outputObj)
        {
            var evalResult = environment.EvalJContainer("[[@" + objName + "]]");
            if(evalResult != null)
            {
                outputObj.Add(objName, evalResult);
            }
        }

        private static void AddScalarsToOutput(JProperty prop, IExecutionEnvironment environment, string objName, JObject outputObj)
        {
            var v = prop.Value as JObject;
            if(v != null)
            {
                var ioDire = v.Properties().FirstOrDefault(property => property.Name == "@ColumnIODirection");
                if(ioDire != null)
                {
                    enDev2ColumnArgumentDirection x = (enDev2ColumnArgumentDirection)Enum.Parse(typeof(enDev2ColumnArgumentDirection), ioDire.Value.ToString());
                    if(x == enDev2ColumnArgumentDirection.Both || x == enDev2ColumnArgumentDirection.Output)
                    {
                        var warewolfEvalResult = environment.Eval("[[" + objName + "]]", 0) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                        if(warewolfEvalResult != null)
                        {
                            var eval = PublicFunctions.AtomtoString(warewolfEvalResult.Item);
                            outputObj.Add(objName, eval);
                        }
                    }
                }
            }
        }

        private static void AddRecordsetsToOutput(IExecutionEnvironment environment, string objName, JObject val, JObject outputObj)
        {
            var evalResult = environment.Eval("[[" + objName + "(*)]]", 0);
            var newArray = new JArray();
            if(evalResult != null)
            {
                var res = evalResult as CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult;
                if(res != null)
                {
                    var data = res.Item.Data;
                    foreach(var dataItem in data)
                    {
                        var jObjForArray = new JObject();
                        var recCol = val.Properties().FirstOrDefault(property => property.Name == dataItem.Key);
                        if(recCol != null)
                        {
                            var io = recCol.Children().FirstOrDefault() as JObject;
                            if(io != null)
                            {
                                var p = io.Properties().FirstOrDefault(token => token.Name == "@ColumnIODirection");
                                if(p != null)
                                {
                                    enDev2ColumnArgumentDirection direction = (enDev2ColumnArgumentDirection)Enum.Parse(typeof(enDev2ColumnArgumentDirection), p.Value.ToString(), true);
                                    if(direction == enDev2ColumnArgumentDirection.Both || direction == enDev2ColumnArgumentDirection.Output)
                                    {
                                        int i = 0;
                                        foreach(var warewolfAtom in dataItem.Value)
                                        {
                                            jObjForArray.Add(dataItem.Key, warewolfAtom.ToString());
                                            if(newArray.Count < i + 1 || newArray.Count == 0)
                                            {
                                                newArray.Add(jObjForArray);
                                            }
                                            else
                                            {
                                                var jToken = newArray[i] as JObject;
                                                if(jToken != null)
                                                    jToken.Add(new JProperty(dataItem.Key, warewolfAtom.ToString()));
                                            }
                                            jObjForArray = new JObject();
                                            i++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                outputObj.Add(objName, newArray);
            }
        }

        public static string GetJsonOutputFromEnvironment(IDSFDataObject dataObject,string dataList)
        {
            return GetJsonForOutput(dataObject, dataList);            
        }

        public static void UpdateEnvironmentFromXmlPayload(IDSFDataObject dataObject, StringBuilder rawPayload, string dataList, int update)
        {

            string toLoad = DataListUtil.StripCrap(rawPayload.ToString()); // clean up the rubish ;)
            XmlDocument xDoc = new XmlDocument();
            toLoad = string.Format("<Tmp{0}>{1}</Tmp{0}>", Guid.NewGuid().ToString("N"), toLoad);
            xDoc.LoadXml(toLoad);
            if (dataList != null)
            {
                dataList = dataList.Replace("ADL>", "DataList>").Replace("root>", "DataList>");
                if (xDoc.DocumentElement != null)
                {
                    XmlNodeList children = xDoc.DocumentElement.ChildNodes;
                    var dataListTO = new DataListTO(dataList, true);
                    TryConvert(dataObject, children, dataListTO.Inputs, update);
                }
            }
        }
        public static void UpdateEnvironmentFromInputPayload(IDSFDataObject dataObject, StringBuilder rawPayload, string dataList)
        {

            JObject inputObject;
            string toLoad = DataListUtil.StripCrap(rawPayload.ToString());
            if (!toLoad.IsJSON())
            {
                var sXNode = JsonConvert.SerializeXNode(XDocument.Parse(toLoad), Newtonsoft.Json.Formatting.Indented, true);
                inputObject = JsonConvert.DeserializeObject(sXNode) as JObject;
            }
            else
            {
                inputObject = JsonConvert.DeserializeObject(toLoad) as JObject;
            }
            if (inputObject != null)
            {
                dataList = dataList.Replace("ADL>", "DataList>").Replace("root>", "DataList>");
                var dataListTO = new DataListTO(dataList);
                var inputs = dataListTO.Inputs;
                var recSets = inputs.Where(DataListUtil.IsValueRecordset).ToList();
                var processedRecsets = new List<string>();
                foreach (var input in inputs)
                {
                    var inputName = input;
                    var isValueRecordset = DataListUtil.IsValueRecordset(input);
                    if (isValueRecordset)
                    {
                        inputName = DataListUtil.ExtractRecordsetNameFromValue(input);
                        if (processedRecsets.Contains(inputName))
                        {
                            continue;
                        }
                    }
                    var propsMatching = inputObject.Properties().Where(property => property.Name == inputName).ToList();
                    foreach(var prop in propsMatching)
                    {
                        var value = prop.Value;
                        var tokenType = value.Type;
                        if(tokenType == JTokenType.Object)
                        {
                            var jContainer = value as JContainer;
                            dataObject.Environment.AddToJsonObjects(DataListUtil.AddBracketsToValueIfNotExist("@"+input), jContainer);
                        }
                        else if (tokenType == JTokenType.Array)
                        {
                            var arrayValue = value as JArray;
                            if (!isValueRecordset)
                            {
                                dataObject.Environment.AddToJsonObjects(DataListUtil.AddBracketsToValueIfNotExist("@" + input+"()"), arrayValue);
                            }
                            else
                            {
                                if (arrayValue != null)
                                {
                                    for (int i = 0; i < arrayValue.Count; i++)
                                    {
                                        var val = arrayValue[i];
                                        var valObj = val as JObject;
                                        if (valObj != null)
                                        {
                                            var recs = recSets.Where(s => DataListUtil.ExtractRecordsetNameFromValue(s) == inputName);
                                            foreach (var rec in recs)
                                            {
                                                var field = DataListUtil.ExtractFieldNameOnlyFromValue(rec);
                                                var fieldProp = valObj.Properties().FirstOrDefault(property => property.Name == field);
                                                if (fieldProp != null)
                                                {
                                                    dataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(rec), fieldProp.Value.ToString(), i + 1);
                                                }
                                            }
                                        }
                                    }
                                    processedRecsets.Add(inputName);
                                }
                            }
                        }
                        else
                        {
                            dataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(input), value.ToString(),0);
                        }

                    }
                }

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

        private static void TryConvert(IDSFDataObject dataObject, XmlNodeList children, List<string> inputDefs, int update, int level = 0)
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
                    var scalarResult = evalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
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