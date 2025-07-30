using Dev2.Common.X6;
using Dev2.Data.SystemTemplates.Models;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Resource.Errors;

namespace Dev2.Activities.WF
{
    public class WorkflowToX6Converter
    {
        private int _currentX = 100;
        private int _currentY = 100;

        public string ConvertToX6Json(ActivityBuilder workflow, string xml)
        {

            var graphData = new X6WorkflowLoadModel { WorkflowXml = xml };
            //var graphData = new X6GraphData();
            var activityNodeMap = new Dictionary<Activity, string>();

            var startNode = CreateStartNode();
            graphData.Nodes.Add(startNode);

            var previousNodeId = startNode.id;

            if (workflow.Implementation != null)
            {
                ProcessActivity(workflow.Implementation, graphData, activityNodeMap, previousNodeId);
            }

            return JsonConvert.SerializeObject(graphData);
        }

        private string ProcessActivity(Activity activity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            if (activity == null) return previousNodeId;

            string nodeId;


            if (!(activity is Flowchart))
            {
                nodeId = Guid.NewGuid().ToString();
                activityNodeMap[activity] = nodeId;
                var node = CreateActivityNode(activity, nodeId);

                graphData.Nodes.Add(node);

                if (!string.IsNullOrEmpty(previousNodeId))
                {
                    graphData.Edges.Add(CreateEdge(previousNodeId, nodeId));
                }
            }
            else
                nodeId = previousNodeId;


            switch (activity)
            {
                case Sequence sequence:
                    return ProcessSequence(sequence, graphData, activityNodeMap, nodeId);

                case Flowchart flowchart:
                    return ProcessFlowchart(flowchart, graphData, activityNodeMap, nodeId);

                case If ifActivity:
                    return ProcessIfActivity(ifActivity, graphData, activityNodeMap, nodeId);

                case While whileActivity:
                    return ProcessWhileActivity(whileActivity, graphData, activityNodeMap, nodeId);

                //case System.Activities.Statements.ForEach<> forEachActivity:
                //    return ProcessForEachActivity(forEachActivity, graphData, activityNodeMap, nodeId);

                case DoWhile doWhileActivity:
                    return ProcessDoWhileActivity(doWhileActivity, graphData, activityNodeMap, nodeId);

                case TryCatch tryCatchActivity:
                    return ProcessTryCatchActivity(tryCatchActivity, graphData, activityNodeMap, nodeId);

                case Parallel parallelActivity:
                    return ProcessParallelActivity(parallelActivity, graphData, activityNodeMap, nodeId);

                default:
                    return ProcessGenericActivity(activity, graphData, activityNodeMap, nodeId);
            }
        }

        private string ProcessSequence(Sequence sequence, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            var currentNodeId = parentNodeId;

            foreach (var childActivity in sequence.Activities)
            {
                currentNodeId = ProcessActivity(childActivity, graphData, activityNodeMap, currentNodeId);
            }

            return currentNodeId;
        }

        private string ProcessFlowchart(Flowchart flowchart, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            var processedNodes = new List<string>();

            if (flowchart.StartNode != null)
            {
                var startNodeId = ProcessFlowNode(flowchart.StartNode, graphData, activityNodeMap, parentNodeId);
                processedNodes.Add(startNodeId);
            }

            return processedNodes.LastOrDefault() ?? parentNodeId;
        }

        private string ProcessFlowNode(FlowNode flowNode, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            switch (flowNode)
            {
                case FlowStep flowStep:
                    return ProcessFlowStep(flowStep, graphData, activityNodeMap, previousNodeId);

                case FlowDecision flowDecision:
                    return ProcessFlowDecision(flowDecision, graphData, activityNodeMap, previousNodeId);

                case FlowSwitch<object> flowSwitch:
                    return ProcessFlowSwitch(flowSwitch, graphData, activityNodeMap, previousNodeId);

                default:
                    // Handle generic FlowSwitch case by checking the type dynamically
                    var flowNodeType = flowNode.GetType();
                    if (flowNodeType.IsGenericType && flowNodeType.GetGenericTypeDefinition() == typeof(FlowSwitch<>))
                    {
                        // Convert to FlowSwitch<object> for processing
                        var switchNode = ConvertToObjectSwitch(flowNode);
                        if (switchNode != null)
                        {
                            return ProcessFlowSwitch(switchNode, graphData, activityNodeMap, previousNodeId);
                        }
                    }
                    return previousNodeId;
            }
        }

