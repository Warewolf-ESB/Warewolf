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
        public static string GetXmlOutputFromEnvironment(IDSFDataObject dataObject,string dataList)
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
                        warewolfEvalResult = environment.Eval(name);
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
                var evalResult = environment.Eval(DataListUtil.AddBracketsToValueIfNotExist(output));
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

        public static string GetJsonOutputFromEnvironment(IDSFDataObject dataObject,string dataList)
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
                    var warewolfIterator = new WarewolfIterator(environment.Eval(name));
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
                    if (warewolfListIterators.HasMoreData())
                    {
                        result.Append("}");
                        result.Append(",");
                    }
                }
                result.Append("}");
                result.Append("]");
                i++;
                if (i <= recSets.Count())
                {
                    result.Append(",");
                }

            }

            var scalars = scalarOutputs as string[] ?? scalarOutputs.ToArray();
            foreach (var output in scalars)
            {
                var evalResult = environment.Eval(DataListUtil.AddBracketsToValueIfNotExist(output));
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
                if (keyCnt < scalars.Count())
                {
                    result.Append(",");
                }
            }
            var jsonOutputFromEnvironment = result.ToString();
            jsonOutputFromEnvironment += "}";
            return jsonOutputFromEnvironment;
        }

        public static void UpdateEnvironmentFromXmlPayload(IDSFDataObject dataObject, StringBuilder rawPayload, string dataList)
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
                    TryConvert(dataObject, children, dataListTO.Inputs);
                }
            }
        }

        public static void UpdateEnvironmentFromInputPayload(IDSFDataObject dataObject, StringBuilder rawPayload, string dataList)
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
                TryConvert(dataObject, children, dataListTO.Inputs);
            }
        }
        
        public static void UpdateEnvironmentFromOutputPayload(IDSFDataObject dataObject, StringBuilder rawPayload, string dataList)
        {

            string toLoad = DataListUtil.StripCrap(rawPayload.ToString()); // clean up the rubish ;)
            XmlDocument xDoc = new XmlDocument();
            toLoad = string.Format("<Tmp{0}>{1}</Tmp{0}>", Guid.NewGuid().ToString("N"), toLoad);
            xDoc.LoadXml(toLoad);
            dataList = dataList.Replace("ADL>", "DataList>").Replace("root>", "DataList>");
            if (xDoc.DocumentElement != null)
            {
                XmlNodeList children = xDoc.DocumentElement.ChildNodes;
                var dataListTO = new DataListTO(dataList);
                TryConvert(dataObject, children, dataListTO.Outputs);
            }
        }

        static void TryConvert(IDSFDataObject dataObject, XmlNodeList children, List<string> inputDefs, int level = 0)
        {
            try
            {
                // spin through each element in the XML
            foreach (XmlNode c in children)
            {
                if (c.Name != GlobalConstants.NaughtyTextNode)
                {
                    // scalars and recordset fetch
                   if (level>0)
                    {
                        var c1 = c;
                        var scalars = inputDefs.Where(definition => definition == c1.Name);
                        var recSets = inputDefs.Where(definition => DataListUtil.ExtractRecordsetNameFromValue(definition) == c1.Name);
                        var scalarDefs = scalars as string[] ?? scalars.ToArray();
                        var recSetDefs = recSets as string[] ?? recSets.ToArray();
                        if (recSetDefs.Count() != 0)
                        {
                            // fetch recordset index
                            // process recordset
                            var nl = c.ChildNodes;
                            foreach (XmlNode subc in nl)
                            {
                                // Extract column being mapped to ;)
                                foreach (var definition in recSetDefs)
                                {
                                    if (DataListUtil.IsValueRecordset(definition))
                                    {
                                        if (DataListUtil.ExtractFieldNameFromValue(definition) == subc.Name)
                                        {
                                            var recSetAppend = DataListUtil.ReplaceRecordsetIndexWithBlank(definition);
                                            var a = subc.InnerXml;
                                            a = RemoveXMLPrefix(a);
                                                dataObject.Environment.AssignWithFrame(new AssignValue(recSetAppend, a));
                                     
                                        }
                                    }
                                }
                            }
                        }
                        if (scalarDefs.Count() != 0)
                        {
                            // fetch recordset index
                            // process recordset
                            var a = c.InnerXml;
                            a = RemoveXMLPrefix(a);
                            dataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(c.Name), a);
                        }
                    }
                    else
                    {
                        if (level == 0)
                        {
                            // Only recurse if we're at the first level!!
                            TryConvert(dataObject, c.ChildNodes, inputDefs, ++level);
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
            if(a.StartsWith(GlobalConstants.XMLPrefix))
            {
                a = a.Replace(GlobalConstants.XMLPrefix, "");
                a = Encoding.UTF8.GetString(Convert.FromBase64String(a));
            }
            return a;
        }

        public static string GetXmlInputFromEnvironment(IDSFDataObject dataObject, string dataList)
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
                Dictionary<string, IWarewolfIterator> iterators = new Dictionary<string, IWarewolfIterator>();
                foreach (var name in groupedRecSet)
                {
                    var warewolfIterator = new WarewolfIterator(environment.Eval(name));
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
                var evalResult = environment.Eval(DataListUtil.AddBracketsToValueIfNotExist(output));
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

        public static string GetSwaggerOutputForService(IResource resource, string dataList)
        {
            if(resource==null)
            {
                throw new ArgumentNullException("resource");
            }
            if (string.IsNullOrEmpty(dataList))
            {
                throw new ArgumentNullException("dataList");
            }
            var dataListTO = new DataListTO(dataList);

            var scalarInputs = dataListTO.Inputs.Where(s => !DataListUtil.IsValueRecordset(s));

            var scalarOutputs = dataListTO.Outputs.Where(s => !DataListUtil.IsValueRecordset(s));

            var parameters = scalarInputs.Select(scalarInput => new JObject
            {
                { "name", scalarInput }, { "in", "query" }, { "required", true }, { "type", "string" }
            }).ToList();

            var jsonSwaggerInfoObject = new JObject
            {
                { "title", new JValue("") }, 
                { "description", new JValue("") },
                { "version", new JValue(resource.VersionInfo.VersionNumber) }
            };

            var jsonSwaggerPathObject = new JObject
            {
                {"serviceName",new JValue(resource.ResourceName)},
                {"get", new JObject
                    {
                        {"summary",new JValue("")},
                        {"description",new JValue("")},
                        {"parameters",new JArray(parameters)}
                    }
                }
            };

            var jsonSwaggerResponsesObject = new JObject
            {
                {"200",new JArray(scalarOutputs)}
            };

            var jsonSwaggerObject = new JObject
            {
                { "swagger", new JValue(2) }, 
                { "info", jsonSwaggerInfoObject },
                { "host", new JValue(EnvironmentVariables.WebServerUri) }, 
                { "basePath", new JValue("/") }, 
                { "schemes", new JArray("http", "https") }, 
                { "produces", new JValue("application/json") }, 
                { "paths",jsonSwaggerPathObject },
                { "responses", jsonSwaggerResponsesObject }
            };
            
            var converter = new JsonSerializer();
            StringBuilder result = new StringBuilder();
            var jsonTextWriter = new JsonTextWriter(new StringWriter(result)) { Formatting = Newtonsoft.Json.Formatting.Indented };
            converter.Serialize(jsonTextWriter, jsonSwaggerObject);
            jsonTextWriter.Flush();
            var resultString = Regex.Replace(result.ToString(), @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
            return resultString;
        }
    }


}