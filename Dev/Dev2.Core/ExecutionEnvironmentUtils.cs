using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data;
using Dev2.Data.Util;
using Warewolf.Storage;
using WarewolfParserInterop;

namespace Dev2
{
    public static class ExecutionEnvironmentUtils
    {
        public static string GetXmlOutputFromEnvironment(IDSFDataObject dataObject, Guid workspaceGuid,string dataList,int update)
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
                        warewolfEvalResult = environment.Eval(name, update);
                    }
                    catch(Exception)
                    {
                        //Possible that the output defs have variables that were never initialised (i.e. null)
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

            result.Append("</" + "DataList" + ">");
            return result.ToString();
        }

        public static string GetJsonOutputFromEnvironment(IDSFDataObject dataObject, Guid workspaceGuid,string dataList,int update)
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
                    var warewolfIterator = new WarewolfIterator(environment.Eval(name, update));
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
                if (i < recSets.Count())
                {
                    result.Append(",");
                }
            }

            var scalars = scalarOutputs as string[] ?? scalarOutputs.ToArray();
            foreach (var output in scalars)
            {
                var evalResult = environment.Eval(DataListUtil.AddBracketsToValueIfNotExist(output), update);
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
                foreach(XmlNode c in children)
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
                            if(recSetDefs.Count() != 0)
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
                                                dataObject.Environment.AssignWithFrame(new AssignValue(recSetAppend, subc.InnerXml), update);
                                            }
                                        }
                                    }
                                }
                            }
                            if(scalarDefs.Count() != 0)
                            {
                                dataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(c.Name), c.InnerXml, update);
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

        public static string GetXmlInputFromEnvironment(IDSFDataObject dataObject, Guid workspaceGuid, string dataList,int update)
        {
            if(dataObject == null)
            {
                throw new ArgumentNullException("dataObject");
            }
            var environment = dataObject.Environment;
            var dataListTO = new DataListTO(dataList);
            var result = new StringBuilder("<DataList>");
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

            result.Append("</" + "DataList" + ">");
            return result.ToString();
        }

    }
}