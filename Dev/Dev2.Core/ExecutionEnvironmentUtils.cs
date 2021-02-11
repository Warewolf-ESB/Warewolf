/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.DB;
using Dev2.Data;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;
using Warewolf.Data;

namespace Dev2
{
    public static class ExecutionEnvironmentUtils
    {
        public static string GetXmlOutputFromEnvironment(IDSFDataObject dataObject, string dataList, int update)
        {
            var jsonOutput = GetJsonForEnvironmentWithColumnIoDirection(dataObject, dataList, enDev2ColumnArgumentDirection.Output, update);
            var xml = JsonConvert.DeserializeXNode(jsonOutput, "DataList", true);
            return xml.ToString();
        }

        static string GetJsonForEnvironmentWithColumnIoDirection(IDSFDataObject dataObject, string dataList, enDev2ColumnArgumentDirection requestIODirection, int update)
        {
            var environment = dataObject.Environment;
            var fixedDataList = dataList.Replace(GlobalConstants.SerializableResourceQuote, "\"").Replace(GlobalConstants.SerializableResourceSingleQuote, "\'");
            var serializeXNode = JsonConvert.SerializeXNode(XDocument.Parse(fixedDataList), Newtonsoft.Json.Formatting.Indented, true);
            if (JsonConvert.DeserializeObject(serializeXNode) is JObject deserializeObject)
            {
                var outputObj = new JObject();
                var props = deserializeObject.Properties().ToList();
                foreach (var prop in props)
                {
                    TryAddPropToOutput(requestIODirection, update, environment, outputObj, prop);
                }

                var dataListString = outputObj.ToString(Newtonsoft.Json.Formatting.Indented);
                return dataListString;
            }

            return "{}";
        }

        private static void TryAddPropToOutput(enDev2ColumnArgumentDirection requestIODirection, int update, IExecutionEnvironment environment, JObject outputObj, JProperty prop)
        {
            if (prop.Value != null && prop.Value.Type == JTokenType.Object)
            {
                var val = prop.Value as JObject;
                var jProperty = val?.Properties().FirstOrDefault(property => property.Name == "@ColumnIODirection");
                if (jProperty != null)
                {
                    var propValue = jProperty.Value;
                    if (Enum.TryParse(propValue.ToString(), true, out enDev2ColumnArgumentDirection ioDirection) && (ioDirection == enDev2ColumnArgumentDirection.Both || ioDirection == requestIODirection))
                    {
                        AddPropToOutput(requestIODirection, update, environment, outputObj, prop, val);
                    }
                }
            }
        }

        private static void AddPropToOutput(enDev2ColumnArgumentDirection requestIODirection, int update, IExecutionEnvironment environment, JObject outputObj, JProperty prop, JObject val)
        {
            var objName = prop.Name;
            var isJson = val.Properties().FirstOrDefault(property => property.Name == "@IsJson");
            if (isJson != null && isJson.Value.ToString() == "True")
            {
                AddObjectsToOutput(environment, objName, outputObj);
            }
            else
            {
                if (prop.Value.Count() > 3)
                {
                    AddRecordsetsToOutput(environment, objName, val, outputObj, requestIODirection, update);
                }
                else
                {
                    AddScalarsToOutput(prop, environment, objName, outputObj, requestIODirection);
                }
            }
        }

        static void AddObjectsToOutput(IExecutionEnvironment environment, string objName, JObject outputObj)
        {
            var evalResult = environment.EvalJContainer("[[@" + objName + "]]");
            if (evalResult != null)
            {
                outputObj.Add(objName, evalResult);
            }
        }

