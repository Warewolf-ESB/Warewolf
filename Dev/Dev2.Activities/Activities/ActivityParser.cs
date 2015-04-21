using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Dev2.Activities;
using Dev2.Data.SystemTemplates.Models;
using Newtonsoft.Json;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public  class ResourceCache
    {
        ActivityParser _activityParser;
        ConcurrentDictionary<Guid,IDev2Activity> _cache;

        public ResourceCache(ActivityParser activityParser, ConcurrentDictionary<Guid, IDev2Activity> cache)
        {
            _activityParser = activityParser;
            _cache = cache;
        }

        public ResourceCache()
        {
            _activityParser = new ActivityParser();
            _cache = new ConcurrentDictionary<Guid, IDev2Activity>();
        }

        public IDev2Activity Parse(Func<DynamicActivity> actFunc,Guid resourceIdGuid)
        {
            if(_cache.ContainsKey(resourceIdGuid))
            {
                return _cache[resourceIdGuid];
            }
            var dynamicActivity = actFunc();
            IDev2Activity act = _activityParser.Parse(dynamicActivity);
            if (_cache.TryAdd(resourceIdGuid, act))
            {
                return act;
            }
            return _cache[resourceIdGuid];
        }

        public bool HasId(Guid resourceID)
        {
            return _cache.ContainsKey(resourceID);
        }
    }
    
    
    public  class ActivityParser : IActivityParser
    {
        #region Implementation of IActivityParser

        public IDev2Activity Parse(DynamicActivity dynamicActivity)
        {
            var chart = WorkflowInspectionServices.GetActivities(dynamicActivity).FirstOrDefault() as Flowchart;
            if(chart != null)
            {
                var start = (chart.StartNode) as FlowStep;

                if(start != null)
                {
                    var tool = ParseTools(start);
                    return tool.FirstOrDefault();
                }
                var flowstart = (chart.StartNode) as FlowSwitch<string>;
                if(flowstart != null)
                {
                    return ParseSwitch(flowstart).FirstOrDefault();
                }
                var flowdec = (chart.StartNode) as FlowDecision;
                return ParseDecision(flowdec).FirstOrDefault();
            }
            return null;
        }

        IEnumerable<IDev2Activity> ParseTools(FlowNode startNode)
        {
            if (startNode == null)
                return null;
            FlowStep step = startNode as FlowStep;
            if (step != null)
                return ParseFlowStep(step);
                // ReSharper disable RedundantIfElseBlock
            else
            {
                FlowDecision node = startNode as FlowDecision;
                if (node != null)
               
                    return ParseDecision(node);
                else
                {
                    FlowSwitch<string> @switch = startNode as FlowSwitch<string>;
                    if (@switch != null)
                        return ParseSwitch(@switch);
                }
            }
            return null;
            // ReSharper restore RedundantIfElseBlock
        }

        IEnumerable<IDev2Activity> ParseSwitch(FlowSwitch<string> switchFlowSwitch)
        {
            var activity = switchFlowSwitch.Expression as DsfFlowSwitchActivity;
            if (activity != null)
            {
                var val = new StringBuilder(Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(activity.ExpressionText));
                 Dev2Switch ds = new Dev2Switch { SwitchVariable = val.ToString() };
                return new List<IDev2Activity>
                {  new DsfSwitch
                    {
                        Switches   =  switchFlowSwitch.Cases.Select( a=> new Tuple<string,IDev2Activity>(a.Key,ParseTools(a.Value).FirstOrDefault())).ToDictionary((a=>a.Item1),a=>a.Item2),
                        Default   = ParseTools(switchFlowSwitch.Default),
                        Switch = ds.SwitchVariable
                    }
                };
            }
            throw new Exception("Invalid activity");
          
        }

        IEnumerable<IDev2Activity> ParseDecision(FlowDecision decision)
        {
            var activity = decision.Condition as DsfFlowDecisionActivity;
            if(activity != null)
            {
                var rawText = activity.ExpressionText;
                // ReSharper disable MaximumChainedReferences
                var activityTextjson = rawText.Substring(rawText.IndexOf("{", StringComparison.Ordinal)).Replace(@""",AmbientDataList)","").Replace("\"","!");
                // ReSharper restore MaximumChainedReferences
                var activityText = Dev2DecisionStack.FromVBPersitableModelToJSON(activityTextjson);
                var decisionStack =  JsonConvert.DeserializeObject<Dev2DecisionStack>(activityText);
                return new List<IDev2Activity>
                {  new DsfDecision
                {
                    TrueArm   = ParseTools(decision.True),
                    FalseArm   = ParseTools(decision.False),
                    Conditions = decisionStack,
                    And=decisionStack.Mode== Dev2DecisionMode.AND
                }};
            }
            throw new Exception("Invalid activity");
        }

        IEnumerable<IDev2Activity> ParseFlowStep(FlowStep startNode)
        {
            var action = startNode.Action as IDev2Activity;
            if (action == null)
                return null;
            action.NextNodes = ParseTools(startNode.Next);
            return new List<IDev2Activity> { action };

        }

        #endregion
    }
}