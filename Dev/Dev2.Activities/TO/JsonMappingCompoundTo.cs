/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Warewolf.Resource.Errors;
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
        CommonFunctions.WarewolfEvalResult _evalResult;

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
            LanguageAST.LanguageExpression parsed = FsInteropFunctions.ParseLanguageExpression(sourceName,0);
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

        public CommonFunctions.WarewolfEvalResult EvalResult => _evalResult ?? (_evalResult = _env.EvalForJson(
            Simple.SourceName));

        public object EvalResultAsObject
        {
            get
            {
                if (_evalResultAsObject == null)
                {
                    CommonFunctions.WarewolfEvalResult e = EvalResult;
                    _evalResultAsObject = CommonFunctions.evalResultToJsonCompatibleObject(e);
                    if (EvalResult.IsWarewolfAtomListresult && _evalResultAsObject == null)
                    {
                        _evalResultAsObject = new object[] { null };
                    }
                    if (e.IsWarewolfAtomResult)
                    {
                        var x = e as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                        if (x != null && x.Item.IsDataString)
                        {
                            if (((DataStorage.WarewolfAtom.DataString)x.Item).Item == "true")
                            {
                                _evalResultAsObject = true;
                            }
                            else if (((DataStorage.WarewolfAtom.DataString)x.Item).Item == "false")
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
                    return ((CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult)EvalResult).Item.Count;
                }
                return 1;
            }
        }
    }

    public class JsonMappingCompoundTo
    {
        JsonMappingTo Compound { get; set; }
        List<JsonMappingEvaluated> Evaluations { get; set; }

        public JsonMappingCompoundTo(
            IExecutionEnvironment env,
            JsonMappingTo compound)
        {
            var env1 = env;

            Compound = compound;
            Evaluations = new List<JsonMappingEvaluated>();

            if (!IsCompound)
            {
                Evaluations.Add(new JsonMappingEvaluated(env1, compound.SourceName));
            }
            else
            {
                if (FsInteropFunctions.ParseLanguageExpression(Compound.SourceName,0).IsRecordSetNameExpression)
                {
                    Evaluations = new List<JsonMappingEvaluated> { new JsonMappingEvaluated(env1, Compound.SourceName) };
                }
                else
                {
                    // we know this is a comma seperated list of expressions
                    Evaluations =
                        // ReSharper disable MaximumChainedReferences
                        ((LanguageAST.LanguageExpression.ComplexExpression)FsInteropFunctions.ParseLanguageExpression(Compound.SourceName,0))
                            .Item
                            .Where(x => !x.IsWarewolfAtomExpression)
                            .Select(FsInteropFunctions.LanguageExpressionToString)
                            .Select(x =>
                                new JsonMappingEvaluated(env1, x))
                            .ToList();
                }         // ReSharper restore MaximumChainedReferences
            }
        }

        public CommonFunctions.WarewolfEvalResult EvalResult => Evaluations.First().EvalResult;

        bool? _isCompound;
        public bool IsCompound
        {
            get
            {
                if (_isCompound == null)
                {
                    _isCompound = FsInteropFunctions.ParseLanguageExpression(
                        Compound.SourceName,0)
                        .IsComplexExpression || FsInteropFunctions.ParseLanguageExpression(
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

        public string DestinationName => Compound.DestinationName;

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
                    CommonFunctions.evalResultToJsonCompatibleObject(jsonMappingEvaluated.EvalResult))
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

        object GetEvalResult(CommonFunctions.WarewolfEvalResult evalResult, int i)
        {
            if (evalResult.IsWarewolfAtomListresult)
            {
                WarewolfAtomList<DataStorage.WarewolfAtom> lst = ((CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult)evalResult).Item;
                if (i > lst.Count)
                {
                    return null;
                }
                return CommonFunctions.atomToJsonCompatibleObject(lst[i]);
            }
            if (evalResult.IsWarewolfAtomResult)
            {
                if (i == 0)
                {
                    return CommonFunctions.evalResultToJsonCompatibleObject(evalResult);
                }
                return null;
            }
            if (evalResult.IsWarewolfRecordSetResult)
            {
                DataStorage.WarewolfRecordset recset = ((CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult)EvalResult).Item;

                KeyValuePair<string, WarewolfAtomList<DataStorage.WarewolfAtom>>[] data = recset.Data.ToArray();
                var jObjects = new List<JObject>();
                for (int j = 0; j < recset.Count; j++)
                {
                    var a = new JObject();
                    foreach (KeyValuePair<string, WarewolfAtomList<DataStorage.WarewolfAtom>> pair in data)
                    {
                        if (pair.Key != FsInteropFunctions.PositionColumn)
                        {
                            try
                            {
                                a.Add(new JProperty(pair.Key, CommonFunctions.atomToJsonCompatibleObject(pair.Value[j])));
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
            throw new Exception(ErrorResource.InvalidResultTypeFromWarewolfStorage);
        }

        public static string IsValidJsonMappingInput(string sourceName, string destinationName)
        {
            if (string.IsNullOrEmpty(sourceName)) return ErrorResource.SupplySourceName;
            if (string.IsNullOrEmpty(destinationName)) return ErrorResource.SupplyDestinationName;

            return ValidateInput(sourceName);


        }

        public static string ValidateInput(string sourceName)
        {
            try
            {
                var parsed = FsInteropFunctions.ParseLanguageExpression(sourceName,0);
                if (parsed.IsComplexExpression)
                {
                    var complex = (LanguageAST.LanguageExpression.ComplexExpression)parsed;
                    if (complex.Item
                        .Any(x => x.IsRecordSetNameExpression))
                    {
                        return ErrorResource.CannotCommaSeperateRecordset;
                    }
                    if (complex.Item.Count() < 3 ||
                        complex.Item.Count() % 2 != 1 ||
                        // ReSharper disable MaximumChainedReferences
                       !Enumerable.Range(1, complex.Item.Count() - 1)
                           .Where(i => i % 2 == 1)
                           .Select(i =>

                                        FsInteropFunctions.LanguageExpressionToString(
                                            complex.Item.ElementAt(i)
                                            ) == ",")
                           .Aggregate((a, b) => a && b))
                    // ReSharper restore MaximumChainedReferences
                    {
                        return ErrorResource.ExpressionMustBeCommaSeperated;
                    }
                }
                else if (!parsed.IsRecordSetNameExpression &&
                        !parsed.IsRecordSetExpression &&
                        !parsed.IsScalarExpression &&
                        !parsed.IsWarewolfAtomExpression)
                {
                    return ErrorResource.OnlyScalarRecordsetCommaSeperated;
                }
            }
            catch (Exception)
            {
                return ErrorResource.UnableToParseSourceName;
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
                    FsInteropFunctions.ParseLanguageExpression(x.Simple.SourceName,0).IsRecordSetNameExpression);
            }
        }
    }
}
