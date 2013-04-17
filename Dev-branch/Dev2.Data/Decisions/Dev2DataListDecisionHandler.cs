using Dev2.Common;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using System;
using System.Collections.Generic;
using System.Data;

namespace Dev2.Data.Decision
{
    public class Dev2DataListDecisionHandler 
    {
        private static readonly IDataListCompiler _compiler = DataListFactory.CreateDataListCompiler();
        private static Dev2DataListDecisionHandler _inst;

        public static Dev2DataListDecisionHandler Instance
        {
            get
            {
                if (_inst == null)
                {
                    _inst = new Dev2DataListDecisionHandler();
                }

                return _inst;
            }

        }

        /// <summary>
        /// Fetches the switch data.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="oldAmbientData">The old ambient data.</param>
        /// <returns></returns>
        public string FetchSwitchData(string variableName, IList<string> oldAmbientData)
        {
            Guid dlID = FetchDataListID(oldAmbientData);
            IBinaryDataListEntry tmp = EvaluateRegion(variableName, dlID);

            if(tmp != null)
            {
                if(tmp.IsRecordset)
                {
                    string error;
                    return tmp.TryFetchLastIndexedRecordsetUpsertPayload(out error).TheValue;
                }

                return tmp.FetchScalar().TheValue;
            }

            return string.Empty;
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
            Guid dlID = FetchDataListID(oldAmbientData);
            if (dlID == GlobalConstants.NullDataListID) throw new InvalidExpressionException("Could not evaluate decision data - no DataList ID sent!");
            // Swap out ! with a new internal token to avoid nasty issues with 
            string newDecisionData = Dev2DecisionStack.FromVBPersitableModelToJSON(decisionDataPayload);

            IBinaryDataListEntry tmp = EvaluateRegion(newDecisionData, dlID);
            ErrorResultTO errors = new ErrorResultTO();
            
            if (tmp != null)
            {
                string model = tmp.FetchScalar().TheValue; // Get evalauted data value

                if (dlID != GlobalConstants.NullDataListID)
                {

                    //model = Dev2DecisionStack.FromVBPersitableModelToJSON(model);

                    try
                    {
                        Dev2DecisionStack dds = _compiler.ConvertFromJsonToModel<Dev2DecisionStack>(model);

                        if (dds.TheStack != null)
                        {

                            for (int i = 0; i < dds.TotalDecisions; i++)
                            {
                                Dev2Decision dd = dds.GetModelItem(i);
                                enDecisionType typeOf = dd.EvaluationFn;

                                // Treat Errors special
                                if (typeOf == enDecisionType.IsError || typeOf == enDecisionType.IsNotError)
                                {
                                    dd.Col1 = _compiler.EvaluateSystemEntry(dlID, enSystemTag.Error, out errors);
                                }

                                IDecisionOperation op = Dev2DecisionFactory.Instance().FetchDecisionFunction(typeOf);
                                if (op != null)
                                {
                                    try
                                    {
                                        bool result = op.Invoke(dds.GetModelItem(i).FetchColsAsArray());

                                        if(!result && dds.Mode == Dev2DecisionMode.AND)
                                        {
                                            // Naughty stuff, we have a false in AND mode... break;
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
                                        _compiler.UpsertSystemTag(dlID, enSystemTag.Error, errors.MakeDataListReady(), out errorErrors);

                                        return false;
                                    }
                                }
                                else
                                {
                                    throw new InvalidExpressionException("Could not evaluate decision data - No decision function found for [ " + typeOf +" ]");
                                }
                            }

                            // else we are in AND mode and all have passed ;)
                            if (dds.Mode == Dev2DecisionMode.AND)
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


        /// <summary>
        /// Evaluates the region.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="dlID">The dl ID.</param>
        /// <returns></returns>
        private IBinaryDataListEntry EvaluateRegion(string payload, Guid dlID )
        {
            
            ErrorResultTO errors;
            IBinaryDataListEntry tmp = _compiler.Evaluate(dlID, enActionType.User, payload, false, out errors);

            return tmp;

        }

        /// <summary>
        /// Fetches the data list ID.
        /// </summary>
        /// <param name="oldAmbientData">The old ambient data.</param>
        /// <returns></returns>
        public Guid FetchDataListID(IList<string> oldAmbientData)
        {
            Guid dlID = GlobalConstants.NullDataListID;
            if (oldAmbientData != null && oldAmbientData.Count == 1)
            {
                Guid.TryParse(oldAmbientData[0], out dlID);
            }

            return dlID;
        }
    }
}