        private static FlowSwitch<object> ConvertToObjectSwitch(FlowNode flowNode)
        {
            try
            {
                // Use reflection to access the properties of the generic FlowSwitch
                var type = flowNode.GetType();
                var expressionProperty = type.GetProperty("Expression");
                var casesProperty = type.GetProperty("Cases");
                var defaultProperty = type.GetProperty("Default");

                if (expressionProperty == null || casesProperty == null)
                    return null;

                var expression = expressionProperty.GetValue(flowNode) as Activity;
                var cases = casesProperty.GetValue(flowNode);
                var defaultCase = defaultProperty?.GetValue(flowNode) as FlowNode;

                var objectSwitch = new FlowSwitch<object>
                {
                    Expression = expression,
                    Default = defaultCase
                };

                // Copy cases using reflection
                if (cases is System.Collections.IDictionary casesDictionary)
                {
                    foreach (System.Collections.DictionaryEntry entry in casesDictionary)
                    {
                        if (entry.Key != null && entry.Value is FlowNode flowNodeValue)
                        {
                            objectSwitch.Cases[entry.Key] = flowNodeValue;
                        }
                    }
                }

                return objectSwitch;
            }
            catch
            {
                return null;
            }
        }

        private string ProcessFlowStep(FlowStep flowStep, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            var nodeId = ProcessActivity(flowStep.Action, graphData, activityNodeMap, previousNodeId);

            if (flowStep.Next != null)
            {
                ProcessFlowNode(flowStep.Next, graphData, activityNodeMap, nodeId);
                return nodeId;
            }

            return nodeId;
        }

        private string ProcessFlowDecision(FlowDecision flowDecision, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            var decisionNodeId = Guid.NewGuid().ToString();
            var decisionNode = CreateDecisionNode(flowDecision, decisionNodeId);
            graphData.Nodes.Add(decisionNode);

            if (!string.IsNullOrEmpty(previousNodeId))
            {
                graphData.Edges.Add(CreateEdge(previousNodeId, decisionNodeId));
            }

            var endNodes = new List<string>();

            // Process True branch
            if (flowDecision.True != null)
            {
                var trueNodeId = ProcessFlowNode(flowDecision.True, graphData, activityNodeMap, null);
                graphData.Edges.Add(CreateEdge(decisionNodeId,
                    activityNodeMap.ContainsValue(trueNodeId) ? trueNodeId : GetFirstNodeId(flowDecision.True, activityNodeMap),
                    Constants.TRUE));
                endNodes.Add(trueNodeId);
            }

            // Process False branch
            if (flowDecision.False != null)
            {
                var falseNodeId = ProcessFlowNode(flowDecision.False, graphData, activityNodeMap, null);
                graphData.Edges.Add(CreateEdge(decisionNodeId,
                    activityNodeMap.ContainsValue(falseNodeId) ? falseNodeId : GetFirstNodeId(flowDecision.False, activityNodeMap),
                    Constants.FALSE));
                endNodes.Add(falseNodeId);
            }

            return decisionNodeId;
        }

