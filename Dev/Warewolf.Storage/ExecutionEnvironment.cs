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

using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Warewolf.Data;
using Warewolf.Exceptions;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;
using static DataStorage;

namespace Warewolf.Storage
{
    public class ExecutionEnvironment : IExecutionEnvironment
    {
        protected DataStorage.WarewolfEnvironment _env;
        readonly IBuildIndexMap _buildIndexMap;

        public ExecutionEnvironment()
        {
            _env = PublicFunctions.CreateEnv(@"");
            Errors = new HashSet<string>();
            AllErrors = new HashSet<string>();
            Id = Guid.NewGuid();

            _buildIndexMap = new BuildJObjectArrayIndexMapHelper(this);
        }

        public CommonFunctions.WarewolfEvalResult Eval(string exp, int update) => Eval(exp, update, false, false);

        public CommonFunctions.WarewolfEvalResult Eval(string exp, int update, bool throwsifnotexists) => Eval(exp, update, throwsifnotexists, false);

        public CommonFunctions.WarewolfEvalResult Eval(string exp, int update, bool throwsifnotexists, bool shouldEscape)
        {
            try
            {
                return PublicFunctions.EvalEnvExpression(exp, update, shouldEscape, _env);
            }
            catch (IndexOutOfRangeException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (throwsifnotexists || e is IndexOutOfRangeException || e.Message.Contains(@"index was not an int"))
                {
                    throw;
                }

                return CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing);
            }
        }

        public CommonFunctions.WarewolfEvalResult EvalForJson(string exp) => EvalForJson(exp, false);

        public CommonFunctions.WarewolfEvalResult EvalForJson(string exp, bool shouldEscape)
        {
            if (string.IsNullOrEmpty(exp))
            {
                return CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing);
            }
            try
            {
                return PublicFunctions.EvalEnvExpression(exp, 0, shouldEscape, _env);
            }
            catch (IndexOutOfRangeException)
            {
                throw;
            }
            catch (Exception)
            {
                if (!IsRecordsetIdentifier(exp))
                {
                    return CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing);
                }

