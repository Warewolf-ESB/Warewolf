using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Data.Audit;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.Compilers
{
    /// <summary>
    /// Used to configure and evaluate a Dev2 data language piece.
    /// </summary>
    public class EvaluateRuleSet
    {
        #region Properties

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>
        /// The expression.
        /// </value>
        public string Expression { get; set; }

        /// <summary>
        /// Gets or sets the binary data list.
        /// </summary>
        /// <value>
        /// The binary data list.
        /// </value>
        public IBinaryDataList BinaryDataList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [evaluate to root only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [evaluate to root only]; otherwise, <c>false</c>.
        /// </value>
        public bool EvaluateToRootOnly { get; set; }

        /// <summary>
        /// Gets or sets the compiled expression.
        /// </summary>
        /// <value>
        /// The compiled expression.
        /// </value>
        public string CompiledExpression { get; set; }

        /// <summary>
        /// Gets the errors.
        /// </summary>
        /// <value>
        /// The errors.
        /// </value>
        public ErrorResultTO Errors { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is debug.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is debug; otherwise, <c>false</c>.
        /// </value>
        public bool IsDebug { get; set; }

        #endregion


        // build result name space ;)
        private readonly string _ns = GlobalConstants.NullEntryNamespace + Guid.NewGuid();
        private IBinaryDataListEntry _result;
        private readonly IDictionary<int, IIntellisenseResult> _internalMap = new Dictionary<int, IIntellisenseResult>();
        private readonly IDictionary<IIntellisenseResult, IBinaryDataListEntry> _internalKeyMap = new Dictionary<IIntellisenseResult, IBinaryDataListEntry>();

        public EvaluateRuleSet()
        {
            Errors = new ErrorResultTO();
        }

        public EvaluateRuleSet(EvaluateRuleSet prevIter)
        {
            Errors = new ErrorResultTO();
            Errors.MergeErrors(prevIter.Errors);
            _result = prevIter._result;
            _ns = prevIter._ns;
            IsDebug = prevIter.IsDebug;
            EvaluateToRootOnly = prevIter.EvaluateToRootOnly;

        }

        /// <summary>
        /// Fetches the intellisense parts.
        /// </summary>
        /// <returns></returns>
        public IList<IDev2DataLanguageIntellisensePart> FetchIntellisenseParts()
        {
            if(BinaryDataList == null)
            {
                return null;
            }

            return BinaryDataList.FetchIntellisenseParts();
        }

        /// <summary>
        /// Processes the error tokens.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        public void ProcessErrorTokens(IEnumerable<IIntellisenseResult> tokens)
        {
            if(tokens == null)
            {
                return;
            }

            foreach(var err in tokens)
            {
                Errors.AddError(err.Message);
            }
        }

        /// <summary>
        /// Fetches the tokens for evaluation.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IIntellisenseResult> FetchTokensForEvaluation()
        {
            return _internalMap.Values;
        }

        /// <summary>
        /// Adds the bound item.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="binding">The binding.</param>
        public void AddBoundItem(IIntellisenseResult token, IBinaryDataListEntry binding)
        {
            if(binding == null)
            {
                Errors.AddError("Could not evaluate { " + token.Option.DisplayValue + " }");

                return;
            }

            _internalKeyMap[token] = binding;
        }

        /// <summary>
        /// Binds the compiled expression.
        /// </summary>
        /// <returns></returns>
        public IBinaryDataListEntry BindCompiledExpression()
        {

            // very short circuit if no items ;)
            if(_internalKeyMap.Keys.Count == 0)
            {
                CompiledExpression = null;
                return null;
            }

            // short circuit the long eval for mix mode data ;)
            if(_internalMap.Keys.Count <= 1 && FetchEvaluationIterationCount(Expression) == 1 && CompiledExpression.Length == 3)
            {
                return _internalKeyMap.Values.FirstOrDefault();
            }

            var replaceValue = string.Empty;

            // Right now we assume there are not ;)
            foreach(var idx in _internalMap.Keys)
            {
                var token = BuildSubToken(idx);
                var otherKey = _internalMap[idx];
                IBinaryDataListEntry value;
                if(_internalKeyMap.TryGetValue(otherKey, out value))
                {
                    if(value != null)
                    {
                        if(!value.IsRecordset)
                        {
                            var scalar = value.FetchScalar();
                            if(scalar != null)
                            {
                                if(_result == null)
                                {
                                    var toReplace = scalar.TheValue;
                                    CompiledExpression = CompiledExpression.Replace(token, toReplace);
                                }
                                else
                                {
                                    var itr = _result.FetchRecordsetIndexes();
                                    var replaceVal = scalar.TheValue;

                                    while(itr.HasMore())
                                    {
                                        var val = itr.FetchNextIndex();

                                        // Fetch the next value from result ;)
                                        string error;
                                        var template = _result.TryFetchRecordsetColumnAtIndex(GlobalConstants.EvaluationRsField, val, out error).TheValue;
                                        Errors.AddError(error);

                                        template = template.Replace(token, replaceVal);
                                        _result.TryPutRecordItemAtIndex(new BinaryDataListItem(template, _ns, GlobalConstants.EvaluationRsField, val), val, out error);
                                        Errors.AddError(error);
                                    }

                                    CompiledExpression = CompiledExpression.Replace(token, replaceVal);
                                }
                            }
                        }
                        else
                        {
                            string error;
                            // build up the complex expression result - this means debug will be out of sync of complex expressions ;)
                            if(_result == null)
                            {
                                IList<Dev2Column> cols = new List<Dev2Column> { new Dev2Column(GlobalConstants.EvaluationRsField, enDev2ColumnArgumentDirection.Both) };
                                _result = Dev2BinaryDataListFactory.CreateEntry(_ns, string.Empty, cols, BinaryDataList.UID);

                                var max = _internalKeyMap.Values.OrderByDescending(c => c.ItemCollectionSize()).FirstOrDefault();

                                if(max != null)
                                {
                                    var itrToVal = max.ItemCollectionSize();
                                    if(itrToVal == 0)
                                    {
                                        itrToVal = 1;
                                    }

                                    for(int i = 0; i < itrToVal; i++)
                                    {
                                        int idxT = (i + 1);
                                        _result.TryPutRecordItemAtIndex(new BinaryDataListItem(CompiledExpression, _ns, GlobalConstants.EvaluationRsField, idxT), idxT, out error);
                                        Errors.AddError(error);
                                    }
                                }

                                if(IsDebug)
                                {
                                    // attach audit object for debug ;)
                                    _result.ComplexExpressionAuditor = new ComplexExpressionAuditor();
                                }
                            }

                            var idxItr = value.FetchRecordsetIndexes();
                            int expIdx = 1;

                            // we need to treat this as a scalar ;)
                            if(idxItr.Count == 1)
                            {
                                int curVal = idxItr.FetchNextIndex();
                                int amt = _result.ItemCollectionSize();
                                // ensure we always iterate once ;)
                                if(amt == 0)
                                {
                                    amt = 1;
                                }

                                idxItr = new LoopedIndexIterator(curVal, amt);
                            }

                            // else iterate across the recordset cuz it had a star ;)
                            while(idxItr.HasMore())
                            {
                                var val = idxItr.FetchNextIndex();

                                // Fetch the next value from result ;)
                                var template = _result.TryFetchRecordsetColumnAtIndex(GlobalConstants.EvaluationRsField,
                                                                            expIdx, out error).TheValue;
                                Errors.AddError(error);

                                var binaryValue = value.TryFetchIndexedRecordsetUpsertPayload(val, out error);
                                Errors.AddError(error);

                                // now bind this result row with the correct data list data ;)
                                if(binaryValue != null)
                                {
                                    var preTemplate = template;
                                    var toReplace = binaryValue.TheValue;
                                    template = template.Replace(token, toReplace);

                                    // In cases when [[[{0}]] is the result, we need to inject the template value
                                    // In cases when [[rec({0}).a]] we need to replace the template pattern ;)
                                    var tmp = CompiledExpression.Replace("[", "").Replace("]", "").Replace(token, string.Empty);
                                    // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
                                    if(tmp.Length > 0)
                                    // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
                                    {
                                        // we have a [[rec({0}.a]] case ;)
                                        replaceValue = toReplace;
                                    }
                                    else
                                    {
                                        replaceValue = template;
                                    }

                                    _result.TryPutRecordItemAtIndex(new BinaryDataListItem(template, _ns, GlobalConstants.EvaluationRsField, expIdx), expIdx, out error);
                                    Errors.AddError(error);

                                    if(IsDebug)
                                    {
                                        var displayValue = DataListUtil.AddBracketsToValueIfNotExist(binaryValue.DisplayValue);
                                        _result.ComplexExpressionAuditor.AddAuditStep(preTemplate, displayValue, token, idx, template, Expression);
                                        _result.ComplexExpressionAuditor.SetMaxIndex(expIdx);
                                    }
                                }

                                expIdx++; // inc result index ;)
                            }

                            replaceValue = DataListUtil.RemoveLanguageBrackets(replaceValue);
                            CompiledExpression = CompiledExpression.Replace(token, replaceValue);
                        }
                    }
                    else
                    {
                        CompiledExpression = CompiledExpression.Replace(token, string.Empty);
                    }
                }
            }

            return _result;
        }

        /// <summary>
        /// Compiles the expression.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        public void CompileExpression(IEnumerable<IIntellisenseResult> tokens)
        {
            // first pass binding ;)
            // ReSharper disable PossibleMultipleEnumeration
            if(!string.IsNullOrEmpty(Expression) && tokens != null && tokens.Any())
            // ReSharper restore PossibleMultipleEnumeration
            {
                int subVar = 0;
                var compiledExpression = Expression;
                HashSet<string> usedTokens = new HashSet<string>();

                // ReSharper disable PossibleMultipleEnumeration
                foreach(var token in tokens)
                // ReSharper restore PossibleMultipleEnumeration
                {
                    var subToken = token.Option.DisplayValue;
                    // we may have dups avoid them ;)
                    if(!usedTokens.Contains(subToken))
                    {
                        if(compiledExpression.IndexOf(subToken, StringComparison.Ordinal) >= 0)
                        {
                            usedTokens.Add(subToken);
                            compiledExpression = compiledExpression.Replace(subToken, BuildSubToken(subVar));
                            _internalMap[subVar] = token;
                            subVar++;
                        }
                        else
                        {
                            Errors.AddError("Could not locate token { " + subToken + " }");
                        }
                    }
                }

                if(subVar == 0)
                {
                    CompiledExpression = null;
                    Errors.ClearErrors(); // clear for relevant errors ;)
                }
                else
                {
                    // save the compiled expression ;)
                    CompiledExpression = compiledExpression;
                }

            }
            else
            {
                Errors.AddError("Invalid Data : Either empty expression or empty token list. Please check that your variable list does not contain errors.");
                CompiledExpression = null;
            }

            // multi-phase binding ;)
            if(tokens != null && _result != null && CompiledExpression != null)
            {
                int subVar = 0;

                var idxItr = _result.FetchRecordsetIndexes();

                // foreach result to far ;)
                while(idxItr.HasMore())
                {
                    var val = idxItr.FetchNextIndex();

                    // Fetch the next value from result ;)
                    string error;
                    var compiledExpression = _result.TryFetchRecordsetColumnAtIndex(GlobalConstants.EvaluationRsField, val, out error).TheValue;
                    Errors.AddError(error);

                    HashSet<string> usedTokens = new HashSet<string>();

                    // now process each token ;)
                    // ReSharper disable PossibleMultipleEnumeration
                    foreach(var token in tokens)
                    // ReSharper restore PossibleMultipleEnumeration
                    {
                        var subToken = token.Option.DisplayValue;
                        // we may have dups avoid them ;)
                        if(!usedTokens.Contains(subToken))
                        {
                            if(compiledExpression.IndexOf(subToken, StringComparison.Ordinal) >= 0)
                            {
                                usedTokens.Add(subToken);
                                compiledExpression = compiledExpression.Replace(subToken, BuildSubToken(subVar));
                                _internalMap[subVar] = token;
                                subVar++;
                            }
                            else
                            {
                                Errors.AddError("Could not locate token { " + subToken + " }");
                            }
                        }
                    }

                    _result.TryPutRecordItemAtIndex(new BinaryDataListItem(compiledExpression, _ns, GlobalConstants.EvaluationRsField, val), val, out error);
                    Errors.AddError(error);

                }
            }
        }

        /// <summary>
        /// Builds the sub token.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        private string BuildSubToken(int idx)
        {
            return "{" + idx + "}";
        }

        /// <summary>
        /// Fetches the evaluation iteration count.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        private int FetchEvaluationIterationCount(string expression)
        {
            int fetchEvaluationIterationCount = 1;

            char[] parts = expression.ToCharArray();

            int hits = 0;
            int pos = 0;
            while(pos < parts.Length)
            {
                if(parts[pos] == GlobalConstants.EvaluationToken)
                {
                    hits++;
                }
                pos++;
            }

            if(hits > 0)
            {
                fetchEvaluationIterationCount = (hits / 2);
            }

            return fetchEvaluationIterationCount;
        }
    }
}
