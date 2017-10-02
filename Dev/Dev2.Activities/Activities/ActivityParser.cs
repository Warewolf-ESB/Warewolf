using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Data.SystemTemplates.Models;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Resource.Errors;

namespace Dev2.Activities
{
    public class ActivityParser : IActivityParser
    {
        #region Implementation of IActivityParser

        public IDev2Activity Parse(DynamicActivity dynamicActivity, List<IDev2Activity> seenActivities)
        {
            try
            {
                if (WorkflowInspectionServices.GetActivities(dynamicActivity).FirstOrDefault() is Flowchart chart)
                {
                    if (chart.StartNode == null)
                    {
                        return null;
                    }

                    if (chart.StartNode is FlowStep start)
                    {
                        var tool = ParseTools(start, seenActivities);
                        return tool.FirstOrDefault();
                    }
                    if (chart.StartNode is FlowSwitch<string> flowstart)
                    {
                        return ParseSwitch(flowstart, seenActivities).FirstOrDefault();
                    }
                    var flowdec = chart.StartNode as FlowDecision;
                    return ParseDecision(flowdec, seenActivities).FirstOrDefault();
                }
                return null;
            }
            catch (InvalidWorkflowException e)
            {

                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                throw;
            }
        }

        IEnumerable<IDev2Activity> ParseTools(FlowNode startNode, List<IDev2Activity> seenActivities)
        {
          
            if (startNode == null)
            {
                return null;
            }

            if (startNode is FlowStep step)
            {

                return ParseFlowStep(step, seenActivities);

            }
            else
            {
                if (startNode is FlowDecision node)
                {
                    return ParseDecision(node, seenActivities);
                }
                else
                {
                    if (startNode is FlowSwitch<string> @switch)
                    {
                        return ParseSwitch(@switch, seenActivities);
                    }
                }
            }
            return null;
            
        }

        IEnumerable<IDev2Activity> ParseSwitch(FlowSwitch<string> switchFlowSwitch, List<IDev2Activity> seenActivities)
        {
            if (switchFlowSwitch.Expression is DsfFlowSwitchActivity activity)
            {
                if (seenActivities.Contains(activity))
                {
                    return new List<IDev2Activity> { activity };
                }

                var val = new StringBuilder(Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(activity.ExpressionText));
                Dev2Switch ds = new Dev2Switch { SwitchVariable = val.ToString() };
                var swi = new DsfSwitch(activity);
                if (!seenActivities.Contains(activity))
                {
                    seenActivities.Add(swi);
                }
                swi.Switches = switchFlowSwitch.Cases.Select(a => new Tuple<string, IDev2Activity>(a.Key, ParseTools(a.Value, seenActivities).FirstOrDefault())).ToDictionary(a => a.Item1, a => a.Item2);
                swi.Default = ParseTools(switchFlowSwitch.Default, seenActivities);
                swi.Switch = ds.SwitchVariable;


                return new List<IDev2Activity>
                {  swi
                };
            }
            throw new Exception(ErrorResource.InvalidActivity);
          
        }

        IEnumerable<IDev2Activity> ParseDecision(FlowDecision decision, List<IDev2Activity> seenActivities)
        {

            if (decision.Condition is DsfFlowDecisionActivity activity)
            {
                if (seenActivities.Contains(activity))
                {
                    return new List<IDev2Activity> { activity };
                }
                var rawText = activity.ExpressionText;

                var activityTextjson = rawText.Substring(rawText.IndexOf("{", StringComparison.Ordinal)).Replace(@""",AmbientDataList)", "").Replace("\"", "!");

                var activityText = Dev2DecisionStack.FromVBPersitableModelToJSON(activityTextjson);
                var decisionStack = JsonConvert.DeserializeObject<Dev2DecisionStack>(activityText);
                var dec = new DsfDecision(activity);
                if (!seenActivities.Contains(activity))
                {
                    seenActivities.Add(dec);
                }
                dec.TrueArm = ParseTools(decision.True, seenActivities);
                dec.FalseArm = ParseTools(decision.False, seenActivities);
                dec.Conditions = decisionStack;
                dec.And = decisionStack.Mode == Dev2DecisionMode.AND;


                return new List<IDev2Activity>
                { dec};
            }
            throw new Exception(ErrorResource.InvalidActivity);
        }

        IEnumerable<IDev2Activity> ParseFlowStep(FlowStep startNode, List<IDev2Activity> seenActivities)
        {
            var action = startNode.Action as IDev2Activity;
            if (action == null)
            {
                return null;
            }

            if (seenActivities.Contains(action))
            {
                return new List<IDev2Activity> { action};
            }

            if (!seenActivities.Contains(action))
            {
                seenActivities.Add(action);
            }
            var tools = ParseTools(startNode.Next, seenActivities);

            action.NextNodes = tools;

            return new List<IDev2Activity> { action };
        }

        #endregion

        #region Implementation of IActivityParser

        public IDev2Activity Parse(DynamicActivity dynamicActivity)
        {
            return Parse(dynamicActivity, new List<IDev2Activity>());
        }

        #endregion
    }
}