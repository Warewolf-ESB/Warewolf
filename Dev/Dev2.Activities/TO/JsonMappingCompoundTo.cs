
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Warewolf.Storage;
using WarewolfParserInterop;

namespace Dev2.TO
{
    public class JsonMappingEvaluated
    {
        readonly IExecutionEnvironment _env;
        // ReSharper disable MemberCanBePrivate.Global
        public JsonMappingTo Simple { get; set; }
        // ReSharper restore MemberCanBePrivate.Global
        object _evalResultAsObject;
        WarewolfDataEvaluationCommon.WarewolfEvalResult _evalResult;

        public JsonMappingEvaluated(
            IExecutionEnvironment env,
            string sourceName)
        {
            _env = env;
            Simple = new JsonMappingTo
            {
                SourceName = sourceName,
                DestinationName = CalculateDestinationNameFromSourceName(sourceName)
            };
        }

        string CalculateDestinationNameFromSourceName(string sourceName)
        {
            LanguageAST.LanguageExpression parsed = WarewolfDataEvaluationCommon.parseLanguageExpression(sourceName,0);
            if (parsed.IsScalarExpression)
            {
                return ((LanguageAST.LanguageExpression.ScalarExpression)parsed).Item;
            }
            if (parsed.IsRecordSetExpression)
            {
                return ((LanguageAST.LanguageExpression.RecordSetExpression)parsed).Item.Column;
            }
            return ((LanguageAST.LanguageExpression.RecordSetNameExpression)parsed).Item.Name;
        }

        public WarewolfDataEvaluationCommon.WarewolfEvalResult EvalResult
        {
            get
            {
                return _evalResult ?? (_evalResult = _env.EvalForJson(
                    Simple.SourceName));
            }
        }

        public object EvalResultAsObject
        {
            get
            {
                if (_evalResultAsObject == null)
                {
                    WarewolfDataEvaluationCommon.WarewolfEvalResult e = EvalResult;
                    _evalResultAsObject = WarewolfDataEvaluationCommon.evalResultToJsonCompatibleObject(e);
                    if (EvalResult.IsWarewolfAtomListresult && _evalResultAsObject == null)
                    {
                        _evalResultAsObject = new object[] { null };
                    }
                    if (e.IsWarewolfAtomResult)
                    {
                        var x = e as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                        if (x != null && x.Item.IsDataString)
                        {
                            if (((DataASTMutable.WarewolfAtom.DataString)x.Item).Item == "true")
                            {
                                _evalResultAsObject = true;
                            }
                            else if (((DataASTMutable.WarewolfAtom.DataString)x.Item).Item == "false")
                            {
                                _evalResultAsObject = false;
                            }
                        }
                    }
                }
                return _evalResultAsObject;
            }
        }

        public int Count
        {
            get
            {
                if (EvalResult.IsWarewolfAtomResult)
                {
                    return 1;
                }
                if (EvalResult.IsWarewolfAtomListresult)
                {
                    return ((WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult)EvalResult).Item.Count;
                }
                return 1;
            }
        }
    }

    public class JsonMappingCompoundTo
    {
        readonly IExecutionEnvironment _env;
        JsonMappingTo Compound { get; set; }
        List<JsonMappingEvaluated> Evaluations { get; set; }

        public JsonMappingCompoundTo(
            IExecutionEnvironment env,
            JsonMappingTo compound)
        {
            _env = env;

            Compound = compound;
            Evaluations = new List<JsonMappingEvaluated>();

            if (!IsCompound)
            {
                Evaluations.Add(new JsonMappingEvaluated(_env, compound.SourceName));
            }
            else
            {
                if (WarewolfDataEvaluationCommon.parseLanguageExpression(Compound.SourceName,0).IsRecordSetNameExpression)
                {
                    Evaluations = new List<JsonMappingEvaluated> { new JsonMappingEvaluated(_env, Compound.SourceName) };
                }
                else
                {
                    // we know this is a comma seperated list of expressions
                    Evaluations =
                        // ReSharper disable MaximumChainedReferences
                        ((LanguageAST.LanguageExpression.ComplexExpression)WarewolfDataEvaluationCommon.parseLanguageExpression(Compound.SourceName,0))
                            .Item
                            .Where(x => !x.IsWarewolfAtomAtomExpression)
                            .Select(WarewolfDataEvaluationCommon.languageExpressionToString)
                            .Select(x =>
                                new JsonMappingEvaluated(_env, x))
                            .ToList();
                }         // ReSharper restore MaximumChainedReferences
            }
        }

        public WarewolfDataEvaluationCommon.WarewolfEvalResult EvalResult
        {
            get
            {
                return Evaluations.First().EvalResult;
            }
        }

        bool? _isCompound;
        public bool IsCompound
        {
            get
            {
                if (_isCompound == null)
                {
                    _isCompound = WarewolfDataEvaluationCommon.parseLanguageExpression(
                        Compound.SourceName,0)
                        .IsComplexExpression || WarewolfDataEvaluationCommon.parseLanguageExpression(
                            Compound.SourceName,0)
                            .IsRecordSetNameExpression;

                }
                return (bool)_isCompound;
            }
        }

        public int MaxCount
        {
            get
            {
                return Evaluations.Select(x => x.Count).Max();
            }
        }

        public string DestinationName
        {
            get
            {
                return Compound.DestinationName;
            }
        }

        public object EvaluatedResultIndexed(int i)
        {
            return i < MaxCount ?
                Evaluations.First().EvalResultAsObject :
                Evaluations.First().EvalResult.IsWarewolfAtomListresult ? new object[] { null } : null;
        }