        private string ProcessFlowSwitch(FlowSwitch<object> flowSwitch, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            var switchNodeId = Guid.NewGuid().ToString();
            var switchNode = CreateSwitchNode(flowSwitch, switchNodeId);
            
            // Store the switch activity in the activity map if it has an expression
            if (flowSwitch.Expression != null)
            {
                activityNodeMap[flowSwitch.Expression] = switchNodeId;
            }
            
            graphData.Nodes.Add(switchNode);

            if (!string.IsNullOrEmpty(previousNodeId))
            {
                graphData.Edges.Add(CreateEdge(previousNodeId, switchNodeId));
            }

            // Process each case
            foreach (var caseItem in flowSwitch.Cases)
            {
                if (caseItem.Value != null)
                {
                    var caseNodeId = ProcessFlowNode(caseItem.Value, graphData, activityNodeMap, null);
                    var targetNodeId = GetTargetNodeId(caseItem.Value, activityNodeMap, caseNodeId);
                    graphData.Edges.Add(CreateEdge(switchNodeId, targetNodeId, caseItem.Key?.ToString() ?? "Case"));
                }
            }

            // Process default case
            if (flowSwitch.Default != null)
            {
                var defaultNodeId = ProcessFlowNode(flowSwitch.Default, graphData, activityNodeMap, null);
                var targetNodeId = GetTargetNodeId(flowSwitch.Default, activityNodeMap, defaultNodeId);
                graphData.Edges.Add(CreateEdge(switchNodeId, targetNodeId, "Default"));
            }

            return switchNodeId;
        }

        private static string GetTargetNodeId(FlowNode flowNode, Dictionary<Activity, string> activityNodeMap, string fallbackNodeId)
        {
            // First try to get the node ID from the activity map
            if (flowNode is FlowStep flowStep && flowStep.Action != null && activityNodeMap.ContainsKey(flowStep.Action))
            {
                return activityNodeMap[flowStep.Action];
            }
            
            if (flowNode is FlowDecision flowDecision && flowDecision.Condition != null && activityNodeMap.ContainsKey(flowDecision.Condition))
            {
                return activityNodeMap[flowDecision.Condition];
            }
            
            if (flowNode is FlowSwitch<object> flowSwitch && flowSwitch.Expression != null && activityNodeMap.ContainsKey(flowSwitch.Expression))
            {
                return activityNodeMap[flowSwitch.Expression];
            }
            
            return fallbackNodeId;
        }

        private string ProcessIfActivity(If ifActivity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            var endNodes = new List<string> { parentNodeId };

            // Process Then branch
            if (ifActivity.Then != null)
            {
                var thenNodeId = ProcessActivity(ifActivity.Then, graphData, activityNodeMap, parentNodeId);
                endNodes.Add(thenNodeId);
            }

            // Process Else branch
            if (ifActivity.Else != null)
            {
                var elseNodeId = ProcessActivity(ifActivity.Else, graphData, activityNodeMap, parentNodeId);
                endNodes.Add(elseNodeId);
            }

            return endNodes.Last();
        }

        private string ProcessWhileActivity(While whileActivity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            if (whileActivity.Body != null)
            {
                var bodyNodeId = ProcessActivity(whileActivity.Body, graphData, activityNodeMap, parentNodeId);
                // Create loop back edge
                graphData.Edges.Add(CreateEdge(bodyNodeId, parentNodeId, "Loop"));
                return bodyNodeId;
            }

            return parentNodeId;
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
            if (doWhileActivity.Body != null)
            {
                var bodyNodeId = ProcessActivity(doWhileActivity.Body, graphData, activityNodeMap, parentNodeId);
                // Create loop back edge
                graphData.Edges.Add(CreateEdge(bodyNodeId, parentNodeId, "Loop"));
                return bodyNodeId;
            }

            return parentNodeId;
        }

        private string ProcessTryCatchActivity(TryCatch tryCatchActivity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            var endNodes = new List<string>();

            // Process Try block
            if (tryCatchActivity.Try != null)
            {
                var tryNodeId = ProcessActivity(tryCatchActivity.Try, graphData, activityNodeMap, parentNodeId);
                endNodes.Add(tryNodeId);
            }

            // Process Catch blocks
            foreach (var catchBlock in tryCatchActivity.Catches)
            {
                //if (catchBlock.Handler != null)
                //{
                //    var catchNodeId = ProcessActivity(catchBlock.Handler, graphData, activityNodeMap, parentNodeId);
                //    endNodes.Add(catchNodeId);
                //}
            }

            // Process Finally block
            if (tryCatchActivity.Finally != null)
            {
                var finallyNodeId = ProcessActivity(tryCatchActivity.Finally, graphData, activityNodeMap, parentNodeId);
                endNodes.Add(finallyNodeId);
            }

            return endNodes.LastOrDefault() ?? parentNodeId;
        }