        static void AddScalarsToOutput(JProperty prop, IExecutionEnvironment environment, string objName, JObject outputObj, enDev2ColumnArgumentDirection requestIODirection)
        {
            var v = prop.Value as JObject;
            var ioDire = v?.Properties().FirstOrDefault(property => property.Name == "@ColumnIODirection");
            if (ioDire != null)
            {
                var x = (enDev2ColumnArgumentDirection) Enum.Parse(typeof(enDev2ColumnArgumentDirection), ioDire.Value.ToString());
                if ((x == enDev2ColumnArgumentDirection.Both || x == requestIODirection) && (environment.Eval("[[" + objName + "]]", 0) is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult warewolfEvalResult))
                {
                    ParseDataItemToOutputs(outputObj, objName, warewolfEvalResult.Item);
                }
            }
        }

        static void AddRecordsetsToOutput(IExecutionEnvironment environment, string objName, JObject val, JObject outputObj, enDev2ColumnArgumentDirection requestedIODirection, int update)
        {
            var evalResult = environment.Eval("[[" + objName + "(*)]]", update);
            var newArray = new JArray();
            if (evalResult != null)
            {
                if (evalResult is CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult res)
                {
                    var data = res.Item.Data;
                    foreach (var dataItem in data)
                    {
                        AddDataItemToOutputs(val, requestedIODirection, newArray, dataItem);
                    }
                }

                outputObj.Add(objName, newArray);
            }
        }

        static void AddDataItemToOutputs(JObject val, enDev2ColumnArgumentDirection requestedIODirection, JArray newArray, KeyValuePair<string, WarewolfAtomList<DataStorage.WarewolfAtom>> dataItem)
        {
            var jObjForArray = new JObject();
            var recCol = val.Properties().FirstOrDefault(property => property.Name == dataItem.Key);
            var io = recCol?.Children().FirstOrDefault() as JObject;
            var p = io?.Properties().FirstOrDefault(token => token.Name == "@ColumnIODirection");
            if (p != null)
            {
                var direction = (enDev2ColumnArgumentDirection) Enum.Parse(typeof(enDev2ColumnArgumentDirection), p.Value.ToString(), true);
                if (direction == enDev2ColumnArgumentDirection.Both || direction == requestedIODirection)
                {
                    var i = 0;
                    foreach (var warewolfAtom in dataItem.Value)
                    {
                        ParseDataItemToOutputs(jObjForArray, dataItem.Key, warewolfAtom);

                        dataItem = CreateDataItem(newArray, dataItem, jObjForArray, i, warewolfAtom);
                        jObjForArray = new JObject();
                        i++;
                    }
                }
            }
        }

        private static KeyValuePair<string, WarewolfAtomList<DataStorage.WarewolfAtom>> CreateDataItem(JArray newArray, KeyValuePair<string, WarewolfAtomList<DataStorage.WarewolfAtom>> dataItem, JObject jObjForArray, int i, DataStorage.WarewolfAtom warewolfAtom)
        {
            if (newArray.Count < i + 1 || newArray.Count == 0)
            {
                newArray.Add(jObjForArray);
            }
            else
            {
                var jToken = newArray[i] as JObject;
                ParseDataItemToOutputs(jToken, dataItem.Key, warewolfAtom);
            }

            return dataItem;
        }

        static void ParseDataItemToOutputs(JObject jObject, string key, DataStorage.WarewolfAtom warewolfAtom)
        {
            if (warewolfAtom is DataStorage.WarewolfAtom.DataString stringResult)
            {
                jObject.Add(key, stringResult.Item);
            }
            else if (warewolfAtom is DataStorage.WarewolfAtom.Int intResult)
            {
                jObject.Add(key, intResult.Item);
            }
            else if (warewolfAtom is DataStorage.WarewolfAtom.Float floatResult)
            {
                jObject.Add(key, floatResult.Item);
            }
            else
            {
                jObject.Add(key, warewolfAtom.ToString());
            }
        }

        public static string GetJsonOutputFromEnvironment(IDSFDataObject dataObject, string dataList, int update) => GetJsonForEnvironmentWithColumnIoDirection(dataObject, dataList, enDev2ColumnArgumentDirection.Output, update);

