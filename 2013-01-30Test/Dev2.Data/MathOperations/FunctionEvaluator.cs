using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Calculations.Engine;
using Infragistics.Calculations;
using Infragistics.Calculations.CalcManager;
using System.Parsing.Intellisense;
using System.Parsing;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.MathOperations {
    public class FunctionEvaluator : IFunctionEvaluator {

        #region Properties

        #endregion Properties

        #region Private Members

        private static IDev2DataLanguageParser _parser = DataListFactory.CreateLanguageParser(); 
        private static IDataListCompiler _compiler = DataListFactory.CreateDataListCompiler();
        private static string _mathsFnDataList = string.Empty;
        private static IDev2CalculationManager _manager;

        //private static string _emptyADL = "<ADL></ADL>";

        #endregion Private Members

        #region Ctor

        internal FunctionEvaluator(string expression) {
            // The evaluation Manager is instantiated here.
            // This will in-turn create an expression which will parse out the expression 
            // to determine the execution of the operation path the expression will take
        }

        internal FunctionEvaluator() {
            _manager = new Dev2CalculationManager();
        }

        #endregion Ctor

        #region Public Methods
        /// <summary>
        /// Entry point for the calculate activity
        /// Will create a Function Repository call the intellisense parser to aid in creating the evaluation
        /// </summary>
        /// <param name="expressionTO"></param>
        /// <returns></returns>
        /// public IList<string> EvaluateFunction(IEvaluationFunction expressionTO, string dataList, string shape)
        public string EvaluateFunction(IEvaluationFunction expressionTO, Guid curDLID, out ErrorResultTO errors) {
            string expression = expressionTO.Function;

            SyntaxTreeBuilder builder = new SyntaxTreeBuilder();
            Node[] nodes = builder.Build(expression);
            errors = new ErrorResultTO();
            string result = string.Empty;

            if (builder.EventLog.HasEventLogs) {
                IList<string> err = EvaluateEventLogs(builder.EventLog.GetEventLogs(), expression);

                foreach (string e in err) {
                    errors.AddError(e);
                }
            } else {

                List<Node> allNodes = new List<Node>();
                nodes[0].CollectNodes(allNodes);
                
                //IRecordsetScopingObject rso = null;
                IterationNodeValueSource valueSource = null;
                bool startedIteration = false;
                bool isIteration = false;
                bool pendingIterationRecordSet = false;
                int maxRecords = -1;
                int currentRecord = 0;

                do {
                    if (startedIteration)
                        for (int i = 0; i < allNodes.Count; i++)
                            allNodes[i].EvaluatedValue = null;

                    for (int i = allNodes.Count - 1; i >= 0; i--) {
                        if (allNodes[i] is IterationNode) {
                            IterationNode refNode = allNodes[i] as IterationNode;
                            if (valueSource == null) valueSource = new IterationNodeValueSource(1);
                            refNode.ValueSource = valueSource;
                            pendingIterationRecordSet = true;
                            isIteration = true;
                        } else if (allNodes[i] is DatalistRecordSetNode) {
                            DatalistRecordSetNode refNode = allNodes[i] as DatalistRecordSetNode;

                            if (refNode.Parameter != null) {
                                if ((refNode.Parameter.Items != null && refNode.Parameter.Items.Length != 0) || refNode.Parameter.Statement != null)
                                    refNode.Parameter.EvaluatedValue = InternalEval(refNode.Parameter.GetEvaluatedValue());
                            }


                            // this way we fetch the correct field with the data...
                            IBinaryDataListEntry e = _compiler.Evaluate(curDLID, enActionType.User, refNode.GetRepresentationForEvaluation(), false, out errors);
                            string error = string.Empty;
                            refNode.EvaluatedValue = e.TryFetchLastIndexedRecordsetUpsertPayload(out error).TheValue;;

                            if (pendingIterationRecordSet) {
                                pendingIterationRecordSet = false;

                                if (refNode.NestedIdentifier != null) {
                                    errors.AddError("An error occured while parsing { " + expression + " } Iteration operator can not be used with nested recordset identifiers.");
                                    break;
                                }

                                string evaluateRecordLeft = refNode.GetRepresentationForEvaluation();
                                evaluateRecordLeft = evaluateRecordLeft.Substring(2, evaluateRecordLeft.IndexOf('(') - 2);

                                int totalRecords = 0;
                                IBinaryDataList bdl = _compiler.FetchBinaryDataList(curDLID, out errors);
                                IBinaryDataListEntry entry;
                                if (bdl.TryGetEntry(evaluateRecordLeft, out entry, out error)) {
                                    totalRecords = entry.FetchLastRecordsetIndex();
                                }

                                maxRecords = Math.Max(totalRecords, maxRecords);

                            }
           
                            
                        } else if (allNodes[i] is DatalistReferenceNode) {
                            DatalistReferenceNode refNode = allNodes[i] as DatalistReferenceNode;
                            IBinaryDataListEntry entry = _compiler.Evaluate(curDLID, enActionType.User, refNode.GetRepresentationForEvaluation(), false, out errors);
                            string error = string.Empty;

                            if (entry.IsRecordset)
                            {
                                refNode.EvaluatedValue = entry.TryFetchLastIndexedRecordsetUpsertPayload(out error).TheValue;
                                var testParse = new double();
                                if (!Double.TryParse(refNode.EvaluatedValue, out testParse)) refNode.EvaluatedValue = String.Concat("\"", refNode.EvaluatedValue, "\"");//Bug 6438
                            }
                            else
                            {
                                refNode.EvaluatedValue = entry.FetchScalar().TheValue;
                                var testParse = new double();
                                if (!Double.TryParse(refNode.EvaluatedValue, out testParse)) refNode.EvaluatedValue = String.Concat("\"", refNode.EvaluatedValue, "\"");//Bug 6438
                            }
                            // Old method
                            //refNode.EvaluatedValue = _compiler.EvaluateFromDataList(refNode.GetRepresentationForEvaluation(), shape, dataList, _emptyADL);
                        } else if (allNodes[i] is BinaryOperatorNode && allNodes[i].Identifier.Start.Definition == TokenKind.Colon) {
                            BinaryOperatorNode biNode = (BinaryOperatorNode)allNodes[i];

                            if (!(biNode.Left is DatalistRecordSetFieldNode)) {
                                errors.AddError("An error occured while parsing { " + expression + " } Range operator can only be used with record set fields.");
                                break;
                            }

                            if (!(biNode.Right is DatalistRecordSetFieldNode)) {
                                errors.AddError("An error occured while parsing { " + expression + " } Range operator can only be used with record set fields.");
                                break;
                            }

                            DatalistRecordSetFieldNode fieldLeft = (DatalistRecordSetFieldNode)biNode.Left;
                            DatalistRecordSetFieldNode fieldRight = (DatalistRecordSetFieldNode)biNode.Right;

                            string evaluateFieldLeft = (fieldLeft.Field != null) ? fieldLeft.Field.GetEvaluatedValue() : fieldLeft.Identifier.Content;
                            string evaluateFieldRight = (fieldRight.Field != null) ? fieldRight.Field.GetEvaluatedValue() : fieldRight.Identifier.Content;

                            if (!String.Equals(evaluateFieldLeft, evaluateFieldRight, StringComparison.Ordinal)) {
                                errors.AddError("An error occured while parsing { " + expression + " } Range operator must be used with the same record set fields.");
                                break;
                            }

                            string evaluateRecordLeft = fieldLeft.RecordSet.GetRepresentationForEvaluation();
                            evaluateRecordLeft = evaluateRecordLeft.Substring(2, evaluateRecordLeft.IndexOf('(') - 2);
                            string evaluateRecordRight = fieldRight.RecordSet.GetRepresentationForEvaluation();
                            evaluateRecordRight = evaluateRecordRight.Substring(2, evaluateRecordRight.IndexOf('(') - 2);

                            if (!String.Equals(evaluateRecordLeft, evaluateRecordRight, StringComparison.Ordinal)) {
                                errors.AddError("An error occured while parsing { " + expression + " } Range operator must be used with the same record sets.");
                                break;
                            }

                            int totalRecords = 0;
                            IBinaryDataList bdl = _compiler.FetchBinaryDataList(curDLID, out errors);
                            string error = string.Empty;
                            IBinaryDataListEntry entry;
                            if (bdl.TryGetEntry(evaluateRecordLeft, out entry, out error)) {
                                totalRecords = entry.FetchLastRecordsetIndex();
                            }

                            string rawParamLeft = fieldLeft.RecordSet.Parameter.GetEvaluatedValue();
                            rawParamLeft = rawParamLeft.Length == 2 ? "" : rawParamLeft.Substring(1, rawParamLeft.Length - 2);
                            string rawParamRight = fieldRight.RecordSet.Parameter.GetEvaluatedValue();
                            rawParamRight = rawParamRight.Length == 2 ? "" : rawParamRight.Substring(1, rawParamRight.Length - 2);

                            int startIndex = 0;
                            int endIndex = 0;

                            if (!String.IsNullOrEmpty(rawParamLeft)) {
                                if (!Int32.TryParse(rawParamLeft, out startIndex) || startIndex <= 0) {
                                    errors.AddError("An error occured while parsing { " + expression + " } Recordset index must be a positive whole number that is greater than zero.");
                                    break;
                                }
                            } else {
                                startIndex = 1;
                            }

                            if (!String.IsNullOrEmpty(rawParamRight)) {
                                if (!Int32.TryParse(rawParamRight, out endIndex) || endIndex <= 0) {
                                    errors.AddError("An error occured while parsing { " + expression + " } Recordset index must be a positive whole number that is greater than zero.");
                                    break;
                                }

                                if (endIndex > totalRecords) {
                                    errors.AddError("An error occured while parsing { " + expression + " } Recordset end index must be a positive whole number that is less than the number of entries in the recordset.");
                                    break;
                                }
                            } else {
                                endIndex = totalRecords;
                            }

                            endIndex++;

                            StringBuilder rangeBuilder = new StringBuilder();

                            for (int k = startIndex; k < endIndex; k++)
                            {
                                if (k != startIndex)
                                {
                                    rangeBuilder.Append("," + entry.TryFetchRecordsetColumnAtIndex(evaluateFieldLeft, k, out error).TheValue);
                                    if (error != string.Empty)
                                    {
                                        errors.AddError(error);
                                    }
                                }
                                else
                                {
                                    rangeBuilder.Append(entry.TryFetchRecordsetColumnAtIndex(evaluateFieldLeft, k, out error).TheValue);
                                    if (error != string.Empty)
                                    {
                                        errors.AddError(error);
                                    }
                                }
                            }

                            //for (int k = startIndex; k < endIndex; k++) {
                            //   if (k != startIndex) rangeBuilder.Append("," + rangedRecord.FetchFieldAtIndex(k, evaluateFieldLeft));
                            //   else rangeBuilder.Append(rangedRecord.FetchFieldAtIndex(k, evaluateFieldLeft));
                            //}

                            allNodes[i].EvaluatedValue = rangeBuilder.ToString();
                        }
                    }

                    string evaluatedValue = nodes[0].GetEvaluatedValue();
                    result = InternalEval(evaluatedValue);

                    if (startedIteration) {
                        currentRecord = valueSource.Index++;
                    }

                    if (isIteration && !startedIteration) {
                        startedIteration = true;
                        currentRecord = valueSource.Index++;
                    }
                }
                while (startedIteration && currentRecord < maxRecords);
            }

            return result;
        }

        private sealed class IterationNodeValueSource : INodeValueSource {
            private int _index;

            public int Index { get { return _index; } set { _index = value; } }

            public IterationNodeValueSource(int index) {
                _index = index;
            }

            public string GetEvaluatedValue(Node node) {
                return _index.ToString();
            }

            public string GetRepresentationForEvaluation(Node node) {
                return _index.ToString();
            }
        }

        private IList<string> EvaluateEventLogs(ParseEventLogEntry[] parseEventLogEntry, string expression) {
            IList<string> result = new List<string>();
            result.Add("An error occured while parsing { " + expression + " } It appears to be malformed");
            return result;
        }

        /// <summary>
        /// Evaluate the expression according to the operation specified and pass this to the CalculationManager
        //  If something goes wrong during the execution, the error field will be populated and the method will
        //  return false.
        /// </summary>
        /// <param name="expressionTO"></param>
        /// <param name="evalution"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool TryEvaluateFunction(IEvaluationFunction expressionTO, out string evaluation, out string error) {
            bool isSuccessfulEvaluation = false;
            error = string.Empty;
            evaluation = string.Empty;

            if (!(string.IsNullOrEmpty(expressionTO.Function))) {
                try {
                    CalculationValue calcVal = _manager.CalculateFormula(expressionTO.Function);
                    evaluation = calcVal.GetResolvedValue().ToString();
                    isSuccessfulEvaluation = true;
                } catch (Exception ex) {
                    error = ex.Message;
                    isSuccessfulEvaluation = false;
                }
            } else {
                error = "Unable to evaluate empty function";
                isSuccessfulEvaluation = false;
            }
            return isSuccessfulEvaluation;
        }


        /// <summary>
        /// Evaluate the expression according to the operation specified and pass this to the CalculationManager
        //  If something goes wrong during the execution, the error field will be populated and the method will
        //  return false.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="evalution"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool TryEvaluateFunction(string expression, out string evaluation, out string error) {
            bool evaluationState = false;
            error = String.Empty;
            evaluation = String.Empty;
            if (!(String.IsNullOrEmpty(expression))) {

                try {
                    CalculationValue value = _manager.CalculateFormula(expression);
                    if (value.IsError) {
                        error = value.ToErrorValue().Message;
                        evaluationState = false;
                    } else {
                        evaluation = value.GetResolvedValue().ToString();
                        evaluationState = true;
                    }
                } catch (Exception ex) {
                    error = ex.Message;
                    evaluationState = false;
                }
            } else {
                error = "Nothing to Evaluate";
                evaluationState = false;
            }

            return evaluationState;
        }


        /// <summary>
        /// This is to cater for range operations 
        // It expects a List of Type To (either strings or any type of object that is IComparable).
        // And evaluates the whole list against the expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="expression"></param>
        /// <param name="evalution"></param>
        /// <param name="error"></param>
        /// <returns></returns>

        public bool TryEvaluateFunction<T>(List<T> value, string expression, out string evaluation, out string error) where T : IConvertible {
            bool evaluationState = false;
            string evalString = string.Concat(expression, "(");
            evaluation = string.Empty;
            error = string.Empty;
            if (value == null || value.Count == 0) {
                evaluationState = false;
                error = "Cannot evaluate empty value list";
            } else if (!string.IsNullOrEmpty(expression)) {
                foreach (T val in value) {
                    string eval = val.ToString();
                    if (!string.IsNullOrEmpty(eval)) {
                        evalString += string.Concat(eval, ",");
                    }
                }
                evalString = evalString.Remove(evalString.LastIndexOf(","), 1);
                evalString = string.Concat(evalString, ")");
                try {
                    CalculationValue calcValue = _manager.CalculateFormula(evalString);
                    evaluation = calcValue.GetResolvedValue().ToString();
                    evaluationState = true;
                } catch (Exception ex) {
                    error = ex.Message;
                    evaluationState = false;
                }
            } else {
                error = "Nothing to Evaluate";
                evaluationState = false;
            }
            return evaluationState;
        }

        #endregion Public Methods

        #region Statics

        public static bool TryEvaluateAtomicFunction(string expression, out string evaluation, out string error) {
            bool evaluationState = false;
            error = String.Empty;
            evaluation = String.Empty;
            if (!(String.IsNullOrEmpty(expression))) {

                try {
                    CalculationValue value = _manager.CalculateFormula(expression);
                    evaluation = value.GetResolvedValue().ToString();
                    evaluationState = true;
                } catch (Exception ex) {
                    error = ex.Message;
                    evaluationState = false;
                }
            } else {
                error = "Nothing to Evaluate";
                evaluationState = false;
            }

            return evaluationState;
        }

        #endregion Statics

        #region Private Methods
        private string InternalEval(string res) {
            string calcResult = string.Empty;
            string error = string.Empty;
            string result = string.Empty;

            TryEvaluateFunction(res, out calcResult, out error);

            if (error == string.Empty) {
                result = calcResult;
            } else {
                if (error.Contains("<")) error = error.Replace("<", "");
                if (error.Contains(">")) error = error.Replace(">", "");

                result = error;
            }

            return result;
        }

        #endregion Private Methods

    }
}
