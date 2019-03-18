#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;

namespace Dev2.TO
{
    public class JsonMappingEvaluated
    {
        readonly IExecutionEnvironment _env;
        
        public JsonMappingTo Simple { get; set; }
        
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
            var parsed = FsInteropFunctions.ParseLanguageExpression(sourceName,0);
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

        public object GetEvalResultAsObject()
        {
            if (_evalResultAsObject == null)
            {
                var e = EvalResult;
                _evalResultAsObject = CommonFunctions.evalResultToJsonCompatibleObject(e);
                if (EvalResult.IsWarewolfAtomListresult && _evalResultAsObject == null)
                {
                    _evalResultAsObject = new object[] { null };
                }
                if (e.IsWarewolfAtomResult && e is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult x && x.Item.IsDataString)
                {
                    SetEvalResultAsObject(x);
                }

            }
            return _evalResultAsObject;
        }

        void SetEvalResultAsObject(CommonFunctions.WarewolfEvalResult.WarewolfAtomResult x)
        {
            if (((DataStorage.WarewolfAtom.DataString)x.Item).Item == "true")
            {
                _evalResultAsObject = true;
            }
            else
            {
                if (((DataStorage.WarewolfAtom.DataString)x.Item).Item == "false")
                {
                    _evalResultAsObject = false;
                }
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
                Evaluations = FsInteropFunctions.ParseLanguageExpression(Compound.SourceName, 0).IsRecordSetNameExpression ? new List<JsonMappingEvaluated> { new JsonMappingEvaluated(env1, Compound.SourceName) } : ((LanguageAST.LanguageExpression.ComplexExpression)FsInteropFunctions.ParseLanguageExpression(Compound.SourceName, 0))
                            .Item
                            .Where(x => !x.IsWarewolfAtomExpression)
                            .Select(FsInteropFunctions.LanguageExpressionToString)
                            .Select(x =>
                                new JsonMappingEvaluated(env1, x))
                            .ToList();
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

        public int MaxCount => Evaluations.Select(x => x.Count).Max();

        public string DestinationName => Compound.DestinationName;

        public object EvaluatedResultIndexed(int i) => i < MaxCount ?
                Evaluations.First().GetEvalResultAsObject() :
                Evaluations.First().EvalResult.IsWarewolfAtomListresult ? new object[] { null } : null;

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
            var jsonMappingEvaluated = Evaluations.First();
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
                var lst = ((CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult)evalResult).Item;
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
                var recset = ((CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult)EvalResult).Item;

                var data = recset.Data.ToArray();
                var jObjects = new List<JObject>();
                for (int j = 0; j < recset.Count; j++)
                {
                    var a = new JObject();
                    foreach (KeyValuePair<string, WarewolfAtomList<DataStorage.WarewolfAtom>> pair in data)
                    {
                        TryAddPairKey(j, a, pair);
                    }
                    jObjects.Add(a);
                }
                return jObjects;
            }
            throw new Exception(ErrorResource.InvalidResultTypeFromWarewolfStorage);
        }

        private static void TryAddPairKey(int j, JObject a, KeyValuePair<string, WarewolfAtomList<DataStorage.WarewolfAtom>> pair)
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

        public static string IsValidJsonMappingInput(string sourceName, string destinationName)
        {
            if (string.IsNullOrEmpty(sourceName))
            {
                return ErrorResource.SupplySourceName;
            }

            if (string.IsNullOrEmpty(destinationName))
            {
                return ErrorResource.SupplyDestinationName;
            }

            return ValidateInput(sourceName);


        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        public static string ValidateInput(string sourceName)
#pragma warning restore S1541 // Methods and properties should not be too complex
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

                       !Enumerable.Range(1, complex.Item.Count() - 1)
                           .Where(i => i % 2 == 1)
                           .Select(i =>

                                        FsInteropFunctions.LanguageExpressionToString(
                                            complex.Item.ElementAt(i)
                                            ) == ",")
                           .Aggregate((a, b) => a && b))

                    {
                        return ErrorResource.ExpressionMustBeCommaSeperated;
                    }
                }
                else
                {
                    if (!parsed.IsRecordSetNameExpression &&
                      !parsed.IsRecordSetExpression &&
                      !parsed.IsScalarExpression &&
                      !parsed.IsWarewolfAtomExpression)
                    {
                        return ErrorResource.OnlyScalarRecordsetCommaSeperated;
                    }
                }
            }
            catch (Exception)
            {
                return ErrorResource.UnableToParseSourceName;
            }
            return null;
        }

        public bool HasRecordSetInCompound => IsCompound &&
                    Evaluations.Any(x =>
                    FsInteropFunctions.ParseLanguageExpression(x.Simple.SourceName, 0).IsRecordSetNameExpression);
    }
}