        public static void UpdateEnvironmentFromXmlPayload(IDSFDataObject dataObject, StringBuilder rawPayload, string dataList, int update)
        {
            var toLoad = rawPayload.ToString().ToCleanXml();
            var xDoc = new XmlDocument();
            toLoad = string.Format("<Tmp{0:N}>{1}</Tmp{0:N}>", Guid.NewGuid(), toLoad);
            xDoc.LoadXml(toLoad);
            if (dataList != null)
            {
                dataList = dataList.Replace("ADL>", "DataList>").Replace("root>", "DataList>");
                if (xDoc.DocumentElement != null)
                {
                    var children = xDoc.DocumentElement.ChildNodes;
                    var dataListTO = new DataListTO(dataList, true);
                    TryLoadXmlIntoEnvironment(dataObject, children, dataListTO.Inputs, update);
                }
            }
        }

        public static void UpdateEnvironmentFromInputPayload(IDSFDataObject dataObject, StringBuilder rawPayload, string dataList)
        {
            dataList = dataList.Replace("ADL>", "DataList>").Replace("root>", "DataList>");
            var dataListTO = new DataListTO(dataList);
            var inputs = dataListTO.Inputs;
            TryUpdateEnviromentWithMappings(dataObject, rawPayload, inputs);
        }

        static void TryUpdateEnviromentWithMappings(IDSFDataObject dataObject, StringBuilder rawPayload, List<string> mappings)
        {
            JObject inputObject;
            var toLoad = rawPayload.ToString();
            if (string.IsNullOrEmpty(toLoad))
            {
                return;
            }

            if (!toLoad.IsJSON())
            {
                toLoad = toLoad.ToCleanXml();
                var sXNode = JsonConvert.SerializeXNode(XDocument.Parse(toLoad), Newtonsoft.Json.Formatting.Indented, true);
                inputObject = JsonConvert.DeserializeObject(sXNode) as JObject;
            }
            else
            {
                inputObject = JsonConvert.DeserializeObject(toLoad) as JObject;
            }

            if (inputObject != null)
            {
                UpdateEnviromentWithMappings(dataObject, mappings, inputObject);
            }
        }

