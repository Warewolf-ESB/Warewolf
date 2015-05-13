
 /*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Linq;
using Warewolf.Storage;

namespace Dev2.TO
{
    public class JsonMappingEvaluated
    {
        readonly IExecutionEnvironment _env;
        JsonMappingTo Simple { get; set; }
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
            LanguageAST.LanguageExpression parsed = WarewolfDataEvaluationCommon.ParseLanguageExpression(sourceName);
            if(parsed.IsScalarExpression)
            {
                return ((LanguageAST.LanguageExpression.ScalarExpression)parsed).Item;
            }
            if(parsed.IsRecordSetExpression)
            {
                return ((LanguageAST.LanguageExpression.RecordSetExpression)parsed).Item.Column;
            }
            if(parsed.IsRecordSetNameExpression)
            {
                return ((LanguageAST.LanguageExpression.RecordSetNameExpression)parsed).Item.Name;
            }
            return string.Empty;
        }

        public WarewolfDataEvaluationCommon.WarewolfEvalResult EvalResult
        {
            get
            {
                return _evalResult ?? (_evalResult = _env.Eval(
                    Simple.SourceName));
            }
        }

        public object EvalResultAsObject
        {
            get
            {
                if(_evalResultAsObject == null)
                {
                    WarewolfDataEvaluationCommon.WarewolfEvalResult e = EvalResult;
                    _evalResultAsObject = WarewolfDataEvaluationCommon.EvalResultToJsonCompatibleObject(e);
                    if(EvalResult.IsWarewolfAtomListresult && _evalResultAsObject == null)
                    {
                        _evalResultAsObject = new object[] { null };
                    }
                    if(e.IsWarewolfAtomResult)
                    {
                        var x = e as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                        if(x != null && x.Item.IsDataString)
                        {
                            if(((DataASTMutable.WarewolfAtom.DataString)x.Item).Item == "true")
                            {
                                _evalResultAsObject = true;
                            }
                            else if((x.Item as DataASTMutable.WarewolfAtom.DataString).Item == "false")
                            {
                                _evalResultAsObject = false;
                            }
                        }
                    }
                }
                return _evalResultAsObject;
            }
        }

        public object EvalResultIndexed(int index)
        {
            if(EvalResult.IsWarewolfAtomResult)
            {
                return index > 0 ? null :
                    ((WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult)EvalResult).Item;
            }
            if(EvalResult.IsWarewolfAtomListresult)
            {
                return index >= Count ? null :
                    ((WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult)EvalResult).Item.ElementAt(index);
            }
            if(EvalResult.IsWarewolfRecordSetResult)
            {
                return index >= Count ? null :
                    ((WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfRecordSetResult)EvalResult).Item.Data;
            }
            return null;
        }

        public int Count
        {
            get
            {
                if(EvalResult.IsWarewolfAtomResult)
                {
                    return 1;
                }
                if(EvalResult.IsWarewolfAtomListresult)
                {
                    return ((WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult)EvalResult).Item.Count;
                }
                if(EvalResult.IsWarewolfRecordSetResult)
                {
                    return ((WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfRecordSetResult)EvalResult).Item.Data.Count;
                }
                return 0;
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

            if(!IsCompound)
            {
                Evaluations.Add(new JsonMappingEvaluated(_env, compound.SourceName));
            }
            else
            {
                // we know this is a comma seperated list of expressions
                Evaluations =
                    ((LanguageAST.LanguageExpression.ComplexExpression)WarewolfDataEvaluationCommon.ParseLanguageExpression(Compound.SourceName))
                        .Item
                        .Where(x => !x.IsWarewolfAtomAtomExpression)
                        .Select(x =>
                            WarewolfDataEvaluationCommon.LanguageExpressionToString(x))
                        .Select(x =>
                            new JsonMappingEvaluated(_env, x))
                        .ToList();
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
                if(_isCompound == null)
                {
                    _isCompound = WarewolfDataEvaluationCommon.ParseLanguageExpression(
                        Compound.SourceName)
                        .IsComplexExpression;
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
    }
}
