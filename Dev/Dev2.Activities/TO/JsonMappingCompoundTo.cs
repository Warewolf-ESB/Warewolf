
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

namespace Dev2.TO
{
    public class JsonMappingEvaluated
    {
        public JsonMappingTo Simple { get; }
        WarewolfDataEvaluationCommon.WarewolfEvalResult _evalResult = null;

        public JsonMappingEvaluated(string sourceName)
        {
            this.Simple = new JsonMappingTo
            {
                SourceName = sourceName,
                DestinationName = CalculateDestinationNameFromSourceName(sourceName)
            };
        }

        private string CalculateDestinationNameFromSourceName(string sourceName)
        {
            var parsed = WarewolfDataEvaluationCommon.ParseLanguageExpression(this.Simple.SourceName);
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
                    _evalResult = WarewolfDataEvaluationCommon.Eval()Simple.SourceName);
                return _evalResult;
            }
        }

        public int Count
        {
            get
            {
                if (this.EvalResult.IsWarewolfAtomResult) return 1;
                if (this.EvalResult.IsWarewolfAtomListresult)
                    return (this.EvalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult).Item.Count;
                if (this.EvalResult.IsWarewolfRecordSetResult)
                    return (this.EvalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfRecordSetResult).Item.Count;
            }
        }
    }

    public class JsonMappingCompoundTo
    {
        public JsonMappingTo Compound { get; set; }
        public List<JsonMappingEvaluated> Evaluations { get; set; }

        public JsonMappingCompoundTo(JsonMappingTo compound)
        {
            Compound = compound;
            this.Evaluations = new List<JsonMappingEvaluated>();

            if (!this.IsCompound)
            {
                this.Evaluations.Add(new JsonMappingEvaluated( )compound.SourceName));
            }
            else
            {
                // we assume this is a comma seperated list of expressions
                this.Evaluations =
                    (WarewolfDataEvaluationCommon.ParseLanguageExpression(this.Compound.SourceName)
                as LanguageAST.LanguageExpression.ComplexExpression)
                .Item
                .Where(x => !x.IsWarewolfAtomAtomExpression)
                .Select(x => 
                    WarewolfDataEvaluationCommon.LanguageExpressionToString(x))
                .Select(x => 
                    new JsonMappingEvaluated(x))
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
                        Compound.SourceName
                        ).IsComplexExpression;
                return _isCompound;
            }
        }

        public int MaxCount
        {
            get
            {
                return this.Evaluations.Select(x => x.Count).Max();
            }
        }
    }
}
