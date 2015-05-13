
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;
using Warewolf.Storage;

namespace Dev2.TO
{
    public class JsonMappingEvaluated
    {
        IExecutionEnvironment _env;
        public JsonMappingTo Simple { get; set; }
        object _evalResultAsObject = null;
        WarewolfDataEvaluationCommon.WarewolfEvalResult _evalResult = null;

        public JsonMappingEvaluated(
            IExecutionEnvironment env,
            string sourceName)
        {
            this._env = env;
            this.Simple = new JsonMappingTo
            {
                SourceName = sourceName,
                DestinationName = CalculateDestinationNameFromSourceName(sourceName)
            };
        }

        private string CalculateDestinationNameFromSourceName(string sourceName)
        {
            var parsed = WarewolfDataEvaluationCommon.ParseLanguageExpression(sourceName);
            if (parsed.IsScalarExpression)
                return (parsed as LanguageAST.LanguageExpression.ScalarExpression).Item;
            if (parsed.IsRecordSetExpression)
                return (parsed as LanguageAST.LanguageExpression.RecordSetExpression).Item.Column;
            if (parsed.IsRecordSetNameExpression)
                return (parsed as LanguageAST.LanguageExpression.RecordSetNameExpression).Item.Name;
            return string.Empty;
        }

        public WarewolfDataEvaluationCommon.WarewolfEvalResult EvalResult
        {
            get
            {
                if (_evalResult == null)
                    _evalResult = _env.Eval(
                         Simple.SourceName);
                return _evalResult;
            }
        }

        public object EvalResultAsObject
        {
            get
            {
                if (_evalResultAsObject == null)
                {
                    var e = this.EvalResult;
                    _evalResultAsObject = WarewolfDataEvaluationCommon.EvalResultToJsonCompatibleObject(e);
                    if (this.EvalResult.IsWarewolfAtomListresult && _evalResultAsObject == null)
                        _evalResultAsObject = new object[] { null };
                    if (e.IsWarewolfAtomResult)
                    {
                        var x = e as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                        if (x != null && x.Item.IsDataString)
                        {
                            if ((x.Item as DataASTMutable.WarewolfAtom.DataString).Item == "true") _evalResultAsObject = (bool)true;
                            else if ((x.Item as DataASTMutable.WarewolfAtom.DataString).Item == "false") _evalResultAsObject = (bool)false;
                        }
                    }
                }
                return _evalResultAsObject;
            }
        }

        public object EvalResultIndexed(int index)
        {
            if (this.EvalResult.IsWarewolfAtomResult)
                return index > 0 ? null :
                    (this.EvalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult).Item;
            if (this.EvalResult.IsWarewolfAtomListresult)
                return index >= this.Count ? null :
                    (this.EvalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult).Item.ElementAt(index);
            if (this.EvalResult.IsWarewolfRecordSetResult)
                return index >= this.Count ? null :
                    (this.EvalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfRecordSetResult).Item.Data;
            return null;
        }

        public int Count
        {
            get
            {
                if (this.EvalResult.IsWarewolfAtomResult) return 1;
                if (this.EvalResult.IsWarewolfAtomListresult)
                    return (this.EvalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult).Item.Count;
                if (this.EvalResult.IsWarewolfRecordSetResult)
                    return (this.EvalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfRecordSetResult).Item.Data.Count;
                return 0;
            }
        }
    }

    public class JsonMappingCompoundTo
    {
        IExecutionEnvironment _env;
        public JsonMappingTo Compound { get; set; }
        public List<JsonMappingEvaluated> Evaluations { get; set; }

        public JsonMappingCompoundTo(
            IExecutionEnvironment env,
            JsonMappingTo compound)
        {
            this._env = env;

            Compound = compound;
            this.Evaluations = new List<JsonMappingEvaluated>();

            if (!this.IsCompound)
            {
                this.Evaluations.Add(new JsonMappingEvaluated(
                    env: this._env,
sourceName: compound.SourceName));
            }
            else
            {
                // we know this is a comma seperated list of expressions
                this.Evaluations =
                    (WarewolfDataEvaluationCommon.ParseLanguageExpression(this.Compound.SourceName)
                as LanguageAST.LanguageExpression.ComplexExpression)
                .Item
                .Where(x => !x.IsWarewolfAtomAtomExpression)
                .Select(x =>
                    WarewolfDataEvaluationCommon.LanguageExpressionToString(x))
                .Select(x =>
                    new JsonMappingEvaluated(
                        env: this._env,
                        sourceName: x))
                .ToList();
            }
        }

        public WarewolfDataEvaluationCommon.WarewolfEvalResult EvalResult { get { return this.Evaluations.First().EvalResult; } }

        bool? _isCompound = null;
        public bool IsCompound
        {
            get
            {
                if (_isCompound == null)
                    _isCompound = WarewolfDataEvaluationCommon.ParseLanguageExpression(
                        Compound.SourceName)
                        .IsComplexExpression;
                return (bool)_isCompound;
            }
        }

        public int MaxCount
        {
            get
            {
                return this.Evaluations.Select(x => x.Count).Max();
            }
        }

        public string DestinationName
        {
            get
            {
                return this.Compound.DestinationName;
            }
        }

        public object EvaluatedResultIndexed(int i)
        {
            return i < this.MaxCount ?

                this.Evaluations.First().EvalResultAsObject :
                this.Evaluations.First().EvalResult.IsWarewolfAtomListresult ? new object[] { null } : null;
        }

    }
}