        static void UpdateEnviromentWithMappings(IDSFDataObject dataObject, List<string> mappings, JObject inputObject)
        {
            var recSets = mappings.Where(DataListUtil.IsValueRecordset).ToList();
            var processedRecsets = new List<string>();
            foreach (var input in mappings)
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
                foreach (var prop in propsMatching)
                {
                    var value = prop.Value;
                    var tokenType = value.Type;
                    if (tokenType == JTokenType.Object)
                    {
                        PerformRecordsetUpdate(dataObject, value, processedRecsets, input, recSets, inputName, isValueRecordset);
                    }
                    else if (tokenType == JTokenType.Array)
                    {
                        PerformRecordsetUpdate(dataObject, value, isValueRecordset, input, recSets, inputName, processedRecsets);
                    }
                    else
                    {
                        dataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(input), value.ToString(), 0);
                    }
                }
            }
        }

        private static void PerformRecordsetUpdate(IDSFDataObject dataObject, JToken value, List<string> processedRecsets, string input, List<string> recSets, string inputName, bool isValueRecordset)
        {
            if (isValueRecordset)
            {
                var arr = new JArray(value);
                PerformRecordsetUpdate(dataObject, arr, true, input, recSets, inputName, processedRecsets);
            }
            else
            {
                var jContainer = value as JContainer;
                dataObject.Environment.AddToJsonObjects(DataListUtil.AddBracketsToValueIfNotExist("@" + input), jContainer);
            }
        }

        static void PerformRecordsetUpdate(IDSFDataObject dataObject, JToken value, bool isValueRecordset, string input, List<string> recSets, string inputName, List<string> processedRecsets)
        {
            var arrayValue = value as JArray;
            if (!isValueRecordset)
            {
                dataObject.Environment.AddToJsonObjects(DataListUtil.AddBracketsToValueIfNotExist("@" + input + "()"), arrayValue);
            }
            else
            {
                if (arrayValue != null)
                {
                    for (int i = 0; i < arrayValue.Count; i++)
                    {
                        UpdateEnvironmentFromJObject(dataObject, recSets, inputName, i, arrayValue[i]);
                    }

                    processedRecsets.Add(inputName);
                }
            }
        }

        static void UpdateEnvironmentFromJObject(IDSFDataObject dataObject, List<string> recSets, string inputName, int i, JToken val)
        {
            if (val is JObject valObj)
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

        public static void UpdateEnvironmentFromOutputPayload(IDSFDataObject dataObject, StringBuilder rawPayload, string dataList)
        {
            dataList = dataList.Replace("ADL>", "DataList>").Replace("root>", "DataList>");
            var dataListTO = new DataListTO(dataList);
            var outputs = dataListTO.Outputs;
            TryUpdateEnviromentWithMappings(dataObject, rawPayload, outputs);
        }

        static void TryLoadXmlIntoEnvironment(IDSFDataObject dataObject, XmlNodeList children, List<string> inputDefs, int update, int level = 0)
        {
            try
            {
                LoadXmlIntoEnvironment(dataObject, children, inputDefs, update, level);
            }
            finally
            {
                dataObject.Environment.CommitAssign();
            }
        }

        static void LoadXmlIntoEnvironment(IDSFDataObject dataObject, XmlNodeList children, List<string> inputDefs, int update, int level)
        {
            foreach (XmlNode c in children)
            {
                if (c.Name != GlobalConstants.NaughtyTextNode)
                {
                    if (level > 0)
                    {
                        var c1 = c;
                        var scalars = inputDefs.Where(definition => definition == c1.Name);
                        var recSets = inputDefs.Where(definition => DataListUtil.ExtractRecordsetNameFromValue(definition) == c1.Name);
                        UpdateForRecordset(dataObject, update, recSets, c);
                        UpdateForScalars(dataObject, update, scalars, c);
                    }
                    else
                    {
                        ContinueLoadingXmlIntoEnvironment(dataObject, inputDefs, update, level, c);
                    }
                }
            }
        }

        static void ContinueLoadingXmlIntoEnvironment(IDSFDataObject dataObject, List<string> inputDefs, int update, int level, XmlNode c)
        {
            if (level == 0)
            {
                TryLoadXmlIntoEnvironment(dataObject, c.ChildNodes, inputDefs, update, ++level);
            }
        }

        static void UpdateForScalars(IDSFDataObject dataObject, int update, IEnumerable<string> scalars, XmlNode c)
        {
            var scalarDefs = scalars as string[] ?? scalars.ToArray();
            if (scalarDefs.Length != 0)
            {
                // fetch recordset index
                // process recordset
                var a = c.InnerXml;
                a = RemoveXMLPrefix(a);
                dataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(c.Name), a, update);
            }
        }

        static void UpdateForRecordset(IDSFDataObject dataObject, int update, IEnumerable<string> recSets, XmlNode c)
        {
            var recSetDefs = recSets as string[] ?? recSets.ToArray();
            if (recSetDefs.Length != 0)
            {
                var nl = c.ChildNodes;
                foreach (XmlNode subc in nl)
                {
                    foreach (var definition in recSetDefs)
                    {
                        UpdateForChildNodes(dataObject, update, subc, definition);
                    }
                }
            }
        }

        private static void UpdateForChildNodes(IDSFDataObject dataObject, int update, XmlNode subc, string definition)
        {
            if (DataListUtil.IsValueRecordset(definition) && DataListUtil.ExtractFieldNameFromValue(definition) == subc.Name)
            {
                var recSetAppend = DataListUtil.ReplaceRecordsetIndexWithBlank(definition);
                var a = subc.InnerXml;
                a = RemoveXMLPrefix(a);
                dataObject.Environment.AssignWithFrame(new AssignValue(recSetAppend, a), update);
            }
        }

        static string RemoveXMLPrefix(string a)
        {
            if (a.StartsWith(GlobalConstants.XMLPrefix))
            {
                a = a.Replace(GlobalConstants.XMLPrefix, "");
                a = Encoding.UTF8.GetString(System.Convert.FromBase64String(a));
            }

            return a;
        }

        public static string GetXmlInputFromEnvironment(IDSFDataObject dataObject, string dataList, int update)
        {
            var xml = JsonConvert.DeserializeXNode(GetJsonForEnvironmentWithColumnIoDirection(dataObject, dataList, enDev2ColumnArgumentDirection.Input, update), "DataList", true);
            return xml.ToString();
        }

        public static string GetOpenAPIOutputForService(IWarewolfResource resource, string dataList, string webServerUrl)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (string.IsNullOrEmpty(dataList))
            {
                throw new ArgumentNullException(nameof(dataList));
            }

            Uri.TryCreate(webServerUrl, UriKind.RelativeOrAbsolute, out Uri url);
            var jsonOpenAPIInfoObject = BuildJsonOpenAPIInfoObject(resource);
            var jsonOpenAPIServerObject = BuildJsonOpenAPIServerObject(url);
            var jsonOpenAPIPathObject = BuildJsonOpenAPIPathObject(url, dataList);
            var jsonOpenAPIObject = BuildJsonOpenAPIObject(jsonOpenAPIInfoObject, jsonOpenAPIServerObject, jsonOpenAPIPathObject);
            var resultString = GetSerializedOpenAPIObject(jsonOpenAPIObject);
            return resultString;
        }

        public static string GetOpenAPIOutputForServiceList(IList<IWarewolfResource> resourceList,
            string webServerUrl)
        {
            if (resourceList == null)
            {
                throw new ArgumentNullException(nameof(resourceList));
            }
            
            if (string.IsNullOrEmpty(webServerUrl))
            {
                throw new ArgumentNullException(nameof(webServerUrl));
            }

            var paths = new List<JObject>();
            foreach (var resource in resourceList)
            {
                var filePath1 = webServerUrl;
                filePath1 = filePath1.Substring(0, filePath1.IndexOf("secure", StringComparison.Ordinal) + 6);

                var filePath2 = resource.FilePath;
                filePath2 = filePath2.Substring(filePath2.IndexOf("Resources", StringComparison.Ordinal) + 10)
                    .Replace(".bite", ".api");

                Uri.TryCreate($"{filePath1}/{filePath2}", UriKind.RelativeOrAbsolute, out Uri url);
                paths.Add(BuildJsonOpenAPIPathObject(url, resource.DataList.ToString()));
            }
            
            var info =  new JObject
            {
                {"title", new JValue(webServerUrl)},
                {"description", new JValue(webServerUrl)},
                {"version", "1"}
            };
            
            var jsonOpenAPIObject = new JObject
            {
                {"openapi", new JValue(EnvironmentVariables.OpenAPiVersion)},
                {"info", info},
                {"servers", new JArray(BuildJsonOpenAPIServerObject(new Uri(webServerUrl)))},
                {"paths", new JArray(paths)}
            };
            var serializedObject = GetSerializedOpenAPIObject(jsonOpenAPIObject);
            serializedObject = RemovePathsArray(serializedObject);

            return serializedObject;
        }

        static string RemovePathsArray(string apiObject)
        {
            apiObject = apiObject.Replace("\"paths\": [", "\"paths\": ");
            apiObject = apiObject.Replace("},\r\n    {\r\n      \"/secure", ",\r\n      \"/secure");
            apiObject = apiObject.Remove(apiObject.LastIndexOf("]"), 1);
            return apiObject;
        }

        static string GetSerializedOpenAPIObject(JObject jsonOpenAPIObject)
        {
            var converter = new JsonSerializer();
            var result = new StringBuilder();
            var jsonTextWriter = new JsonTextWriter(new StringWriter(result)) {Formatting = Newtonsoft.Json.Formatting.Indented};
            converter.Serialize(jsonTextWriter, jsonOpenAPIObject);
            jsonTextWriter.Flush();
            var resultString = Regex.Replace(result.ToString(), @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
            return resultString;
        }

        static JObject BuildJsonOpenAPIObject(JObject jsonOpenAPIInfoObject, JObject jsonOpenAPIServerObject, JObject jsonOpenAPIPathObject)
        {
            var jsonOpenAPIObject = new JObject
            {
                {"openapi", new JValue(EnvironmentVariables.OpenAPiVersion)},
                {"info", jsonOpenAPIInfoObject},
                {"servers", new JArray(jsonOpenAPIServerObject)},
                {"paths", jsonOpenAPIPathObject}
            };
            return jsonOpenAPIObject;
        }

        static JObject BuildJsonOpenAPIServerObject(Uri url)
        {
            var jsonOpenAPIServerObject = new JObject
            {
                {"url", new JValue(url.Scheme + "://" + url.Host)}
            };
            return jsonOpenAPIServerObject;
        }

        static JObject BuildJsonOpenAPIInfoObject(IWarewolfResource resource)
        {
            var versionValue = resource.VersionInfo != null ? new JValue(resource.VersionInfo.VersionNumber) : new JValue("1.0.0");

            var jsonOpenAPIInfoObject = new JObject
            {
                {"title", new JValue(resource.ResourceName)},
                {"description", new JValue(resource.ResourceName)},
                {"version", versionValue}
            };
            return jsonOpenAPIInfoObject;
        }

        static JObject BuildJsonOpenAPIPathObject(Uri path, string dataList)
        {
            var parametersObject = BuildParametersObject(dataList);
            var responseObject = BuildJsonOpenAPIResponsesObject(dataList);
            var pathGetObject = new JObject
            {
                {
                    "get", new JObject
                    {
                        {"tags", new JArray("")},
                        {"description", new JValue("")},
                        {"parameters", parametersObject},
                        {"responses", responseObject}
                    }
                }
            };

            var pathObject = new JObject
            {
                {path.AbsolutePath.Replace(".api", ""), pathGetObject}
            };
            return pathObject;
        }

        static JToken BuildParametersObject(string dataList)
        {
            JArray arrayObj = new JArray();
            var dataListTo = new DataListTO(dataList);

            var scalarInputs = dataListTo.Inputs.Where(s => !DataListUtil.IsValueRecordset(s));
            foreach (var input in scalarInputs)
            {
                var inputObject = new JObject
                {
                    {"name", input},
                    {"in", "query"},
                    {"required", true},
                    {
                        "schema", new JObject
                        {
                            {"type", "string"}
                        }
                    }
                };
                arrayObj.Add(inputObject);
            }

            var recSetInputs = dataListTo.Inputs.Where(DataListUtil.IsValueRecordset).ToList();
            var groupedRecSets = recSetInputs.GroupBy(DataListUtil.ExtractRecordsetNameFromValue);
            foreach (var groupedRecSet in groupedRecSets)
            {
                var recSetName = groupedRecSet.Key;
                var propObject = BuildPropertyDefinition(groupedRecSet);
                var recSchemaObject = new Schema
                {
                    Type = "object",
                    Properties = propObject
                };
                var serializedSchema = JsonConvert.SerializeObject(recSchemaObject);
                var deserializedSchema = JsonConvert.DeserializeObject(serializedSchema) as JToken;
                var recObject = new JObject
                {
                    {"name", recSetName},
                    {"in", "query"},
                    {"required", true},
                    {"schema", deserializedSchema}
                };
                arrayObj.Add(recObject);
            }

            var serialized = JsonConvert.SerializeObject(arrayObj);
            var des = JsonConvert.DeserializeObject(serialized) as JToken;
            var definitionObject = des;
            return definitionObject;
        }

        static JObject BuildJsonOpenAPIResponsesObject(string dataList)
        {
            var jsonOpenAPIResponsesObject = new JObject
            {
                {
                    "200", new JObject
                    {
                        {"description", new JValue("Success")},
                        {
                            "content", new JObject
                            {
                                {
                                    "application/json", BuildResponseSchema(dataList)
                                }
                            }
                        }
                    }
                }
            };
            return jsonOpenAPIResponsesObject;
        }

        private static JToken BuildResponseSchema(string dataList)
        {
            var dataListTo = new DataListTO(dataList);
            var scalarOutputs = dataListTo.Outputs.Where(s => !DataListUtil.IsValueRecordset(s));
            var recSetOutputs = dataListTo.Outputs.Where(DataListUtil.IsValueRecordset);

            var propertiesObject = scalarOutputs.ToDictionary(scalarInput => scalarInput, scalarInput => new Schema {Type = "string"});
            var groupedRecSets = recSetOutputs.GroupBy(DataListUtil.ExtractRecordsetNameFromValue);
            foreach (var groupedRecSet in groupedRecSets)
            {
                var recSetName = groupedRecSet.Key;
                var propObject = BuildPropertyDefinition(groupedRecSet);

                var recObject = new Schema
                {
                    Type = "object",
                    Properties = propObject
                };
                propertiesObject.Add(recSetName, recObject);
            }

            var responseSchema = new Dictionary<string, Schema>
            {
                {
                    "schema", new Schema
                    {
                        Type = "object",
                        Properties = propertiesObject,
                    }
                }
            };
            var serialized = JsonConvert.SerializeObject(responseSchema);
            var definitionObject = JsonConvert.DeserializeObject(serialized) as JToken;
            var res = definitionObject;
            return res;
        }

        static Dictionary<string, Schema> BuildPropertyDefinition(IGrouping<string, string> groupedRecSet) => groupedRecSet.ToDictionary(DataListUtil.ExtractFieldNameOnlyFromValue, name => new Schema
        {
            Type = "string"
        });


        public static void ProcessOutputMapping(IExecutionEnvironment environment, int update, ref bool started, ref int rowIdx, DataRow row, IServiceOutputMapping serviceOutputMapping)
        {
            var rsType = DataListUtil.GetRecordsetIndexType(serviceOutputMapping.MappedTo);
            var rowIndex = DataListUtil.ExtractIndexRegionFromRecordset(serviceOutputMapping.MappedTo);

            var rs = serviceOutputMapping.RecordSetName;
            if (!string.IsNullOrEmpty(rs) && environment.HasRecordSet(DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.MakeValueIntoHighLevelRecordset(rs, rsType == enRecordsetIndexType.Star))))
            {
                if (started)
                {
                    rowIdx = environment.GetLength(rs) + 1;
                    started = false;
                }
            }
            else
            {
                try
                {
                    environment.AssignDataShape(serviceOutputMapping.MappedTo);
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                }
            }

            GetRowIndex(ref started, ref rowIdx, rsType, rowIndex);
            if (!row.Table.Columns.Contains(serviceOutputMapping.MappedFrom))
            {
                return;
            }

            var value = row[serviceOutputMapping.MappedFrom];

            var colDataType = row.Table.Columns[serviceOutputMapping.MappedFrom].DataType;
            if (colDataType.Name == "Byte[]")
            {
                value = Encoding.UTF8.GetString(value as byte[]);
            }

            if (update != 0)
            {
                rowIdx = update;
            }

            var displayExpression = DataListUtil.ReplaceRecordsetBlankWithIndex(DataListUtil.AddBracketsToValueIfNotExist(serviceOutputMapping.MappedTo), rowIdx);
            if (rsType == enRecordsetIndexType.Star)
            {
                displayExpression = DataListUtil.ReplaceStarWithFixedIndex(displayExpression, rowIdx);
            }

            environment.Assign(displayExpression, value.ToString(), update);
        }

        static void GetRowIndex(ref bool started, ref int rowIdx, enRecordsetIndexType rsType, string rowIndex)
        {
            if (rsType == enRecordsetIndexType.Star && started)
            {
                rowIdx = 1;
                started = false;
            }

            if (rsType == enRecordsetIndexType.Numeric)
            {
                rowIdx = int.Parse(rowIndex);
            }
        }
    }

    public class Schema
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, Schema> Properties { get; set; }
    }
}