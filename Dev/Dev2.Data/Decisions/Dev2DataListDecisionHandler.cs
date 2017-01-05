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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.Data.Decision
{
    public class Dev2DataListDecisionHandler
    {
        private static Dev2DataListDecisionHandler _inst;
        private static readonly IDictionary<Guid, IExecutionEnvironment> _environments = new ConcurrentDictionary<Guid, IExecutionEnvironment>();
        public static Dev2DataListDecisionHandler Instance => _inst ?? (_inst = new Dev2DataListDecisionHandler());


        public void AddEnvironment(Guid id, IExecutionEnvironment env)
        {
            _environments.Add(id,env);
        }

        public void RemoveEnvironment(Guid id)
        {
            _environments.Remove(id);
        }


        // Guid dlID
        /// <summary>
        /// Executes the decision stack.
        /// </summary>
        /// <param name="decisionDataPayload">The decision data payload.</param>
        /// <param name="oldAmbientData">The old ambient data.</param>
        /// <param name="update"></param>
        /// <returns></returns>
        /// <exception cref="System.Data.InvalidExpressionException">Could not evaluate decision data - No decision function found for [  + typeOf + ]</exception>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public bool ExecuteDecisionStack(string decisionDataPayload, IList<string> oldAmbientData,int update)
        {

            Guid dlId = FetchDataListID(oldAmbientData);
//            if(dlId == GlobalConstants.NullDataListID) throw new InvalidExpressionException("Could not evaluate decision data - no DataList ID sent!");
            string newDecisionData = Dev2DecisionStack.FromVBPersitableModelToJSON(decisionDataPayload);
            var dds = EvaluateRegion(newDecisionData, dlId, update);


              var env =  _environments[dlId];
            if(dds != null)
            {
                if(dlId != GlobalConstants.NullDataListID)
                {
                    try
                    {

                        if(dds.TheStack != null)
                        {

                            for(int i = 0; i < dds.TotalDecisions; i++)
                            {
                                Dev2Decision dd = dds.GetModelItem(i);
                                enDecisionType typeOf = dd.EvaluationFn;

                                // Treat Errors special
                                if(typeOf == enDecisionType.IsError || typeOf == enDecisionType.IsNotError)
                                {
                                    dd.Col1 = env.FetchErrors();
                                }

                                IDecisionOperation op = Dev2DecisionFactory.Instance().FetchDecisionFunction(typeOf);
                                if(op != null)
                                {
                                    try
                                    {
                                        bool result = op.Invoke(dds.GetModelItem(i).FetchColsAsArray());

                                        if(!result && dds.Mode == Dev2DecisionMode.AND)
                                        {
                                            // Naughty stuff, we have a false in AND mode... break
                                            return false;
                                        }

                                        if(result && dds.Mode == Dev2DecisionMode.OR)
                                        {
                                            return true;
                                        }
                                    }
                                    catch(Exception e)
                                    {
                                        // An error, push into the DL
                                       env.AddError(e.Message);

                                        return false;
                                    }
                                }
                                else
                                {
                                    throw new InvalidExpressionException(string.Format(ErrorResource.CouldNotEvaluateDecisionData, typeOf));
                                }
                            }

                            // else we are in AND mode and all have passed ;)
                            if(dds.Mode == Dev2DecisionMode.AND)
                            {
                                return true;
                            }

                            //finally, it must be OR mode with no matches ;(
                            return false;
                        }

                        throw new InvalidExpressionException(ErrorResource.InvalidModelDataSent);
                    }
                    catch
                    {
                        // all hell has broken loose... ;)
                        throw new InvalidExpressionException(ErrorResource.NoModelDataSent);
                    }
                }

                throw new InvalidExpressionException(ErrorResource.NoDataListIDsent);
            }

            throw new InvalidExpressionException(ErrorResource.DataListErrors);
        }

        /// <summary>
        /// Evaluates the region.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="dlId">The dl ID.</param>
        /// <param name="update"></param>
        /// <returns></returns>
        private Dev2DecisionStack EvaluateRegion(string payload, Guid dlId,int update)
        {

            var env =  _environments[dlId];

            if(payload.StartsWith("{\"TheStack\":[{") || payload.StartsWith("{'TheStack':[{"))
            {
                //2013.05.06: Ashley Lewis for PBI 9460 - handle record-sets with stars in their index by resolving them
                var dds = DataListUtil.ConvertFromJsonToModel<Dev2DecisionStack>(new StringBuilder(payload));

                if(dds.TheStack != null)
                {
                    var effectedCols = new[] { false, false, false };
                    //Find decisions that mention record sets with starred indexes
                    var invalidDecisions = new List<Dev2Decision>();
                    for(int i = 0; i < dds.TotalDecisions; i++)
                    {
                        Dev2Decision dd = dds.GetModelItem(i);

                        if(dd.Col1 != null && DataListUtil.GetRecordsetIndexType(dd.Col1) == enRecordsetIndexType.Star)
                        {
                            invalidDecisions.Add(dd);
                            effectedCols[0] = true;
                        }
                        else
                        {
                            var warewolfEvalResult = GetWarewolfEvalResult(env, dd.Col1, update);
                            dd.Col1 = ExecutionEnvironment.WarewolfEvalResultToString(warewolfEvalResult);
                        }

                        if(dd.Col2 != null && DataListUtil.GetRecordsetIndexType(dd.Col2) == enRecordsetIndexType.Star)
                        {
                            if(!effectedCols[0])
                            {
                                invalidDecisions.Add(dd);
                            }
                            effectedCols[1] = true;
                        }
                        else
                        {
                            var warewolfEvalResult = GetWarewolfEvalResult(env, dd.Col2, update);
                            dd.Col2 = ExecutionEnvironment.WarewolfEvalResultToString(warewolfEvalResult);
                        }

                        if(dd.Col3 != null && DataListUtil.GetRecordsetIndexType(dd.Col3) == enRecordsetIndexType.Star)
                        {
                            if(!effectedCols[0] && !effectedCols[1])
                            {
                                invalidDecisions.Add(dd);
                            }
                            effectedCols[2] = true;
                        }
                        else
                        {
                            var warewolfEvalResult = GetWarewolfEvalResult(env, dd.Col3, update);
                            dd.Col3 = ExecutionEnvironment.WarewolfEvalResultToString(warewolfEvalResult);
                        }
                    }
                    //Remove those record sets and replace them with a new decision for each resolved value
                    foreach(Dev2Decision decision in invalidDecisions)
                    {
                        ErrorResultTO errors;
                        dds = ResolveAllRecords(env, dds, decision, effectedCols, out errors, update);
                    }
                }

                return dds;
            }
            return null;
        }

        static CommonFunctions.WarewolfEvalResult GetWarewolfEvalResult(IExecutionEnvironment env, string col,int update)
        {
            var warewolfEvalResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing);
            try
            {
                warewolfEvalResult = env.Eval(col, update);
            }
            catch(NullValueInVariableException)
            {
                //This is allow for decisions.
            }
            catch(Exception err)
            {
                env.Errors.Add(err.Message);
            }
            
            return warewolfEvalResult;
        }

        /// <summary>
        /// Fetches the stack value.
        /// </summary>
        /// <param name="stack">The stack.</param>
        /// <param name="stackIndex">Index of the stack.</param>
        /// <param name="columnIndex">Index of the column.</param>
        /// <returns></returns>
        private string FetchStackValue(Dev2DecisionStack stack, int stackIndex, int columnIndex)
        {
            // if out of bounds return an empty string ;)
            if(stackIndex >= stack.TheStack.Count)
            {
                return string.Empty;
            }

            if(columnIndex == 1)
            {
                return stack.TheStack[stackIndex].Col1;
            }
            if(columnIndex == 2)
            {
                return stack.TheStack[stackIndex].Col2;
            }

            return string.Empty;
        }

        Dev2DecisionStack ResolveAllRecords(IExecutionEnvironment env, Dev2DecisionStack stack, Dev2Decision decision, bool[] effectedCols, out ErrorResultTO errors,int update)
        {
            if(effectedCols == null)
            {
                throw new ArgumentNullException("effectedCols");
            }
            int stackIndex = stack.TheStack.IndexOf(decision);
            stack.TheStack.Remove(decision);
            errors = new ErrorResultTO();
            if(effectedCols[0])
            {
                var data = env.EvalAsListOfStrings(decision.Col1, update);
               
                int reStackIndex = stackIndex;

                foreach(var item in data)
                {


                    var newDecision = new Dev2Decision { Col1 = item, Col2 = decision.Col2, Col3 = decision.Col3, EvaluationFn = decision.EvaluationFn };
                    stack.TheStack.Insert(reStackIndex++, newDecision);
                }
            }
            if(effectedCols[1])
            {
                var data = env.EvalAsListOfStrings(decision.Col2, update);
                int reStackIndex = stackIndex;

                 foreach(var item in data)
                {
                    var newDecision = new Dev2Decision { Col1 = FetchStackValue(stack, reStackIndex, 1), Col2 = item, Col3 = decision.Col3, EvaluationFn = decision.EvaluationFn };
                    if(effectedCols[0])
                    {
                        // ensure we have the correct indexing ;)
                        if(reStackIndex < stack.TheStack.Count)
                        {
                            stack.TheStack[reStackIndex++] = newDecision;
                        }
                        else
                        {
                            stack.TheStack.Insert(reStackIndex++, newDecision);
                        }
                    }
                    else
                    {
                        stack.TheStack.Insert(reStackIndex++, newDecision);
                    }
                }
            }
            if(effectedCols[2])
            {
                var data = env.EvalAsListOfStrings(decision.Col3, update);
                int reStackIndex = stackIndex;

                foreach (var item in data)
                {
                    var newDecision = new Dev2Decision { Col1 = FetchStackValue(stack, reStackIndex, 1), Col2 = FetchStackValue(stack, reStackIndex, 2), Col3 = item, EvaluationFn = decision.EvaluationFn };
                    if(effectedCols[0] || effectedCols[1])
                    {
                        // ensure we have the correct indexing ;)
                        if(reStackIndex < stack.TheStack.Count)
                        {
                            stack.TheStack[reStackIndex++] = newDecision;
                        }
                        else
                        {
                            stack.TheStack.Insert(reStackIndex++, newDecision);
                        }
                    }
                    else
                    {
                        stack.TheStack.Insert(reStackIndex++, newDecision);
                    }
                }
            }
            return stack;
        }

        /// <summary>
        /// Fetches the data list ID.
        /// </summary>
        /// <param name="oldAmbientData">The old ambient data.</param>
        /// <returns></returns>
        public Guid FetchDataListID(IList<string> oldAmbientData)
        {
            Guid dlID = GlobalConstants.NullDataListID;
            if(oldAmbientData != null && oldAmbientData.Count == 1)
            {
                Guid.TryParse(oldAmbientData[0], out dlID);
            }

            return dlID;
        }
    }
}