        private string ProcessParallelActivity(Parallel parallelActivity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            var endNodes = new List<string>();

            foreach (var branch in parallelActivity.Branches)
            {
                var branchNodeId = ProcessActivity(branch, graphData, activityNodeMap, parentNodeId);
                endNodes.Add(branchNodeId);
            }

            return endNodes.LastOrDefault() ?? parentNodeId;
        }

        private string ProcessGenericActivity(Activity activity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            // Handle activities with child activities using reflection
            var childActivities = GetChildActivities(activity);
            var currentNodeId = parentNodeId;

            foreach (var child in childActivities)
            {
                currentNodeId = ProcessActivity(child, graphData, activityNodeMap, currentNodeId);
            }

            return currentNodeId;
        }

        private static List<Activity> GetChildActivities(Activity activity)
        {
            var children = new List<Activity>();

            // Use reflection to find child activities
            var properties = activity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (typeof(Activity).IsAssignableFrom(prop.PropertyType))
                {
                    var childActivity = prop.GetValue(activity) as Activity;
                    if (childActivity != null)
                    {
                        children.Add(childActivity);
                    }
                }
                else if (typeof(ICollection<Activity>).IsAssignableFrom(prop.PropertyType))
                {
                    var childActivities = prop.GetValue(activity) as ICollection<Activity>;
                    if (childActivities != null)
                    {
                        children.AddRange(childActivities);
                    }
                }
            }

            return children;
        }

        private Cell CreateStartNode()
        {
            var node = new Cell
            {
                id = Guid.NewGuid().ToString(),
                shape = Constants.RECT,
                position = new Position(_currentX, _currentY),
                label = Constants.START,
                data = new Dictionary<string, object> { [Constants.TYPE] = Constants.START }
            };
            
            // Update position for next node
            _currentY += 150;
            
            return node;
        }

        private Cell CreateActivityNode(Activity activity, string nodeId)
        {
            var cell = new Cell { id = nodeId, data = new Dictionary<string, object>() };
            var activityType = activity.GetType();
            if (activityType == typeof(DsfDotNetMultiAssignActivity))
            {
                var p = (DsfDotNetMultiAssignActivity)activity;
                p.ToX6Json(cell);
            }
            else if (activityType == typeof(DsfDecision))
            {
                var p = (DsfDecision)activity;
                return CreateDecisionNode(p, nodeId);
            }

            cell.shape = Constants.RECT;
            cell.position = new Position(_currentX, _currentY);
            cell.label = GetActivityLabel(activity);
            cell.data.Add(Constants.TYPE, activityType);
            cell.data.Add(Constants.DISPLAYNAME, activity.DisplayName);
            cell.data.Add(Constants.PROPERTIES, ExtractActivityProperties(activity));
            
            // Update position for next node
            _currentY += 150;
            
            return cell;
        }

        private Cell CreateDecisionNode(FlowDecision decision, string nodeId)
        {
            var parser = new ActivityParser();
            var dsfDecision = parser.ParseDsfDecisionOnly(decision, new List<IDev2Activity>()) ?? new DsfDecision();
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
            var cell = new Cell
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

            // Update position for next node
            _currentY += 150;

            // If the expression is a DsfFlowSwitchActivity, extract more detailed information
            // Note: Expression is Activity<object>, but DsfFlowSwitchActivity inherits from DsfFlowNodeActivity<string>
            if (flowSwitch.Expression != null)
            {
                // Try to access the underlying activity through reflection if needed
                var expression = flowSwitch.Expression;
                if (expression.GetType().Name.Contains(nameof(DsfFlowSwitchActivity)))
                {
                    ProcessSwitchActivityReflection(expression, flowSwitch, cell);
                }
                else
                {
                    // Basic processing for other expression types
                    cell.data[Constants.DISPLAYNAME] = expression.DisplayName ?? Constants.SWITCH;
                    cell.label = expression.DisplayName ?? Constants.SWITCH;
                }
            }

            return cell;
        }