        public object ComplexEvaluatedResultIndexed(int i)
        {
            var a = new JObject();
            if (Evaluations.Any(x => x.EvalResult.IsWarewolfAtomListresult))
            {
                return CreateArrayOfResults();
            }
            if (Evaluations.Any(x => x.EvalResult.IsWarewolfRecordSetResult))
            {
                return CreateArrayOfObjectsFromRecordSet(i);
            }

            CreateScalarObject(a);
            return a;
        }

        void CreateScalarObject(JObject a)
        {
            foreach (JsonMappingEvaluated jsonMappingEvaluated in Evaluations)
            {
                a.Add(new JProperty(
                    jsonMappingEvaluated.Simple.DestinationName,
                    WarewolfDataEvaluationCommon.evalResultToJsonCompatibleObject(jsonMappingEvaluated.EvalResult))
                    );
            }
        }

        object CreateArrayOfObjectsFromRecordSet(int i)
        {
            JsonMappingEvaluated jsonMappingEvaluated = Evaluations.First();
            return new JProperty(
                        jsonMappingEvaluated.Simple.DestinationName,
                GetEvalResult(jsonMappingEvaluated.EvalResult, i));
        }

        object CreateArrayOfResults()
        {
            var objects = new List<JObject>(MaxCount);
            for (int j = 0; j < MaxCount; j++)
            {
                var obj = new JObject();
                foreach (JsonMappingEvaluated jsonMappingEvaluated in Evaluations)
                {
                    obj.Add(new JProperty(
                        jsonMappingEvaluated.Simple.DestinationName,
                        GetEvalResult(jsonMappingEvaluated.EvalResult, j))
                        );
                }
                objects.Add(obj);
            }
            return objects.ToArray();
        }

        object GetEvalResult(WarewolfDataEvaluationCommon.WarewolfEvalResult evalResult, int i)
        {
            if (evalResult.IsWarewolfAtomListresult)
            {
                WarewolfAtomList<DataASTMutable.WarewolfAtom> lst = ((WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult)evalResult).Item;
                if (i > lst.Count)
                {
                    return null;
                }
                return WarewolfDataEvaluationCommon.atomToJsonCompatibleObject(lst[i]);
            }
            if (evalResult.IsWarewolfAtomResult)
            {
                if (i == 0)
                {
                    return WarewolfDataEvaluationCommon.evalResultToJsonCompatibleObject(evalResult);
                }
                return null;
            }
            if (evalResult.IsWarewolfRecordSetResult)
            {
                DataASTMutable.WarewolfRecordset recset = ((WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfRecordSetResult)EvalResult).Item;

                KeyValuePair<string, WarewolfAtomList<DataASTMutable.WarewolfAtom>>[] data = recset.Data.ToArray();
                var jObjects = new List<JObject>();
                for (int j = 0; j < recset.Count; j++)
                {
                    var a = new JObject();
                    foreach (KeyValuePair<string, WarewolfAtomList<DataASTMutable.WarewolfAtom>> pair in data)
                    {
                        if (pair.Key != WarewolfDataEvaluationCommon.PositionColumn)
                        {
                            try
                            {
                                a.Add(new JProperty(pair.Key, WarewolfDataEvaluationCommon.atomToJsonCompatibleObject(pair.Value[j])));
                            }
                            catch (Exception)
                            {
                                a.Add(new JProperty(pair.Key, null));
                            }
                        }
                    }
                    jObjects.Add(a);
                }
                return jObjects;
            }
            throw new Exception("Invalid result type was encountered from warewolfstorage");
        }

        public static string IsValidJsonMappingInput(string sourceName, string destinationName)
        {
            if (string.IsNullOrEmpty(sourceName)) return "Must supply a Source Name";
            if (string.IsNullOrEmpty(destinationName)) return "Must supply a Destination Name";

            return ValidateInput(sourceName);


        }

        public static string ValidateInput(string sourceName)
        {
            try
            {
                var parsed = WarewolfDataEvaluationCommon.parseLanguageExpression(sourceName,0);
                if (parsed.IsComplexExpression)
                {
                    var complex = (LanguageAST.LanguageExpression.ComplexExpression)parsed;
                    if (complex.Item
                        .Any(x => x.IsRecordSetNameExpression))
                    {
                        return "Cannot specify a Recordset as part of a comma saperated list of expressions";
                    }
                    if (complex.Item.Count() < 3 ||
                        complex.Item.Count() % 2 != 1 ||
                        // ReSharper disable MaximumChainedReferences
                       !Enumerable.Range(1, complex.Item.Count() - 1)
                           .Where(i => i % 2 == 1)
                           .Select(i =>

                                        WarewolfDataEvaluationCommon.languageExpressionToString(
                                            complex.Item.ElementAt(i)
                                            ) == ",")
                           .Aggregate((a, b) => a && b))
                    // ReSharper restore MaximumChainedReferences
                    {
                        return "Problem with input: expressions must be comma separated";
                    }
                }
                else if (!parsed.IsRecordSetNameExpression &&
                        !parsed.IsRecordSetExpression &&
                        !parsed.IsScalarExpression &&
                        !parsed.IsWarewolfAtomAtomExpression)
                {
                    return "Can only have a scalar, a RecordSet or a RecordSet with column qualification as input";
                }
            }
            catch (Exception)
            {
                return "Unable to parse the Source Name";
            }
            return null;
        }

        public bool HasRecordSetInCompound
        {
            get
            {
                return
                    IsCompound &&
                    Evaluations.Any(x =>
                    WarewolfDataEvaluationCommon.parseLanguageExpression(x.Simple.SourceName,0).IsRecordSetNameExpression);
            }
        }
    }
}
