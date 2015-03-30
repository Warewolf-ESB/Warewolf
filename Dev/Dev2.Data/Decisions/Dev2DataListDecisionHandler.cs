
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Text;
using Dev2.Common;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Warewolf.Storage;
using DataListUtil = Dev2.Data.Util.DataListUtil;
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.Data.Decision
{
    public class Dev2DataListDecisionHandler
    {
        private static readonly IDataListCompiler Compiler = DataListFactory.CreateDataListCompiler();
        private static Dev2DataListDecisionHandler _inst;
        private static readonly IDictionary<Guid, IExecutionEnvironment> _environments = new ConcurrentDictionary<Guid, IExecutionEnvironment>();
        public static Dev2DataListDecisionHandler Instance
        {
            get
            {
                return _inst ?? (_inst = new Dev2DataListDecisionHandler());
            }
        }


        public void AddEnvironment(Guid id, IExecutionEnvironment env)
        {
            _environments.Add(id,env);
        }

        public void RemoveEnvironment(Guid id)
        {
            _environments.Remove(id);
        }
        /// <summary>
        /// Fetches the switch data.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="oldAmbientData">The old ambient data.</param>
        /// <returns></returns>
        public string FetchSwitchData(string variableName, IList<string> oldAmbientData)
        {
       

      
            Guid dlId = FetchDataListID(oldAmbientData);

            var env = _environments[dlId];
            var output = ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(variableName));
        
            return output;

        }

        // Guid dlID
        /// <summary>
        /// Executes the decision stack.
        /// </summary>
        /// <param name="decisionDataPayload">The decision data payload.</param>
        /// <param name="oldAmbientData">The old ambient data.</param>
        /// <returns></returns>
        /// <exception cref="System.Data.InvalidExpressionException">Could not evaluate decision data - No decision function found for [  + typeOf + ]</exception>
        public bool ExecuteDecisionStack(string decisionDataPayload, IList<string> oldAmbientData)
        {

            // Evaluate decisionDataPayload through the EvaluateFunction ;)
            Guid dlId = FetchDataListID(oldAmbientData);
            if(dlId == GlobalConstants.NullDataListID) throw new InvalidExpressionException("Could not evaluate decision data - no DataList ID sent!");
            // Swap out ! with a new internal token to avoid nasty issues with 
            string newDecisionData = Dev2DecisionStack.FromVBPersitableModelToJSON(decisionDataPayload);
         //   var env= _environments[dlId];
            var dds = EvaluateRegion(newDecisionData, dlId);

            ErrorResultTO errors = new ErrorResultTO();
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
                                    dd.Col1 = String.Join("", env.Errors);
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
                                        ErrorResultTO errorErrors;
                                        errors.AddError(e.Message);
                                        Compiler.UpsertSystemTag(dlId, enSystemTag.Dev2Error, errors.MakeDataListReady(), out errorErrors);

                                        return false;
                                    }
                                }
                                else
                                {
                                    throw new InvalidExpressionException("Could not evaluate decision data - No decision function found for [ " + typeOf + " ]");
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

                        throw new InvalidExpressionException("Could not evaluate decision data - Invalid model data sent!");
                    }
                    catch
                    {
                        // all hell has broken loose... ;)
                        throw new InvalidExpressionException("Could not evaluate decision data - No model data sent!");
                    }
                }

                throw new InvalidExpressionException("Could not evaluate decision data - no DataList ID sent!");
            }

            throw new InvalidExpressionException("Could not populate decision model - DataList Errors!");
        }

        private IBinaryDataListEntry EvaluateForSwitch(string payload, Guid dlId, out ErrorResultTO errors)
        {
            IBinaryDataListEntry tmp = Compiler.Evaluate(dlId, enActionType.User, payload, false, out errors);

            return tmp;
        }

        /// <summary>
        /// Evaluates the region.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="dlId">The dl ID.</param>
        /// <returns></returns>
        private Dev2DecisionStack EvaluateRegion(string payload, Guid dlId)
        {

            var env =  _environments[dlId];

            if(payload.StartsWith("{\"TheStack\":[{") || payload.StartsWith("{'TheStack':[{"))
            {
                //2013.05.06: Ashley Lewis for PBI 9460 - handle record-sets with stars in their index by resolving them
                var dds = Compiler.ConvertFromJsonToModel<Dev2DecisionStack>(new StringBuilder(payload));

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
                            dd.Col1 = ExecutionEnvironment.WarewolfEvalResultToString(env.Eval( dd.Col1));
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
                            dd.Col2 =  ExecutionEnvironment.WarewolfEvalResultToString(env.Eval( dd.Col2));
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
                            dd.Col3 = ExecutionEnvironment.WarewolfEvalResultToString(env.Eval( dd.Col3));
                        }
                    }
                    //Remove those record sets and replace them with a new decision for each resolved value
                    foreach(Dev2Decision decision in invalidDecisions)
                    {
                        ErrorResultTO errors;
                        dds = ResolveAllRecords(env, dds, decision, effectedCols, out errors);
                    }
                }

                return dds;
            }
            return null;
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

        Dev2DecisionStack ResolveAllRecords(IExecutionEnvironment env, Dev2DecisionStack stack, Dev2Decision decision, bool[] effectedCols, out ErrorResultTO errors)
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
                var data = env.EvalAsListOfStrings(decision.Col1);
               
                int reStackIndex = stackIndex;

                foreach(var item in data)
                {


                    var newDecision = new Dev2Decision { Col1 = item, Col2 = decision.Col2, Col3 = decision.Col3, EvaluationFn = decision.EvaluationFn };
                    stack.TheStack.Insert(reStackIndex++, newDecision);
                }
            }
            if(effectedCols[1])
            {
                var data = env.EvalAsListOfStrings(decision.Col2);
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
                var data = env.EvalAsListOfStrings(decision.Col3);
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
