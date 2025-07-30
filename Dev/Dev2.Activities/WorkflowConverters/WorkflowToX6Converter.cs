using Dev2.Common.X6;
using Dev2.Data.SystemTemplates.Models;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    /// <summary>
    /// Converts workflow to X6 based Json
    /// </summary>
    public class WorkflowToX6Converter
    {
        private int _currentX = 100;
        private int _currentY = 100;

        // Cache for reflection-based property lookups to avoid repeated reflection calls
        private static readonly Dictionary<Type, PropertyInfo[]> _typePropertyCache = new();
        private static readonly Dictionary<Type, PropertyInfo[]> _childActivityPropertiesCache = new();

        // Reusable collections to reduce allocations
        private readonly List<Activity> _tempChildActivities = new(16);
        private readonly List<string> _tempEndNodes = new(8);

        /// <summary>
        /// Convert workflow (ActivityBuilder) to X6 Json
        /// </summary>
        /// <param name="workflow">ActivityBuilder</param>
        /// <param name="xml">Workflow Xaml</param>
        /// <returns>Json serialized string</returns>
        public string ConvertToX6Json(ActivityBuilder workflow, string xml)
        {
            var graphData = new X6WorkflowLoadModel { WorkflowXml = xml };
            var activityNodeMap = new Dictionary<Activity, string>(64); 

            var startNode = CreateStartNode();
            graphData.Nodes.Add(startNode);

            if (workflow.Implementation != null)
            {
                ProcessActivity(workflow.Implementation, graphData, activityNodeMap, startNode.id);
            }

            return JsonConvert.SerializeObject(graphData);
        }

        private string ProcessActivity(Activity activity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            if (activity == null) return previousNodeId;

            string nodeId;

            if (activity is not Flowchart)
            {
                nodeId = GenerateNodeId();
                activityNodeMap[activity] = nodeId;
                var node = CreateActivityNode(activity, nodeId);
                graphData.Nodes.Add(node);

                if (!string.IsNullOrEmpty(previousNodeId))
                {
                    graphData.Edges.Add(CreateEdge(previousNodeId, nodeId));
                }
            }
            else
            {
                nodeId = previousNodeId;
            }

            return activity switch
            {
                Sequence sequence => ProcessSequence(sequence, graphData, activityNodeMap, nodeId),
                Flowchart flowchart => ProcessFlowchart(flowchart, graphData, activityNodeMap, nodeId),
                If ifActivity => ProcessIfActivity(ifActivity, graphData, activityNodeMap, nodeId),
                While whileActivity => ProcessWhileActivity(whileActivity, graphData, activityNodeMap, nodeId),
                DoWhile doWhileActivity => ProcessDoWhileActivity(doWhileActivity, graphData, activityNodeMap, nodeId),
                TryCatch tryCatchActivity => ProcessTryCatchActivity(tryCatchActivity, graphData, activityNodeMap, nodeId),
                Parallel parallelActivity => ProcessParallelActivity(parallelActivity, graphData, activityNodeMap, nodeId),
                //ForEach<> forEachActivity => ProcessForEachActivity(forEachActivity, graphData, activityNodeMap, nodeId),
                _ => ProcessGenericActivity(activity, graphData, activityNodeMap, nodeId)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GenerateNodeId() => Guid.NewGuid().ToString();

        private string ProcessSequence(Sequence sequence, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            var currentNodeId = parentNodeId;
            var activities = sequence.Activities;

            for (int i = 0; i < activities.Count; i++)
            {
                currentNodeId = ProcessActivity(activities[i], graphData, activityNodeMap, currentNodeId);
            }

            return currentNodeId;
        }

        private string ProcessFlowchart(Flowchart flowchart, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            if (flowchart.StartNode == null) return parentNodeId;

            return ProcessFlowNode(flowchart.StartNode, graphData, activityNodeMap, parentNodeId);
        }

        private string ProcessFlowNode(FlowNode flowNode, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            return flowNode switch
            {
                FlowStep flowStep => ProcessFlowStep(flowStep, graphData, activityNodeMap, previousNodeId),
                FlowDecision flowDecision => ProcessFlowDecision(flowDecision, graphData, activityNodeMap, previousNodeId),
                FlowSwitch<object> flowSwitch => ProcessFlowSwitch(flowSwitch, graphData, activityNodeMap, previousNodeId),
                _ => previousNodeId
            };
        }
        
        private string ProcessFlowStep(FlowStep flowStep, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            var nodeId = ProcessActivity(flowStep.Action, graphData, activityNodeMap, previousNodeId);

            if (flowStep.Next != null)
            {
                ProcessFlowNode(flowStep.Next, graphData, activityNodeMap, nodeId);
            }

            return nodeId;
        }

        private string ProcessFlowDecision(FlowDecision flowDecision, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            var decisionNodeId = GenerateNodeId();
            var decisionNode = CreateDecisionNode(flowDecision, decisionNodeId);
            graphData.Nodes.Add(decisionNode);

            if (!string.IsNullOrEmpty(previousNodeId))
            {
                graphData.Edges.Add(CreateEdge(previousNodeId, decisionNodeId));
            }

            // Process True branch
            if (flowDecision.True != null)
            {
                var trueNodeId = ProcessFlowNode(flowDecision.True, graphData, activityNodeMap, null);
                var targetNodeId = activityNodeMap.ContainsValue(trueNodeId) ? trueNodeId : GetFirstNodeId(flowDecision.True, activityNodeMap);
                graphData.Edges.Add(CreateEdge(decisionNodeId, targetNodeId, Constants.TRUE));
            }

            // Process False branch
            if (flowDecision.False != null)
            {
                var falseNodeId = ProcessFlowNode(flowDecision.False, graphData, activityNodeMap, null);
                var targetNodeId = activityNodeMap.ContainsValue(falseNodeId) ? falseNodeId : GetFirstNodeId(flowDecision.False, activityNodeMap);
                graphData.Edges.Add(CreateEdge(decisionNodeId, targetNodeId, Constants.FALSE));
            }

            return decisionNodeId;
        }

        private string ProcessFlowSwitch(FlowSwitch<object> flowSwitch, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            var switchNodeId = GenerateNodeId();
            var switchNode = CreateSwitchNode(flowSwitch, switchNodeId);
            graphData.Nodes.Add(switchNode);

            if (!string.IsNullOrEmpty(previousNodeId))
            {
                graphData.Edges.Add(CreateEdge(previousNodeId, switchNodeId));
            }

            foreach (var caseItem in flowSwitch.Cases)
            {
                if (caseItem.Value != null)
                {
                    ProcessFlowNode(caseItem.Value, graphData, activityNodeMap, null);
                    var targetNodeId = GetFirstNodeId(caseItem.Value, activityNodeMap);
                    var label = caseItem.Key?.ToString() ?? "Case";
                    graphData.Edges.Add(CreateEdge(switchNodeId, targetNodeId, label));
                }
            }

            if (flowSwitch.Default != null)
            {
                ProcessFlowNode(flowSwitch.Default, graphData, activityNodeMap, null);
                var targetNodeId = GetFirstNodeId(flowSwitch.Default, activityNodeMap);
                graphData.Edges.Add(CreateEdge(switchNodeId, targetNodeId, "Default"));
            }

            return switchNodeId;
        }

        private string ProcessIfActivity(If ifActivity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            var lastNodeId = parentNodeId;

            // Process Then branch
            if (ifActivity.Then != null)
            {
                lastNodeId = ProcessActivity(ifActivity.Then, graphData, activityNodeMap, parentNodeId);
            }

            // Process Else branch
            if (ifActivity.Else != null)
            {
                var elseNodeId = ProcessActivity(ifActivity.Else, graphData, activityNodeMap, parentNodeId);
                lastNodeId = elseNodeId; // Use the else branch as the final node
            }

            return lastNodeId;
        }

        private string ProcessWhileActivity(While whileActivity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            if (whileActivity.Body == null) return parentNodeId;

            var bodyNodeId = ProcessActivity(whileActivity.Body, graphData, activityNodeMap, parentNodeId);
            
            // Create loop back edge
            graphData.Edges.Add(CreateEdge(bodyNodeId, parentNodeId, "Loop"));
            return bodyNodeId;
        }

        //private string ProcessForEachActivity(ForEach forEachActivity, X6GraphData graphData,
        //    Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        //{
        //    if (forEachActivity.Body != null)
        //    {
        //        return ProcessActivity(forEachActivity.Body.Handler, graphData, activityNodeMap, parentNodeId);
        //    }

        //    return parentNodeId;
        //}

        private string ProcessDoWhileActivity(DoWhile doWhileActivity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            if (doWhileActivity.Body == null) return parentNodeId;

            var bodyNodeId = ProcessActivity(doWhileActivity.Body, graphData, activityNodeMap, parentNodeId);
            // Create loop back edge
            graphData.Edges.Add(CreateEdge(bodyNodeId, parentNodeId, "Loop"));
            return bodyNodeId;
        }

        private string ProcessTryCatchActivity(TryCatch tryCatchActivity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            // Clear and reuse the temp collection
            _tempEndNodes.Clear();

            // Process Try block
            if (tryCatchActivity.Try != null)
            {
                var tryNodeId = ProcessActivity(tryCatchActivity.Try, graphData, activityNodeMap, parentNodeId);
                _tempEndNodes.Add(tryNodeId);
            }

            // Process Catch blocks - commented out in original, keeping as is
            // Process Catch blocks
            //foreach (var catchBlock in tryCatchActivity.Catches)
            //{
                //if (catchBlock.Handler != null)
                //{
                //    var catchNodeId = ProcessActivity(catchBlock.Handler, graphData, activityNodeMap, parentNodeId);
                //    endNodes.Add(catchNodeId);
                //}
            //}

            // Process Finally block
            if (tryCatchActivity.Finally != null)
            {
                var finallyNodeId = ProcessActivity(tryCatchActivity.Finally, graphData, activityNodeMap, parentNodeId);
                _tempEndNodes.Add(finallyNodeId);
            }

            return _tempEndNodes.Count > 0 ? _tempEndNodes[^1] : parentNodeId;
        }

        private string ProcessParallelActivity(Parallel parallelActivity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            // Clear and reuse the temp collection
            _tempEndNodes.Clear();

            var branches = parallelActivity.Branches;
            for (int i = 0; i < branches.Count; i++)
            {
                var branchNodeId = ProcessActivity(branches[i], graphData, activityNodeMap, parentNodeId);
                _tempEndNodes.Add(branchNodeId);
            }

            return _tempEndNodes.Count > 0 ? _tempEndNodes[^1] : parentNodeId;
        }

        private string ProcessGenericActivity(Activity activity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            // Use cached child activities
            GetChildActivities(activity, _tempChildActivities);

            var currentNodeId = parentNodeId;
            for (int i = 0; i < _tempChildActivities.Count; i++)
            {
                currentNodeId = ProcessActivity(_tempChildActivities[i], graphData, activityNodeMap, currentNodeId);
            }

            return currentNodeId;
        }


        private static void GetChildActivities(Activity activity, List<Activity> children)
        {
            children.Clear();
            var activityType = activity.GetType();

            // Use cached properties to avoid repeated reflection
            if (!_childActivityPropertiesCache.TryGetValue(activityType, out var properties))
            {
                properties = activityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                _childActivityPropertiesCache[activityType] = properties;
            }

            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];

                if (typeof(Activity).IsAssignableFrom(prop.PropertyType))
                {
                    if (prop.GetValue(activity) is Activity childActivity)
                    {
                        children.Add(childActivity);
                    }
                }
                else if (typeof(ICollection<Activity>).IsAssignableFrom(prop.PropertyType) && prop.GetValue(activity) is ICollection<Activity> childActivities)
                {
                    children.AddRange(childActivities);
                }

            }
        }

        private Cell CreateStartNode()
        {
            return new Cell
            {
                id = GenerateNodeId(),
                shape = Constants.RECT,
                position = new Position(_currentX, _currentY),
                label = Constants.START,
                data = new Dictionary<string, object> { [Constants.TYPE] = Constants.START }
            };
        }


        private Cell CreateActivityNode(Activity activity, string nodeId)
        {
            var cell = new Cell { id = nodeId, data = new Dictionary<string, object>() };
            var activityType = activity.GetType();

            // Use direct type comparison instead of typeof() for better performance
            if (activity is DsfDotNetMultiAssignActivity multiAssign)
            {
                multiAssign.ToX6Json(cell);
            }
            else if (activity is DsfDecision decision)
            {
                return CreateDecisionNode(decision, nodeId);
            }

            cell.shape = Constants.RECT;
            cell.position = new Position(_currentX, _currentY);
            cell.label = GetActivityLabel(activity);
            cell.data.Add(Constants.TYPE, activityType);
            cell.data.Add(Constants.DISPLAYNAME, activity.DisplayName);
            cell.data.Add(Constants.PROPERTIES, ExtractActivityProperties(activity));

            return cell;
        }

        private Cell CreateDecisionNode(FlowDecision decision, string nodeId)
        {
            var parser = new ActivityParser();
            var dsfDecision = parser.ParseDsfDecisionOnly(decision, new List<IDev2Activity>()) ??
                             new DsfDecision { Conditions = new Dev2DecisionStack { TheStack = new List<Dev2Decision>() } };

            return CreateDecisionNode(dsfDecision, nodeId);
        }

        private Cell CreateDecisionNode(DsfDecision dsfDecision, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                shape = Constants.RECT,
                position = new Position(_currentX, _currentY),
                label = dsfDecision.DisplayName,
                data = new Dictionary<string, object>()
            };

            dsfDecision.ToX6Json(cell);
            return cell;
        }

        private Cell CreateSwitchNode(FlowSwitch<object> flowSwitch, string nodeId)
        {

            return new Cell
            {
                id = nodeId,
                shape = Constants.POLYGON,
                position = new Position(_currentX, _currentY),
                label = Constants.SWITCH,
                data = new Dictionary<string, object>
                {
                    [Constants.TYPE] = Constants.FLOWSWITCH,
                    [Constants.EXPRESSION] = flowSwitch.Expression?.ToString() ?? Constants.SWITCH
                }

            };
        }

        private static Cell CreateEdge(string sourceId, string targetId, string label = "")
        {
            return new Cell
            {
                id = GenerateNodeId(),
                Source = new Connector(sourceId),
                Target = new Connector(targetId),
                label = label,
                data = new Dictionary<string, object>
                {
                    [Constants.TYPE] = Constants.SEQUENCE
                }
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetActivityLabel(Activity activity)
        {
            return !string.IsNullOrEmpty(activity.DisplayName)
                ? activity.DisplayName
                : activity.GetType().Name.Replace(Constants.ACTIVITY, "");
        }

        private static object ExtractActivityProperties(Activity activity)
        {
            var properties = new Dictionary<string, object>
            {
                [Constants.DISPLAYNAME] = activity.DisplayName,
                [Constants.ID] = activity.Id
            };

            var activityType = activity.GetType();

            // Use cached properties
            if (!_typePropertyCache.TryGetValue(activityType, out var props))
            {
                props = activityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead &&
                               !p.PropertyType.IsSubclassOf(typeof(Activity)) &&
                               !typeof(ICollection<Activity>).IsAssignableFrom(p.PropertyType))
                    .ToArray();
                _typePropertyCache[activityType] = props;
            }

            for (int i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                try
                {
                    var value = prop.GetValue(activity);
                    if (value != null && IsSerializable(value))
                    {
                        properties[prop.Name] = value.ToString();
                    }
                }
                catch
                {
                    // Ignore property access errors
                }
            }

            return properties;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsSerializable(object value)
        {
            var type = value.GetType();
            return type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) ||
                   type == typeof(decimal) || type.IsEnum;
        }

        private static string GetFirstNodeId(FlowNode flowNode, Dictionary<Activity, string> activityNodeMap)
        {
            return flowNode switch
            {
                FlowStep flowStep when activityNodeMap.ContainsKey(flowStep.Action) => activityNodeMap[flowStep.Action],
                _ => GenerateNodeId()
            };
        }
    }

}