        private static void ProcessSwitchActivityReflection(Activity expression, FlowSwitch<object> flowSwitch, Cell cell)
        {
            try
            {
                // Use reflection to access DsfFlowSwitchActivity properties
                var type = expression.GetType();
                
                var displayNameProperty = type.GetProperty("DisplayName");
                var expressionTextProperty = type.GetProperty("ExpressionText");
                var uniqueIdProperty = type.GetProperty("UniqueID");

                var displayName = displayNameProperty?.GetValue(expression) as string ?? Constants.SWITCH;
                var expressionText = expressionTextProperty?.GetValue(expression) as string;
                var uniqueId = uniqueIdProperty?.GetValue(expression) as string;

                cell.data[Constants.DISPLAYNAME] = displayName;
                cell.label = displayName;
                
                if (!string.IsNullOrEmpty(expressionText))
                {
                    cell.data["switchExpression"] = CreateSwitchExpressionJson(expressionText, flowSwitch);
                }
                
                if (!string.IsNullOrEmpty(uniqueId))
                {
                    cell.data["UniqueID"] = uniqueId;
                }
            }
            catch
            {
                // Fallback to basic processing
                cell.data[Constants.DISPLAYNAME] = expression.DisplayName ?? Constants.SWITCH;
                cell.label = expression.DisplayName ?? Constants.SWITCH;
            }
        }

        private static string CreateSwitchExpressionJson(string expressionText, FlowSwitch<object> flowSwitch)
        {
            try
            {
                var switchExpression = new
                {
                    SwitchVariable = ExtractSwitchVariable(expressionText),
                    Cases = flowSwitch.Cases.Select(c => new { Key = c.Key?.ToString(), Value = c.Key?.ToString() }).ToList(),
                    DefaultCase = flowSwitch.Default != null ? "Default" : null
                };

                return JsonConvert.SerializeObject(switchExpression);
            }
            catch
            {
                // If serialization fails, return a basic expression
                return JsonConvert.SerializeObject(new { SwitchVariable = "variable", Cases = new object[0] });
            }
        }

        private static string ExtractSwitchVariable(string expressionText)
        {
            if (string.IsNullOrEmpty(expressionText))
                return "variable";

            // Try to extract variable name from expression like: Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData("[[hello]]",AmbientDataList)
            var match = System.Text.RegularExpressions.Regex.Match(expressionText, @"\[\[([^\]]+)\]\]");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // Fallback to basic extraction
            var startIndex = expressionText.IndexOf("\"[[") + 3;
            var endIndex = expressionText.IndexOf("]]\"");
            if (startIndex > 2 && endIndex > startIndex)
            {
                return expressionText.Substring(startIndex, endIndex - startIndex);
            }

            return "variable";
        }

        private static Cell CreateEdge(string sourceId, string targetId, string label = "")
        {
            return new Cell
            {
                id = Guid.NewGuid().ToString(),
                Source = new Connector(sourceId),
                Target = new Connector(targetId),
                label = label,
                data = new Dictionary<string, object>
                {
                    [Constants.TYPE] = Constants.SEQUENCE
                }
            };
        }

        private static string GetActivityLabel(Activity activity)
        {
            if (!string.IsNullOrEmpty(activity.DisplayName))
                return activity.DisplayName;

            return activity.GetType().Name.Replace(Constants.ACTIVITY, "");
        }

        private static string GetDecisionLabel(FlowDecision decision)
        {
            return decision.Condition?.ToString() ?? Constants.DECISION;
        }

        private static object ExtractActivityProperties(Activity activity)
        {
            var properties = new Dictionary<string, object>();

            // Extract common properties
            properties[Constants.DISPLAYNAME] = activity.DisplayName;
            properties[Constants.ID] = activity.Id;

            // Extract activity-specific properties using reflection
            var activityType = activity.GetType();
            var props = activityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && !p.PropertyType.IsSubclassOf(typeof(Activity)) &&
                           !typeof(ICollection<Activity>).IsAssignableFrom(p.PropertyType));

            foreach (var prop in props)
            {
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
                }
            }

            return properties;
        }

        private static bool IsSerializable(object value)
        {
            var type = value.GetType();