                var res = new WarewolfAtomList<DataStorage.WarewolfAtom>(DataStorage.WarewolfAtom.Nothing);
                res.AddNothing();
                return CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(res);
            }
        }

        public void AddToJsonObjects(string exp, JContainer jContainer)
        {
            _env = WarewolfDataEvaluationCommon.addToJsonObjects(_env, exp, jContainer);
        }

        public IEnumerable<CommonFunctions.WarewolfEvalResult> EvalForDataMerge(string exp, int update) => DataMergeFunctions.evalForDataMerge(_env, update, exp);

        public void AssignUnique(IEnumerable<string> distinctList, IEnumerable<string> valueList, IEnumerable<string> resList, int update)
        {
            var output = Distinct.evalDistinct(_env, distinctList, valueList, update, resList);
            _env = output;
        }

        public CommonFunctions.WarewolfEvalResult EvalStrict(string exp, int update)
        {
            var res = Eval(exp, update);
            if (IsNothing(res))
            {
                throw new NullValueInVariableException($"The expression {exp} has no value assigned.", exp);
            }

            return res;
        }

        public void Assign(string exp, string value, int update)
        {
            if (string.IsNullOrEmpty(exp))
            {
                return;
            }
            try
            {
                var envTemp = PublicFunctions.EvalAssignWithFrame(new AssignValue(exp, value), update, _env);

                _env = envTemp;
                CommitAssign();
            }
            catch (Exception err)
            {
                Errors.Add(err.Message);
            }
        }
        public void AssignString(string exp, string value, int update)
        {
            if (string.IsNullOrEmpty(exp))
            {
                return;
            }
            try
            {
                var envTemp = PublicFunctions.EvalAssignWithFrameTypeCast(new AssignValue(exp, value), update, _env, ShouldTypeCast.No);

                _env = envTemp;
                CommitAssign();
            }
            catch (Exception err)
            {
                Errors.Add(err.Message);
            }
        }

        public void AssignStrict(string exp, string value, int update)
        {
            if (string.IsNullOrEmpty(exp))
            {
                return;
            }

            try
            {
                var envTemp = PublicFunctions.EvalAssignWithFrameStrict(new AssignValue(exp, value), update, _env);

                _env = envTemp;
                CommitAssign();
            }
            catch (Exception err)
            {
                Errors.Add(err.Message);
            }
        }


        public void AssignWithFrame(IAssignValue values, int update)
        {
            try
            {

                var envTemp = PublicFunctions.EvalAssignWithFrame(values, update, _env);
                _env = envTemp;

            }
            catch (Exception err)
            {
                Errors.Add(err.Message);
                throw;
            }
        }
        public void AssignWithFrame(IEnumerable<IAssignValue> values, int update)
        {
            foreach (var value in values)
            {
                AssignWithFrame(value, update);
            }
        }
        public int GetLength(string recordSetName)
        {
            if (recordSetName.Length > 1 && recordSetName[0] == '@')
            {
                throw new Exception("not a recordset");
            }
            return _env.RecordSets[recordSetName.Trim()].LastIndex;
        }
        internal JContainer GetObject(string objectName) => _env.JsonObjects[objectName];

        public int GetObjectLength(string recordSetName)
        {
            JToken findArray(JObject ob, string[] names)
            {
                if (names.Length == 1)
                {
                    return ob[names[0]];
                }
                return findArray(ob[names[0]] as JObject, names.Skip(1).ToArray());
            }

            try
            {
                var trimStart = recordSetName.TrimStart('@');
                var parts = trimStart.Split('.');
                if (parts.Length < 2)
                {
                    return _env.JsonObjects[trimStart].Count;
                }

                var arr = findArray(_env.JsonObjects[parts[0]] as JObject, parts.Skip(1).ToArray());



                return arr.Count();
            }
            catch (KeyNotFoundException e)
            {
                throw new Exception("not a json array", e);
            }
        }

        public int GetCount(string recordSetName) => _env.RecordSets[recordSetName.Trim()].Count;

        public IList<int> EvalRecordSetIndexes(string recordsetName, int update) => PublicFunctions.GetIndexes(recordsetName, update, _env).ToList();

        public bool HasRecordSet(string recordsetName)
        {
            var x = EvaluationFunctions.parseLanguageExpression(recordsetName, 0, ShouldTypeCast.Yes);
            if (x.IsRecordSetNameExpression && x is LanguageAST.LanguageExpression.RecordSetNameExpression recsetName)
            {
                return _env.RecordSets.ContainsKey(recsetName.Item.Name);
            }
            return false;
        }

        public IList<string> EvalAsListOfStrings(string expression, int update)
        {
            var result = Eval(expression, update);
            if (result.IsWarewolfAtomResult && result is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult x)
            {
                return new List<string> { WarewolfAtomToString(x.Item) };
            }
            if (result.IsWarewolfRecordSetResult && result is CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult recSetResult)
            {
                var recSetData = recSetResult.Item;
                if (recSetData != null)
                {
                    return EvalListOfStringsHelper(recSetData);
                }
            }
            if (result is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult warewolfAtomListresult)
            {
                var item = warewolfAtomListresult.Item;
                return item.Select(WarewolfAtomToString).ToList();
            }
            throw new Exception(string.Format(ErrorResource.CouldNotRetrieveStringsFromExpression, expression));
        }

        private static IList<string> EvalListOfStringsHelper(DataStorage.WarewolfRecordset recSetData)
        {
            var data = recSetData.Data.ToArray();
            var listOfData = new List<string>();
            foreach (var keyValuePair in data)
            {
                if (keyValuePair.Key == "WarewolfPositionColumn")
                {
                    continue;
                }
                listOfData.AddRange(keyValuePair.Value.Select(WarewolfAtomToString).ToList());
            }
            return listOfData;
        }

        public static string WarewolfAtomToString(DataStorage.WarewolfAtom a) => a?.ToString() ?? string.Empty;

        public static string WarewolfAtomToStringNullAsNothing(DataStorage.WarewolfAtom a) => a == null ? null : (a.IsNothing ? null : a.ToString());

        public static string WarewolfAtomToStringErrorIfNull(DataStorage.WarewolfAtom a)
        {
            if (a == null)
            {
                return string.Empty;
            }

            if (a.IsNothing)
            {
                throw new NullValueInVariableException(ErrorResource.VariableIsNull, string.Empty);
            }
            return a.ToString();
        }

        public static bool IsRecordSetName(string a)
        {
            try
            {
                var x = EvaluationFunctions.parseLanguageExpression(a, 0, ShouldTypeCast.Yes);
                return x.IsRecordSetNameExpression;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsValidVariableExpression(string expression, out string errorMessage, int update)
        {
            errorMessage = string.Empty;
            try
            {
                var x = EvaluationFunctions.parseLanguageExpression(expression, update, ShouldTypeCast.Yes);
                if (x.IsRecordSetExpression || x.IsScalarExpression || x.IsJsonIdentifierExpression ||
                    x.IsRecordSetNameExpression)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }
            return false;
        }

        public static string WarewolfEvalResultToString(CommonFunctions.WarewolfEvalResult result)
        {
            if (result.IsWarewolfAtomResult && result is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult warewolfAtomResult)
            {
                var x = warewolfAtomResult.Item;
                if (x.IsNothing)
                {
                    return null;
                }

                return WarewolfAtomToStringErrorIfNull(x);
            }
            if (result.IsWarewolfRecordSetResult && result is CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult recSetResult)
            {
                var recSetData = recSetResult.Item;
                if (recSetData != null)
                {
                    return WarewolfEvalResultToStringHelper(recSetData);
                }
            }
            if (result is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult warewolfAtomListresult)
            {
                var x = warewolfAtomListresult.Item;
                var res = new StringBuilder();
                for (int index = 0; index < x.Count; index++)
                {
                    var warewolfAtom = x[index];
                    if (index == x.Count - 1)
                    {
                        res.Append(warewolfAtom);
                    }
                    else
                    {
                        res.Append(warewolfAtom).Append(@",");
                    }
                }
                return res.ToString();
            }
            throw new Exception(@"Null when value should have been returned.");
        }

        private static string WarewolfEvalResultToStringHelper(DataStorage.WarewolfRecordset recSetData)
        {
            var data = recSetData.Data.ToArray();
            var listOfData = new List<string>();
            foreach (var keyValuePair in data)
            {
                if (keyValuePair.Key == "WarewolfPositionColumn")
                {
                    continue;
                }
                listOfData.AddRange(keyValuePair.Value.Select(WarewolfAtomToString).ToList());
            }
            return string.Join(",", listOfData);
        }

        public void EvalAssignFromNestedStar(string exp, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update)
        {
            _env = PublicFunctions.EvalAssignFromList(exp, recsetResult.Item, _env, update, false);
        }

        public void EvalAssignFromNestedLast(string exp, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update)
        {
            var expression = exp;
            var exists = PublicFunctions.RecordsetExpressionExists(expression, _env);
            if (!exists)
            {
                expression = ToStar(expression);
            }

            _env = PublicFunctions.EvalAssignFromList(expression, recsetResult.Item, _env, update, exists);
        }

        public void EvalAssignFromNestedNumeric(string rawValue, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update)
        {
            if (recsetResult.Item.Any())
            {
                AssignWithFrame(new AssignValue(rawValue, WarewolfAtomToString(recsetResult.Item.Last())), update);
            }
        }

        public void EvalDelete(string exp, int update)
        {
            _env = PublicFunctions.EvalDelete(exp, update, _env);
        }

        public void CommitAssign()
        {
            _env = PublicFunctions.RemoveFraming(_env);
        }

        public void SortRecordSet(string sortField, bool descOrder, int update)
        {
            _env = PublicFunctions.SortRecset(sortField, descOrder, update, _env);
        }

        public string ToStar(string expression)
        {
            var exp = EvaluationFunctions.parseLanguageExpression(expression, 0, ShouldTypeCast.Yes);
            if (exp.IsRecordSetExpression && exp is LanguageAST.LanguageExpression.RecordSetExpression rec)
            {
                return $"[[{rec.Item.Name}(*).{rec.Item.Column}]]";
            }

            if (exp.IsRecordSetNameExpression && exp is LanguageAST.LanguageExpression.RecordSetNameExpression recNameExp)
            {
                return $"[[{recNameExp.Item.Name}(*)]]";
            }

            if (exp.IsJsonIdentifierExpression)
            {
                var replace = expression.Replace(@"()", @"(*)");
                return replace;
            }
            return expression;
        }
        public IEnumerable<Tuple<string, DataStorage.WarewolfAtom>[]> EvalAsTable(string recordsetExpression, int update) => EvalAsTable(recordsetExpression, update, false);
        public IEnumerable<Tuple<string, DataStorage.WarewolfAtom>[]> EvalAsTable(string recordsetExpression, int update, bool throwsifnotexists)

        {
            var result = PublicFunctions.EvalEnvExpressionToTable(recordsetExpression, update, _env, throwsifnotexists);
            return result;
        }

        public IEnumerable<DataStorage.WarewolfAtom> EvalAsList(string expression, int update) => EvalAsList(expression, update, false);
        public IEnumerable<DataStorage.WarewolfAtom> EvalAsList(string expression, int update, bool throwsifnotexists)
        {
            var result = Eval(expression, update, throwsifnotexists);
            if (result.IsWarewolfAtomResult && result is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult warewolfAtomResult)
            {
                var item = warewolfAtomResult.Item;
                return new List<DataStorage.WarewolfAtom> { item };
            }
            if (result.IsWarewolfRecordSetResult && result is CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult recSetResult)
            {
                var recSetData = recSetResult.Item;
                if (recSetData != null)
                {
                    return EvalAsListHelper(recSetData);
                }
            }

            // BUG: this will just return null if there is an unhandled Warewolf Atom Result of some sort
            var x = (result as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult)?.Item;
            return x?.ToList();
        }

        private static IEnumerable<DataStorage.WarewolfAtom> EvalAsListHelper(DataStorage.WarewolfRecordset recSetData)
        {
            var data = recSetData.Data.ToArray();
            var listOfData = new List<DataStorage.WarewolfAtom>();
            foreach (var keyValuePair in data)
            {
                if (keyValuePair.Key == "WarewolfPositionColumn")
                {
                    continue;
                }
                listOfData.AddRange(keyValuePair.Value.ToList());
            }
            return listOfData;
        }

        public IEnumerable<int> EvalWhere(string expression, Func<DataStorage.WarewolfAtom, bool> clause, int update) => PublicFunctions.EvalWhere(expression, _env, update, clause);

        public void ApplyUpdate(string expression, Func<DataStorage.WarewolfAtom, DataStorage.WarewolfAtom> clause, int update)
        {

            var temp = PublicFunctions.EvalUpdate(expression, _env, update, clause);
            _env = temp;
        }

        public HashSet<string> Errors { get; }

        public HashSet<string> AllErrors { get; }
        public Guid Id { get; protected set; }
        public Guid ParentId { get; protected set; }

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public void AssignDataShape(string p)
        {
            var env = PublicFunctions.EvalDataShape(p, _env);
            _env = env;
        }

        public string FetchErrors() => string.Join(Environment.NewLine, AllErrors.Union(Errors));

        public bool HasErrors() => Errors.Any(s => !string.IsNullOrEmpty(s)) || AllErrors.Any(s => !string.IsNullOrEmpty(s));

        public string EvalToExpression(string exp, int update) => string.IsNullOrEmpty(exp) ? string.Empty : EvaluationFunctions.evalToExpression(_env, update, exp);

        public static string ConvertToIndex(string outputVar, int i)
        {
            var output = EvaluationFunctions.parseLanguageExpression(outputVar, 0, ShouldTypeCast.Yes);
            if (output.IsRecordSetExpression && output is LanguageAST.LanguageExpression.RecordSetExpression recSetExpr)
            {
                var outputidentifier = recSetExpr.Item;
                if (Equals(outputidentifier?.Index, LanguageAST.Index.Star))
                {
                    return $"[[{outputidentifier?.Name}({i}).{outputidentifier?.Column}]]";
                }
            }
            return outputVar;
        }

        public static bool IsRecordsetIdentifier(string assignVar)
        {
            try
            {
                var x = EvaluationFunctions.parseLanguageExpression(assignVar, 0, ShouldTypeCast.Yes);
                return x.IsRecordSetExpression;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsScalar(string assignVar)
        {
            try
            {
                var x = EvaluationFunctions.parseLanguageExpression(assignVar, 0, ShouldTypeCast.Yes);
                return x.IsScalarExpression;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsNothing(CommonFunctions.WarewolfEvalResult evalInp1) => CommonFunctions.isNothing(evalInp1);

        public static string GetPositionColumnExpression(string recordset)
        {
            var rec = EvaluationFunctions.parseLanguageExpression(recordset, 0, ShouldTypeCast.Yes);
            if (!rec.IsRecordSetExpression && !rec.IsRecordSetNameExpression)
            {
                return recordset;
            }

            if (rec is LanguageAST.LanguageExpression.RecordSetExpression recordSetExpression)
            {
                var index = recordSetExpression.Item?.Name;
                return $"[[{index}(*).{EvaluationFunctions.PositionColumn}]]";
            }
            if (rec is LanguageAST.LanguageExpression.RecordSetNameExpression recordSetNameExpression)
            {
                var index = recordSetNameExpression.Item?.Name;
                return $"[[{index}(*).{EvaluationFunctions.PositionColumn}]]";
            }
            // This is a fail safe
            throw new Exception("Unhandled Recordset LanguageExpression");
        }

        public static bool IsValidRecordSetIndex(string exp) => PublicFunctions.IsValidRecsetExpression(exp);

        public void AssignJson(IEnumerable<IAssignValue> values, int update)
        {
            foreach (var value in values)
            {
                AssignJson(value, update);
            }
        }

        public void AssignJson(IAssignValue value, int update)
        {
            try
            {
                if (string.IsNullOrEmpty(value.Name))
                {
                    return;
                }
                var envTemp = AssignEvaluation.evalJsonAssign(value, update, _env, ShouldTypeCast.Yes);
                _env = envTemp;
            }
            catch (Exception err)
            {
                Errors.Add(err.Message);
                throw;
            }
        }

        public JContainer EvalJContainer(string exp)
        {
            if (string.IsNullOrEmpty(exp))
            {
                return null;
            }

            var parsedExpression = EvaluationFunctions.parseLanguageExpressionWithoutUpdate(exp, ShouldTypeCast.Yes);
            if (parsedExpression.IsJsonIdentifierExpression && parsedExpression is LanguageAST.LanguageExpression.JsonIdentifierExpression jsonIdentifierExpression)
            {
                if (jsonIdentifierExpression.Item is LanguageAST.JsonIdentifierExpression.NameExpression nameExpression)
                {
                    return _env.JsonObjects[nameExpression.Item.Name];
                }
                if (jsonIdentifierExpression.Item is LanguageAST.JsonIdentifierExpression.IndexNestedNameExpression arrayExpression)
                {
                    return _env.JsonObjects[arrayExpression.Item.ObjectName];
                }
            }
            // BUG: if any other types are added to JsonIdentifierExpression this method will no longer be correct.
            return null;
        }

        public List<string> GetIndexes(string exp)
        {
            IList<string> indexMap = null;

            if (!string.IsNullOrEmpty(exp))
            {
                var parsedExpression = EvaluationFunctions.parseLanguageExpressionWithoutUpdate(exp, ShouldTypeCast.Yes);

                if (parsedExpression.IsJsonIdentifierExpression)
                {
                    if (parsedExpression is LanguageAST.LanguageExpression.JsonIdentifierExpression jsonIdentifierExpression)
                    {
                        indexMap = _buildIndexMap.Build(jsonIdentifierExpression.Item, exp);
                    }
                }
                else
                {
                    if (parsedExpression.IsRecordSetExpression && parsedExpression is LanguageAST.LanguageExpression.RecordSetExpression recSetExpression)
                    {
                        indexMap = GetIndexAddRecordSetIndexes(exp, recSetExpression);
                    }

                }
            }

            if (indexMap is null)
            {
                return new List<string>();
            }

            return indexMap.Where(s => !s.Contains(@"(*)")).ToList();
        }

        private IList<string> GetIndexAddRecordSetIndexes(string exp, LanguageAST.LanguageExpression.RecordSetExpression recSetExpression)
        {
            var indexMap = new List<string>();
            var indexes = EvalRecordSetIndexes(@"[[" + recSetExpression.Item.Name + @"(*)]]", 0);
            foreach (var index in indexes)
            {
                indexMap.Add(exp.Replace(@"(*).", $"({index})."));
            }
            return indexMap;
        }

        public string ToJson()
        {
            using (var helper = new EnvironmentToJsonHelper())
            {
                helper.WriteVariables(this._env);
                helper.WriteErrors(this.Errors, this.AllErrors);
                return helper.GetJson();
            }
        }

        public void FromJson(string serializedEnv)
        {
            EnvironmentToJsonHelper.FromJson(serializedEnv, this);
        }

        public IExecutionEnvironment Snapshot()
        {
            var clonedExecutionEnvironment = MemberwiseClone() as ExecutionEnvironment;
            clonedExecutionEnvironment.Id = Guid.NewGuid();
            clonedExecutionEnvironment.ParentId = Id;
            clonedExecutionEnvironment._env = PublicFunctions.CreateEnv(@"");
            var executionEnvironmentToJson = ToJson();
            clonedExecutionEnvironment.FromJson(executionEnvironmentToJson);
            return clonedExecutionEnvironment;
        }

        private class EnvironmentToJsonHelper : IDisposable
        {
            readonly MemoryStream _stream = new MemoryStream();
            readonly JsonTextWriter _jsonWriter;

            public EnvironmentToJsonHelper()
            {
                _jsonWriter = new JsonTextWriter(new StreamWriter(_stream));
                OpenJson();
            }


            protected void OpenJson()
            {
                _jsonWriter.WriteStartObject();
            }
            public void WriteVariables(DataStorage.WarewolfEnvironment _env)
            {
                _jsonWriter.WritePropertyName("Environment");
                _jsonWriter.WriteRawValue(VariablesToJson(_env));
            }
            static protected string VariablesToJson(DataStorage.WarewolfEnvironment _env)
            {
                var stringList = PublicFunctions.EvalEnv(_env);
                var sb = new StringBuilder(4096);
                foreach (var @string in stringList)
                {
                    sb.Append(@string);
                }
                return sb.ToString();
            }

            internal void WriteErrors(HashSet<string> errors, HashSet<string> allErrors)
            {
                var serializer = new JsonSerializer();
                _jsonWriter.WritePropertyName("Errors");
                serializer.Serialize(_jsonWriter, errors);

                _jsonWriter.WritePropertyName("AllErrors");
                serializer.Serialize(_jsonWriter, allErrors);
            }

            protected void CloseJson()
            {
                _jsonWriter.WriteEndObject();
                _jsonWriter.Flush();
            }
            public string GetJson()
            {
                CloseJson();
                _stream.Seek(0, SeekOrigin.Begin);
                var reader = new StreamReader(_stream);
                return reader.ReadToEnd();
            }

            public void Dispose()
            {
                ((IDisposable)_jsonWriter).Dispose();
                _stream.Dispose();
            }

            internal static void FromJson(string serializedEnv, ExecutionEnvironment environment)
            {
                if (string.IsNullOrEmpty(serializedEnv))
                {
                    return;
                }

                var jsonEnv = GetEnvironmentJObject(serializedEnv);
                if (jsonEnv is null)
                {
                    return;
                }
                var env = (JObject)jsonEnv.Property("Environment")?.Value;
                var jsonScalars = env?.Property("scalars")?.Value as JObject;
                var jsonRecSets = env?.Property("record_sets")?.Value as JObject;
                var jsonJObjects = env?.Property("json_objects")?.Value as JObject;

                AssignScalarData(environment, jsonScalars);
                AssignRecSetData(environment, jsonRecSets);
                AssignJsonData(environment, jsonJObjects);

            }

            private static JObject GetEnvironmentJObject(string serializedEnv)
            {
                var serializer = new Dev2JsonSerializer();
                JObject jsonEnv = null;
                try
                {
                    jsonEnv = serializer.Deserialize<JObject>(serializedEnv);
                }
                catch (JsonReaderException jre)
                {
                    Dev2Logger.Error($"Error Deserializing Environment: {serializedEnv}", jre, GlobalConstants.WarewolfError);
                }

                return jsonEnv;
            }

            private static void AssignJsonData(ExecutionEnvironment environment, JObject jsonJObjects)
            {
                if (jsonJObjects is null)
                {
                    return;
                }

                foreach (var jsonObj in jsonJObjects.Properties())
                {
                    environment.Assign($"[[@{jsonObj.Name}]]", jsonObj.Value.ToString(), 0);
                }
            }

            private static void AssignRecSetData(ExecutionEnvironment environment, JObject jsonRecSets)
            {
                if (jsonRecSets is null)
                {
                    return;
                }

                foreach (var recSetObj in jsonRecSets.Properties())
                {
                    AssignRecSetData(environment, recSetObj);

                }
            }

            private static void AssignScalarData(ExecutionEnvironment environment, JObject jsonScalars)
            {
                if (jsonScalars is null)
                {
                    return;
                }

                foreach (var scalarObj in jsonScalars.Properties())
                {
                    environment.Assign($"[[{scalarObj.Name}]]", (string)scalarObj.Value, 0);
                }
            }

            private static void AssignRecSetData(ExecutionEnvironment environment, JProperty recSetObj)
            {
                void AssignRecSetDataItem(List<JToken> positionItems, JProperty recSetData)
                {
                    if (recSetData.Name != "WarewolfPositionColumn")
                    {
                        var dataItems = (recSetData.Value as JArray).ToList();
                        int i = 0;
                        foreach (var dataValue in dataItems)
                        {
                            var index = positionItems[i].ToString();
                            environment.Assign($"[[{recSetObj.Name}({index}).{recSetData.Name}]]", dataValue.ToString(), 0);
                            i++;
                        }
                    }
                }

                if (recSetObj != null && recSetObj.Value is JObject recSetDataObj)
                {
                    var positionItems = (recSetDataObj.Property("WarewolfPositionColumn").Value as JArray).ToList();
                    foreach (var recSetData in recSetDataObj.Properties())
                    {
                        AssignRecSetDataItem(positionItems, recSetData);
                    }
                }
            }
        }
    }
}
