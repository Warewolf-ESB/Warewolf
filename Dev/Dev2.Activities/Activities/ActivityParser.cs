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

namespace Dev2.Activities
{
    public class ActivityParser : IActivityParser
    {
        #region Implementation of IActivityParser

        public IDev2Activity Parse(DynamicActivity dynamicActivity,Dictionary<string,IDev2Activity> seenActivities )
        {
            try
            {

    
            var chart = WorkflowInspectionServices.GetActivities(dynamicActivity).FirstOrDefault() as Flowchart;
            if(chart != null)
            {
                var start = (chart.StartNode) as FlowStep;
                
                if(start != null)
                {
                    var tool = ParseTools(start,seenActivities);
                    return tool.FirstOrDefault();
                }
                var flowstart = (chart.StartNode) as FlowSwitch<string>;
                if(flowstart != null)
                {
                    return ParseSwitch(flowstart, seenActivities).FirstOrDefault();
                }
                var flowdec = (chart.StartNode) as FlowDecision;
                return ParseDecision(flowdec, seenActivities).FirstOrDefault();
            }
            return null;
            }
            catch (InvalidWorkflowException e)
            {

                Dev2Logger.Log.Error(e);
                throw;
            }
        }

        IEnumerable<IDev2Activity> ParseTools(FlowNode startNode, Dictionary<string, IDev2Activity> seenActivities)
        {
          
            if (startNode == null)
                return null;
            FlowStep step = startNode as FlowStep;
            if (step != null)
            {
                
                return ParseFlowStep(step, seenActivities);
                // ReSharper disable RedundantIfElseBlock
            }
            else
            {
                FlowDecision node = startNode as FlowDecision;
                if (node != null)

                    return ParseDecision(node, seenActivities);
                else
                {
                    FlowSwitch<string> @switch = startNode as FlowSwitch<string>;
                    if (@switch != null)
                        return ParseSwitch(@switch, seenActivities);
                }
            }
            return null;
            // ReSharper restore RedundantIfElseBlock
        }

        IEnumerable<IDev2Activity> ParseSwitch(FlowSwitch<string> switchFlowSwitch, Dictionary<string, IDev2Activity> seenActivities)
        {
            var activity = switchFlowSwitch.Expression as DsfFlowSwitchActivity;
            if (activity != null)
            {
                if (seenActivities.ContainsKey(activity.UniqueID))
                {
                    return new List<IDev2Activity> { seenActivities[activity.UniqueID] };
                }

                var val = new StringBuilder(Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(activity.ExpressionText));
                 Dev2Switch ds = new Dev2Switch { SwitchVariable = val.ToString() };
                 var swi = new DsfSwitch
                     {
                         Switches = switchFlowSwitch.Cases.Select(a => new Tuple<string, IDev2Activity>(a.Key, ParseTools(a.Value, seenActivities).FirstOrDefault())).ToDictionary((a => a.Item1), a => a.Item2),
                         Default = ParseTools(switchFlowSwitch.Default, seenActivities),
                         Switch = ds.SwitchVariable
                     };
                seenActivities.Add(activity.UniqueID,swi);
                return new List<IDev2Activity>
                {  swi
                };
            }
            throw new Exception("Invalid activity");
          
        }

        IEnumerable<IDev2Activity> ParseDecision(FlowDecision decision, Dictionary<string, IDev2Activity> seenActivities)
        {
            
            var activity = decision.Condition as DsfFlowDecisionActivity;
            if(activity != null)
            {
                if( seenActivities.ContainsKey( activity.UniqueID))
                {
                    return new List<IDev2Activity> { seenActivities[activity.UniqueID]};
                }
                var rawText = activity.ExpressionText;
                // ReSharper disable MaximumChainedReferences
                var activityTextjson = rawText.Substring(rawText.IndexOf("{", StringComparison.Ordinal)).Replace(@""",AmbientDataList)","").Replace("\"","!");
                // ReSharper restore MaximumChainedReferences
                var activityText = Dev2DecisionStack.FromVBPersitableModelToJSON(activityTextjson);
                var decisionStack =  JsonConvert.DeserializeObject<Dev2DecisionStack>(activityText);
                var dec = new DsfDecision
                {
                    TrueArm = ParseTools(decision.True, seenActivities),
                    FalseArm = ParseTools(decision.False, seenActivities),
                    Conditions = decisionStack,
                    And = decisionStack.Mode == Dev2DecisionMode.AND
                };
                seenActivities.Add(activity.UniqueID,dec);
                return new List<IDev2Activity>
                { dec};
            }
            throw new Exception("Invalid activity");
        }

        IEnumerable<IDev2Activity> ParseFlowStep(FlowStep startNode, Dictionary<string, IDev2Activity> seenActivities)
        {
            var action = startNode.Action as IDev2Activity;
            if (action == null)
                return null;
            if (seenActivities.ContainsKey(action.UniqueID))
                return new List<IDev2Activity> { seenActivities[action.UniqueID]};
            var tools = ParseTools(startNode.Next, seenActivities);

            action.NextNodes = tools;

            return new List<IDev2Activity> { action };
        }

        #endregion

        #region Implementation of IActivityParser

        public IDev2Activity Parse(DynamicActivity dynamicActivity)
        {
            return Parse(dynamicActivity, new Dictionary<string, IDev2Activity>());
        }

        #endregion
    }
}