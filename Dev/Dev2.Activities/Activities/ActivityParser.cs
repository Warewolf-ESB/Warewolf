using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Activities.SelectAndApply;
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

        public IDev2Activity Parse(List<IDev2Activity> seenActivities, object step)
        {
            var modelItem = step as ModelItem;
            var currentValue = modelItem?.GetCurrentValue();
            if (currentValue is null)
            {
                return default;
            }

            if (currentValue is FlowStep start)
            {
                var tool = ParseTools(start, seenActivities);
                return tool.FirstOrDefault();
            }
            if (currentValue is FlowSwitch<string> flowstart)
            {
                return ParseSwitch(flowstart, seenActivities).FirstOrDefault();
            }
            var flowdec = currentValue as FlowDecision;
            return ParseDecision(flowdec, seenActivities).FirstOrDefault();
        }

        public IEnumerable<IDev2Activity> ParseToLinkedFlatList(IDev2Activity topLevelActivity)
        {
            if (topLevelActivity is DsfDecision roodDecision)
            {
                IEnumerable<IDev2Activity> vb;
                if (roodDecision.TrueArm == null)
                {
                    vb = roodDecision.FalseArm;
                }
                else if (roodDecision.FalseArm == null)
                {
                    vb = roodDecision.TrueArm;
                }
                else
                {
                    vb = roodDecision.FalseArm.Union(roodDecision.TrueArm);
                }

                var bbb = vb.Flatten(activity =>
                {
                    if (activity.NextNodes != null)
                    {
                        return activity.NextNodes;
                    }

                    if (activity is DsfDecision a)
                    {
                        if (a.TrueArm == null)
                        {
                            return a.FalseArm;
                        }

                        if (a.FalseArm == null)
                        {
                            return a.TrueArm;
                        }

                        var activities = a.FalseArm.Union(a.TrueArm);
                        return activities;
                    }
                    return new List<IDev2Activity>();
                }).ToList();
                var hasDecision = bbb.Contains(topLevelActivity);
                if (!hasDecision)
                {
                    bbb.Add(topLevelActivity);
                }
                return bbb.ToList();
            }
            if (topLevelActivity is DsfSwitch @switch)
            {
                var vv = @switch.Switches.ToDictionary(k => k.Key);
                var activities = vv.Values.Select(k => k.Value);
                return activities;
            }
            if (topLevelActivity is DsfForEachActivity f)
            {
                var dev2Activity = (f.DataFunc.Handler as IDev2Activity);
                return dev2Activity?.NextNodes ?? new List<IDev2Activity>();
            }
            if (topLevelActivity is DsfSelectAndApplyActivity s)
            {
                var dev2Activity = (s.ApplyActivityFunc.Handler as IDev2Activity);
                return dev2Activity?.NextNodes ?? new List<IDev2Activity>();
            }
            var dev2Activities = topLevelActivity.NextNodes?.Flatten(activity =>
            {
                if (activity.NextNodes != null)
                {
                    return activity.NextNodes;
                }

                if (activity is DsfDecision a)
                {
                    if (a.TrueArm == null)
                    {
                        return a.FalseArm;
                    }

                    if (a.FalseArm == null)
                    {
                        return a.TrueArm;
                    }

                    var activities = a.FalseArm.Union(a.TrueArm);
                    return activities;
                }
                if (activity is DsfSwitch b)
                {
                    var vv = b.Switches.ToDictionary(k => k.Key);
                    var activities = vv.Values.Select(k => k.Value).Union(b.Default);
                    return activities;
                }
                if (activity is DsfForEachActivity c)
                {
                    var dev2Activity = (c.DataFunc.Handler as IDev2Activity);
                    return dev2Activity?.NextNodes ?? new List<IDev2Activity>();
                }
                if (activity is DsfSelectAndApplyActivity d)
                {
                    var dev2Activity = (d.ApplyActivityFunc.Handler as IDev2Activity);
                    return dev2Activity?.NextNodes ?? new List<IDev2Activity>();
                }
                return new List<IDev2Activity>();
            }).ToList() ?? new List<IDev2Activity>();
            var contains = dev2Activities.Contains(topLevelActivity);
            if (!contains)
            {
                dev2Activities.Add(topLevelActivity);
            }
            return dev2Activities;
        }

        public IEnumerable<IDev2Activity> FlattenNextNodesInclusive(IDev2Activity decision)
        {
            switch (decision)
            {
                case DsfDecision a:
                    {
                        return new List<IDev2Activity>() { a };
                    }
                case DsfSwitch b:
                    {
                        return new List<IDev2Activity>() { b };
                    }
                default:
                    {
                        var truArmToFlatList = ActivityToFlatList(decision);
                        return truArmToFlatList;
                    }
            }
        }
       

        private static List<IDev2Activity> ActivityToFlatList(IDev2Activity decision)
        {
            var truArmToFlatList =
                decision.NextNodes?.Flatten(activity => activity.NextNodes ?? new List<IDev2Activity>()).ToList() ??
                new List<IDev2Activity>();
            var contains = truArmToFlatList.Contains(decision);
            if (!contains)
            {
                truArmToFlatList.Add(decision);
            }
            return truArmToFlatList;
        }

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

            if (startNode is FlowDecision node)
            {
                return ParseDecision(node, seenActivities);
            }

            if (startNode is FlowSwitch<string> @switch)
            {
                return ParseSwitch(@switch, seenActivities);
            }

            return null;

        }

        IEnumerable<IDev2Activity> ParseSwitch(FlowSwitch<string> switchFlowSwitch, List<IDev2Activity> seenActivities)
        {
            var activity = switchFlowSwitch.Expression as DsfFlowSwitchActivity;
            if (activity != null)
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

            var activity = decision.Condition as DsfFlowDecisionActivity;
            if (activity != null)
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
            if (!(startNode.Action is IDev2Activity action))
            {
                return null;
            }

            if (seenActivities.Contains(action))
            {
                return new List<IDev2Activity> { action };
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