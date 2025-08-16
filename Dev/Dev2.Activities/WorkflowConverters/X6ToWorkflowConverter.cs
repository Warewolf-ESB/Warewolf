using Dev2.Common.X6;
using Dev2.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Common;

namespace Dev2.Activities.WF
{
    /// <summary>
    /// Creates Workflow from X6 Json data (nodes, edges and common data) 
    /// </summary>
    public class X6ToWorkflowConverter
    {
        private Dictionary<string, Activity> activityMap = new();
        private List<Cell> connections = new List<Cell>();
        private Dictionary<string, SwitchCaseData> switchCaseMap = new();
        
        // Store ForEach nesting information for activities
        private Dictionary<string, ForEachNestingInfo> forEachNestingMap = new();

        private class SwitchCaseData
        {
            public List<SwitchCase> Cases { get; set; } = new();
            public string DefaultCase { get; set; }
        }

        private class SwitchCase
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
        
        /// <summary>
        /// Stores information about an activity's nesting within ForEach activities
        /// </summary>
        private class ForEachNestingInfo
        {
            public bool IsNestedInForEach { get; set; }
            public string ForEachParentId { get; set; }
        }

        /// <summary>
        /// Converts X6 Json to Xaml representing a workflow
        /// </summary>
        /// <param name="values">Dictionary containing ResourceJSON</param>
        /// <returns></returns>
        public static StringBuilder X6JsonToXaml(Dictionary<string, System.Text.StringBuilder> values)
        {
            values.TryGetValue("ResourceJSON", out StringBuilder resourceDefinition);

            if (resourceDefinition != null && resourceDefinition.Length > 0)
            {
                var xaml = new X6ToWorkflowConverter().X6JsonToWorkflow(resourceDefinition.ToString());
                xaml = AddReplaceNameSpace(xaml);
                return xaml;
            }

            return new StringBuilder();
        }

        /// <summary>
        /// Converts X6 Json to Xaml representing a workflow
        /// </summary>
        /// <param name="x6Json"></param>
        /// <returns>StringBuilder</returns>
        public StringBuilder X6JsonToWorkflow(string x6Json)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                    FloatParseHandling = FloatParseHandling.Decimal,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                var x6Graph = JsonConvert.DeserializeObject<X6WorkflowSaveModel>(x6Json, settings);

                var activityBuilder = X6JsonToActivityBuilder(x6Graph);
                var flowChart = activityBuilder.Implementation as Flowchart;
                var workflowHelper = new WorkflowHelper();
                workflowHelper.EnsureImplementation(activityBuilder, flowChart);
                var workflowXaml = workflowHelper.GetXamlDefinition(activityBuilder);
                return workflowXaml;
            }
            catch (Exception ex)
            {
				Dev2Logger.Error("Failed to convert X6 JSON to workflow", ex, GlobalConstants.WarewolfError);
				throw;
			}
        }

        /// <summary>
        /// Builds a workflow from X6WorkflowSaveModel and returns ActivityBuilder
        /// </summary>
        /// <param name="x6Graph">X6WorkflowSaveModel representing workflow data in X6</param>
        /// <returns>ActivityBuilder</returns>
        private ActivityBuilder X6JsonToActivityBuilder(X6WorkflowSaveModel x6Graph)
        {
            var workflowName = x6Graph.ResourceName ?? "ConvertedWorkflow";
            var activityBuilder = new ActivityBuilder
            {
                Name = workflowName
            };

            // Separate nodes and edges
            var allNodes = x6Graph.Cells.Where(c => c.shape != "edge").ToList();
            connections = x6Graph.Cells.Where(c => c.shape == "edge").ToList();
            
            // Apply ForEach ID mapping to fix parent relationship references
            ApplyForEachIdMapping(allNodes);
            
            // Process ForEach nesting information first
            ProcessForEachNestingInfo(allNodes);
            
            // Filter out child nodes that belong to ForEach activities' droppedNodes
            var topLevelNodes = FilterTopLevelNodes(allNodes);
            
            Cell startcell = null;

            ProcessSwitchCaseData(topLevelNodes);

            foreach (var node in topLevelNodes)
            {
                var activity = CreateActivityFromNode(node, out bool isStartNode);
                if (activity != null)
                {
                    activityMap[node.id] = activity;

                    if (isStartNode)
                    {
                        startcell = node;
                    }
                }
            }

            // Now embed nested activities into their parent ForEach activities' DataFunc.Handler property
            EmbedNestedActivitiesIntoForEachActivities(allNodes);

            // Build the workflow structure
            activityBuilder.Implementation = BuildWorkflow(topLevelNodes, startcell);

            return activityBuilder;
        }
        
        /// <summary>
        /// Embeds nested activities into their parent ForEach activities' DataFunc.Handler property
        /// </summary>
        /// <param name="allNodes">All nodes from the X6 graph</param>
        private void EmbedNestedActivitiesIntoForEachActivities(List<Cell> allNodes)
        {
            // Group nested nodes by their parent ForEach ID
            var nestedNodesByParent = new Dictionary<string, List<Cell>>();
            
            foreach (var node in allNodes)
            {
                if (forEachNestingMap.TryGetValue(node.id, out var nestingInfo) && 
                    nestingInfo.IsNestedInForEach && !string.IsNullOrEmpty(nestingInfo.ForEachParentId))
                {
                    if (!nestedNodesByParent.ContainsKey(nestingInfo.ForEachParentId))
                    {
                        nestedNodesByParent[nestingInfo.ForEachParentId] = new List<Cell>();
                    }
                    nestedNodesByParent[nestingInfo.ForEachParentId].Add(node);
                }
            }
            
            // For each ForEach activity, embed its nested activities
            foreach (var kvp in nestedNodesByParent)
            {
                var forEachParentId = kvp.Key;
                var nestedNodes = kvp.Value;
                
                // Find the ForEach activity with this ID and ensure it's a DsfForEachActivity
                if (activityMap.TryGetValue(forEachParentId, out var forEachActivity) && 
                    forEachActivity is DsfForEachActivity forEach && 
                    nestedNodes.Count > 0)
                {
                    // For simplicity, we'll take the first nested activity
                    // In a more complex scenario, you might need to handle multiple nested activities
                    var nestedNode = nestedNodes[0]; // Take first nested activity
                    var nestedActivity = CreateActivityFromNode(nestedNode, out _);
                    
                    if (nestedActivity != null)
                    {
                        // Initialize DataFunc if it doesn't exist
                        if (forEach.DataFunc == null)
                        {
                            forEach.DataFunc = new ActivityFunc<string, bool>
                            {
                                DisplayName = "Data Action",
                                Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}")
                            };
                        }
                        
                        // Set the nested activity as the handler
                        forEach.DataFunc.Handler = nestedActivity;
                    }
                }
            }
        }
        
        /// <summary>
        /// Processes and stores ForEach nesting information for all nodes
        /// </summary>
        /// <param name="allNodes">All nodes from the X6 graph</param>
        private void ProcessForEachNestingInfo(List<Cell> allNodes)
        {
            foreach (var node in allNodes)
            {
                var nestingInfo = new ForEachNestingInfo();
                
                // Extract isNestedInForEach property
                if (node.data.TryGetValue("isNestedInForEach", out var isNestedObj) && 
                    bool.TryParse(isNestedObj?.ToString(), out var isNested))
                {
                    nestingInfo.IsNestedInForEach = isNested;
                }
                
                // Extract forEachParentId property
                if (node.data.TryGetValue("forEachParentId", out var parentIdObj) && 
                    parentIdObj is string parentId && !string.IsNullOrWhiteSpace(parentId))
                {
                    nestingInfo.ForEachParentId = parentId;
                }
                
                // Only store if we have meaningful nesting information
                if (nestingInfo.IsNestedInForEach || !string.IsNullOrEmpty(nestingInfo.ForEachParentId))
                {
                    forEachNestingMap[node.id] = nestingInfo;
                }
            }
        }
        
        /// <summary>
        /// Filters out child nodes that belong to ForEach activities' droppedNodes
        /// and also filters out nodes that are nested in ForEach activities (have isNestedInForEach = true)
        /// to prevent them from being processed as separate top-level activities
        /// </summary>
        /// <param name="allNodes">All nodes from the X6 graph</param>
        /// <returns>List of nodes that should be processed as top-level activities</returns>
        private static List<Cell> FilterTopLevelNodes(List<Cell> allNodes)
        {
            var childNodeIds = new HashSet<string>();
            var nestedNodeIds = new HashSet<string>();
            
            // First pass: identify all child node IDs that are embedded in ForEach droppedNodes
            foreach (var node in allNodes)
            {
                if (IsForEachNode(node))
                {
                    var droppedNodeIds = ExtractDroppedNodeIds(node);
                    foreach (var childId in droppedNodeIds)
                    {
                        childNodeIds.Add(childId);
                    }
                }
                
                // Also identify nodes that are nested in ForEach activities
                if (node.data.TryGetValue("isNestedInForEach", out var isNestedObj) && 
                    bool.TryParse(isNestedObj?.ToString(), out var isNested) && isNested)
                {
                    nestedNodeIds.Add(node.id);
                }
            }
            
            // Second pass: filter out child nodes and nested nodes, keeping only top-level nodes
            return allNodes.Where(node => !childNodeIds.Contains(node.id) && !nestedNodeIds.Contains(node.id)).ToList();
        }
        
        /// <summary>
        /// Checks if a node represents a ForEach activity
        /// </summary>
        /// <param name="node">The node to check</param>
        /// <returns>True if the node is a ForEach activity</returns>
        private static bool IsForEachNode(Cell node)
        {
            if (!node.data.TryGetValue("type", out var typeObj) || typeObj is not string type)
                return false;
                
            var nodeType = type.ToLowerInvariant();
            return nodeType.Contains("dsfforeachactivity") || nodeType.Contains("foreach");
        }
        
        /// <summary>
        /// Extracts the IDs of child nodes from a ForEach activity's droppedNodes
        /// </summary>
        /// <param name="forEachNode">The ForEach node</param>
        /// <returns>List of child node IDs</returns>
        private static List<string> ExtractDroppedNodeIds(Cell forEachNode)
        {
            var childIds = new List<string>();
            
            try
            {
                if (forEachNode.data.TryGetValue("droppedNodes", out var droppedNodesObj))
                {
                    List<object> droppedNodesList = null;
                    
                    if (droppedNodesObj is JArray jArray)
                    {
                        droppedNodesList = jArray.ToObject<List<object>>();
                    }
                    else if (droppedNodesObj != null)
                    {
                        var json = JsonConvert.SerializeObject(droppedNodesObj);
                        droppedNodesList = JsonConvert.DeserializeObject<List<object>>(json);
                    }
                    
                    if (droppedNodesList != null)
                    {
                        foreach (var droppedNode in droppedNodesList)
                        {
                            var childId = ExtractNodeId(droppedNode);
                            if (!string.IsNullOrEmpty(childId))
                            {
                                childIds.Add(childId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"Error extracting dropped node IDs from ForEach: {ex.Message}", ex, GlobalConstants.WarewolfError);
            }
            
            return childIds;
        }
        
        /// <summary>
        /// Extracts the ID from a dropped node object
        /// </summary>
        /// <param name="droppedNode">The dropped node object</param>
        /// <returns>The node ID or null if not found</returns>
        private static string ExtractNodeId(object droppedNode)
        {
            try
            {
                if (droppedNode is string jsonString)
                {
                    var nodeData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                    return nodeData?.TryGetValue("id", out var idObj) == true ? idObj?.ToString() : null;
                }
                else
                {
                    var json = JsonConvert.SerializeObject(droppedNode);
                    var nodeData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    return nodeData?.TryGetValue("id", out var idObj) == true ? idObj?.ToString() : null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Builds workflow from nodes
        /// </summary>
        /// <param name="nodes">X6 Nodes (also known as Cell)</param>
        /// <param name="startcell">Start Node (i.e., X6 Cell)</param>
        /// <returns>Activity</returns>
        private Activity BuildWorkflow(List<Cell> nodes, Cell startcell)
        {
            var sequence = new Sequence();
            var flowchart = new Flowchart();

            if (startcell == null) return sequence;

            // Build flowchart structure
            var flowNodes = new Dictionary<string, FlowNode>();
            FlowStep startFlowNode = null;
            foreach (var node in nodes)
            {
                // Only process nodes that have activities in the activityMap
                if (!activityMap.TryGetValue(node.id, out var action))
                    continue;

                var flowNode = CreateFlowNode(action);

                if (flowNode != null)
                {
                    flowNodes[node.id] = flowNode;

                    if (node.id == startcell.id)
                    {
                        startFlowNode = flowNode as FlowStep;
                    }
                    else
                    {
                        flowchart.Nodes.Add(flowNode);
                    }
                }
            }

            CreateConnections(flowNodes);

            // Set Start Node
            if (startFlowNode != null)
                flowchart.StartNode = startFlowNode.Next ?? startFlowNode;

            return flowchart;
        }

        /// <summary>
        /// Activity Factory: Creates Flow Node from Activity (action) 
        /// </summary>
        /// <param name="action"></param>
        /// <returns>FlowNode</returns>
        private static FlowNode CreateFlowNode(Activity action)
        {
            if (action is DsfFlowDecisionActivity flowAction)
            {
                return new FlowDecision { DisplayName = flowAction.DisplayName, Condition = flowAction };
            }
            else if (action is DsfFlowSwitchActivity switchAction)
            {
                return new FlowSwitch<string> { DisplayName = switchAction.DisplayName, Expression = switchAction };
            }

            return new FlowStep { Action = action };
        }

        /// <summary>
        /// Activity Factory: Creates Activity from X6 Json Cell
        /// </summary>
        /// <param name="node">X6 Json Cell</param>
        /// <param name="isStartNode">flag to indicate if node is a start node</param>
        /// <returns>Activity</returns>
        private Activity CreateActivityFromNode(Cell node, out bool isStartNode)
        {
            isStartNode = false;

            if (!node.data.TryGetValue("type", out var typeObj) || typeObj is not string type || string.IsNullOrWhiteSpace(type))
                return null;

            var nodeType = type.ToLowerInvariant();
            Activity activity = null;
            
            if (nodeType == Constants.START)
            {
                isStartNode = true;
                activity = new WriteLine { Text = "Workflow Start Node" };
            }
            else if (nodeType.Contains("dsfdotnetmultiassignactivity"))
            {
                activity = CreateAssignActivity(node);
            }
            else if (nodeType.Contains("flowdecision"))
            {
                activity = CreateFlowDecisionActivity(node);
            }
            else if (nodeType.Contains("dsfdecision"))
            {
                activity = CreateDecisionActivity(node);
            }
            else if (nodeType.Contains("dsfflowswitchactivity") || nodeType.Contains("flowswitch"))
            {
                activity = CreateSwitchActivity(node);
            }
            else if (nodeType.Contains("dsfforeachactivity") || nodeType.Contains("foreach"))
            {
                activity = CreateForEachActivity(node);
            }
            else
            {
                activity = new WriteLine { Text = "Unknown type" };
            }
            
            // Apply ForEach nesting information to the created activity
            if (activity != null)
            {
                ApplyForEachNestingInfo(activity, node.id);
            }
            
            return activity;
        }
        
        /// <summary>
        /// Applies ForEach nesting information to an activity by storing it in the activity's annotations
        /// </summary>
        /// <param name="activity">The activity to apply nesting info to</param>
        /// <param name="nodeId">The node ID to look up nesting info</param>
        private void ApplyForEachNestingInfo(Activity activity, string nodeId)
        {
            if (forEachNestingMap.TryGetValue(nodeId, out var nestingInfo) && 
                activity.GetType().GetProperty("Annotations") != null)
            {
                // Use reflection to set annotations if the property exists
                var annotationsProperty = activity.GetType().GetProperty("Annotations");
                if (annotationsProperty != null)
                {
                    var annotations = annotationsProperty.GetValue(activity) as System.Collections.ObjectModel.Collection<object>;
                    if (annotations == null)
                    {
                        annotations = new System.Collections.ObjectModel.Collection<object>();
                        annotationsProperty.SetValue(activity, annotations);
                    }
                    
                    // Add our custom nesting information as an annotation
                    var nestingAnnotation = new Dictionary<string, object>
                    {
                        ["isNestedInForEach"] = nestingInfo.IsNestedInForEach,
                        ["forEachParentId"] = nestingInfo.ForEachParentId ?? string.Empty,
                        ["_annotationType"] = nameof(ForEachNestingInfo)
                    };
                    
                    annotations.Add(nestingAnnotation);
                }
            }
        }

        private static DsfFlowSwitchActivity CreateSwitchActivity(Cell node)
        {
            var displayName = "Switch";
            if (node.data.TryGetValue(Constants.DISPLAYTEXT, out var displayObject) && displayObject is string displayText && !string.IsNullOrWhiteSpace(displayText))
            {
                displayName = displayText;
            }

            var activity = new DsfFlowSwitchActivity
            {
                DisplayName = displayName
            };

            // Extract switch variable from node data
            if (node.data.TryGetValue("switchVariable", out var switchVarObj) && switchVarObj is string switchVariable)
            {
                // Create the proper expression text format for switch
                activity.ExpressionText = string.Join("", GlobalConstants.InjectedSwitchDataFetch,
                                                     "(\"", switchVariable, "\",",
                                                     GlobalConstants.InjectedDecisionDataListVariable,
                                                     ")");
            }

            if (node.data.TryGetValue("switchExpression", out var switchExprObj) && switchExprObj is string switchExprJson)
            {
                try
                {
                    var switchExpression = JsonConvert.DeserializeObject<dynamic>(switchExprJson);

                    // Extract switch variable from expression if not already set
                    if (string.IsNullOrEmpty(activity.ExpressionText) && switchExpression?.SwitchVariable != null)
                    {
                        var switchVar = switchExpression.SwitchVariable.ToString();
                        activity.ExpressionText = string.Join("", GlobalConstants.InjectedSwitchDataFetch,
                                                             "(\"", switchVar, "\",",
                                                             GlobalConstants.InjectedDecisionDataListVariable,
                                                             ")");
                    }
                }
                catch (JsonException)
                {
                    // If JSON parsing fails, continue with default values for
                }
            }

            // Set other properties if available
            activity.UniqueID = node.data.TryGetValue("UniqueID", out var uniqueIdObj) && uniqueIdObj is string uniqueId
                ? uniqueId
                : Guid.NewGuid().ToString();

            return activity;
        }


        /// <summary>
        /// Creates FlowDecisionActivity from X6 Node
        /// </summary>
        /// <param name="node">X6 Node</param>
        /// <returns>DsfFlowDecisionActivity</returns>
        private static DsfFlowDecisionActivity CreateFlowDecisionActivity(Cell node)
        {
            if (!node.data.TryGetValue(Constants.DISPLAYTEXT, out var displayObject)
                || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfFlowDecisionActivity();
            activity.FromX6Json(node);
            return activity;
        }

        /// <summary>
        /// Creates DsfDecision from X6 Node
        /// </summary>
        /// <param name="node">X6 Node</param>
        /// <returns>DsfDecision</returns>
        private static DsfDecision CreateDecisionActivity(Cell node)
        {
            if (!node.data.TryGetValue(Constants.DISPLAYTEXT, out var displayObject)
                || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfDecision();
            activity.FromX6Json(node);
            return activity;
        }

        /// <summary>
        /// Creates DsfDotNetMultiAssignActivity from X6 Node
        /// </summary>
        /// <param name="node">X6 Node</param>
        /// <returns>DsfDotNetMultiAssignActivity</returns>
        private static DsfDotNetMultiAssignActivity CreateAssignActivity(Cell node)
        {
            // Try both camelCase and lowercase variations for compatibility
            var hasDisplayName = node.data.TryGetValue("displayName", out var displayObject) ||
                                 node.data.TryGetValue(Constants.DISPLAYNAME, out displayObject);
            
            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfDotNetMultiAssignActivity();
            activity.FromX6Json(node);
            return activity;
        }

        /// <summary>
        /// Creates DsfForEachActivity from X6 Node
        /// </summary>
        /// <param name="node">X6 Node</param>
        /// <returns>DsfForEachActivity</returns>
        private static DsfForEachActivity CreateForEachActivity(Cell node)
        {
            // Try both camelCase and lowercase variations for compatibility
            var hasDisplayName = node.data.TryGetValue("displayName", out var displayObject) ||
                                 node.data.TryGetValue(Constants.DISPLAYNAME, out displayObject);
            
            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfForEachActivity();
            activity.FromX6Json(node);
            return activity;
        }

        /// <summary>
        /// Creates Connections among flow nodes
        /// </summary>
        /// <param name="flowNodes">Flow Nodes</param>
        private void CreateConnections(Dictionary<string, FlowNode> flowNodes)
        {
            // First, create connections from switch case data
            CreateSwitchCaseConnections(flowNodes);

            // Then create regular connections
            foreach (var connection in connections)
            {
                if (connection.Source?.Id is not string sourceId ||
                    connection.Target?.Id is not string targetId ||
                    !flowNodes.TryGetValue(sourceId, out var sourceNode) ||
                    !flowNodes.TryGetValue(targetId, out var targetNode))
                {
                    continue;
                }

                switch (sourceNode)
                {
                    case FlowDecision decision:
                        HandleDecisionConnection(connection, decision, targetNode);
                        break;
                    case FlowSwitch<string> switchNode:
                        HandleSwitchConnection(connection, switchNode, targetNode);
                        break;
                    case FlowStep step:
                        step.Next = targetNode;
                        break;
                }
            }
        }

        private void CreateSwitchCaseConnections(Dictionary<string, FlowNode> flowNodes)
        {
            foreach (var kvp in switchCaseMap)
            {
                if (TryGetSwitchNode(flowNodes, kvp.Key, out var switchNode))
                {
                    ProcessSwitchCases(kvp.Value, kvp.Key, switchNode, flowNodes);
                    ProcessDefaultCase(kvp.Value, kvp.Key, switchNode, flowNodes);
                }
            }
        }

        private static bool TryGetSwitchNode(Dictionary<string, FlowNode> flowNodes, string switchNodeId, out FlowSwitch<string> switchNode)
        {
            switchNode = null;
            return flowNodes.TryGetValue(switchNodeId, out var flowNode) && 
                   (switchNode = flowNode as FlowSwitch<string>) != null;
        }

        private void ProcessSwitchCases(SwitchCaseData caseData, string switchNodeId, FlowSwitch<string> switchNode, Dictionary<string, FlowNode> flowNodes)
        {
            foreach (var switchCase in caseData.Cases)
            {
                var matchingConnection = FindMatchingConnection(switchNodeId, switchCase.Key, switchCase.Value);
                if (TryGetTargetNode(matchingConnection, flowNodes, out var targetNode))
                {
                    switchNode.Cases[switchCase.Key] = targetNode;
                }
            }
        }

        private void ProcessDefaultCase(SwitchCaseData caseData, string switchNodeId, FlowSwitch<string> switchNode, Dictionary<string, FlowNode> flowNodes)
        {
            if (string.IsNullOrEmpty(caseData.DefaultCase)) 
                return;

            var defaultConnection = FindDefaultConnection(switchNodeId, caseData.DefaultCase);
            if (TryGetTargetNode(defaultConnection, flowNodes, out var defaultTargetNode))
            {
                switchNode.Default = defaultTargetNode;
            }
        }

        private Cell FindMatchingConnection(string switchNodeId, string caseKey, string caseValue)
        {
            return connections.FirstOrDefault(c => 
                c.Source?.Id == switchNodeId && 
                (c.label == caseKey || c.label == caseValue));
        }

        private Cell FindDefaultConnection(string switchNodeId, string defaultCase)
        {
            return connections.FirstOrDefault(c => 
                c.Source?.Id == switchNodeId && 
                (c.label == "Default" || c.label == "default" || c.label == defaultCase));
        }

        private static bool TryGetTargetNode(Cell connection, Dictionary<string, FlowNode> flowNodes, out FlowNode targetNode)
        {
            targetNode = null;
            return connection?.Target?.Id != null && 
                   flowNodes.TryGetValue(connection.Target.Id, out targetNode);
        }

        private void ProcessSwitchCaseData(List<Cell> nodes)
        {
            foreach (var node in nodes)
            {
                if (!TryGetNodeType(node, out string nodeType))
                    continue;

                if (IsSwitchNodeType(nodeType) && TryGetSwitchExpression(node, out string switchExprJson))
                {
                    ProcessSwitchExpressionJson(node.id, switchExprJson);
                }
            }
        }

        private static bool TryGetNodeType(Cell node, out string nodeType)
        {
            nodeType = null;
            if (!node.data.TryGetValue("type", out var typeObj) || typeObj is not string type)
                return false;
            
            nodeType = type.ToLowerInvariant();
            return true;
        }

        private static bool IsSwitchNodeType(string nodeType)
        {
            return nodeType.Contains("dsfflowswitchactivity") || nodeType.Contains("flowswitch");
        }

        private static bool TryGetSwitchExpression(Cell node, out string switchExprJson)
        {
            switchExprJson = null;
            return node.data.TryGetValue("switchExpression", out var switchExprObj) && 
                   switchExprObj is string expr && 
                   !string.IsNullOrEmpty(expr) &&
                   (switchExprJson = expr) != null;
        }

        private void ProcessSwitchExpressionJson(string nodeId, string switchExprJson)
        {
            try
            {
                var switchExpression = JsonConvert.DeserializeObject<dynamic>(switchExprJson);
                var caseData = ExtractSwitchCaseData(switchExpression);
                switchCaseMap[nodeId] = caseData;
            }
            catch (JsonException)
            {
                // If JSON parsing fails, continue without case data
            }
        }

        private static SwitchCaseData ExtractSwitchCaseData(dynamic switchExpression)
        {
            var caseData = new SwitchCaseData();

            // Extract cases
            if (switchExpression?.Cases != null)
            {
                foreach (var caseItem in switchExpression.Cases)
                {
                    if (caseItem?.Key != null && caseItem?.Value != null)
                    {
                        caseData.Cases.Add(new SwitchCase
                        {
                            Key = caseItem.Key.ToString(),
                            Value = caseItem.Value.ToString()
                        });
                    }
                }
            }

            // Extract default case
            if (switchExpression?.DefaultCase != null)
            {
                caseData.DefaultCase = switchExpression.DefaultCase.ToString();
            }

            return caseData;
        }

        private static void HandleDecisionConnection(Cell connection, FlowDecision decision, FlowNode targetNode)
        {
            if (connection.data == null) return;
            
            var isDecisionArm = connection.data.TryGetValue(Constants.ISDECISIONARM, out var isDecisionArmObj) &&
                               bool.TryParse(isDecisionArmObj?.ToString(), out var isDecision) && isDecision;
                               
            if (!isDecisionArm) return;
            
            var isTrue = connection.data.TryGetValue(Constants.ISTRUEARM, out var isTrueArmObj) &&
                        bool.TryParse(isTrueArmObj?.ToString(), out var isTrueArm) && isTrueArm;
                        
            if (isTrue)
                decision.True = targetNode;
            else
                decision.False = targetNode;
        }

        private static void HandleSwitchConnection(Cell connection, FlowSwitch<string> switchNode, FlowNode targetNode)
        {
            string caseKey = null;
            
            // Try to get case key from connection data
            if (connection.data?.TryGetValue(nameof(caseKey), out var caseKeyObj) == true && caseKeyObj is string key)
            {
                caseKey = key;
            }
            // Fallback to using connection label as case key
            else if (!string.IsNullOrEmpty(connection.label))
            {
                caseKey = connection.label;
            }
            
            if (caseKey == null) return;
            
            if (caseKey == "Default" || caseKey == "default")
            {
                switchNode.Default = targetNode;
            }
            else
            {
                switchNode.Cases[caseKey] = targetNode;
            }
        }

        /// <summary>
        /// To open workflow in WW Studio, namespaces should be changed back to mscorlib
        /// </summary>
        /// <param name="root">Root Element</param>
        public static void ReplaceDefaultNamespace(XElement root)
        {
            var oldNs = XNamespace.Get("clr-namespace:System.Collections.Generic;assembly=System.Private.CoreLib");
            var newNs = XNamespace.Get("clr-namespace:System.Collections.Generic;assembly=mscorlib");

            // Recursively update namespace of all elements that used the old default namespace
            void UpdateNamespace(XElement element)
            {
                if (element.Name.Namespace == oldNs)
                {
                    element.Name = newNs + element.Name.LocalName;
                }

	            foreach (var attr in element.Attributes())
	            {
                    // Fix namespace in x:TypeArguments or other attributes that use oldNs in string form
                    if ((attr.Name.LocalName == "x:TypeArguments") &&
                        attr.Value.Contains("clr-namespace:System.Collections.Generic;assembly=System.Private.CoreLib"))
                    {
                        attr.Value = attr.Value.Replace(
                            "clr-namespace:System.Collections.Generic;assembly=System.Private.CoreLib",
                            "clr-namespace:System.Collections.Generic;assembly=mscorlib");
                    }
	            }

                foreach (var child in element.Elements())
                {
                    UpdateNamespace(child);
                }
            }

            UpdateNamespace(root);

            // Remove the old xmlns declaration if any, and add the new one explicitly
            var oldAttr = root.Attributes().FirstOrDefault(a =>
                a.IsNamespaceDeclaration && a.Value == oldNs.NamespaceName);
            oldAttr?.Remove();
        }

        /// <summary>
        /// Add/replaces namespaces
        /// </summary>
        /// <param name="xmlString">xml</param>
        /// <returns>Updated xml with added/replaced namespaces</returns>
        public static StringBuilder AddReplaceNameSpace(StringBuilder xmlString)
        {
            try
            {
                var doc = XElement.Parse(xmlString.ToString());
                ProcessNamespaceReplacements(doc);
                ProcessNamespacesForImplementation(doc);
                ProcessReferencesForImplementation(doc);
                ReplaceDefaultNamespace(doc);
                return new StringBuilder(doc.ToString());
            }
            catch (Exception)
            {
                return xmlString;
            }
        }

        private static void ProcessNamespaceReplacements(XElement doc)
        {
            XNamespace xmlnsNs = "http://www.w3.org/2000/xmlns/";

            // Helper to replace assembly name in given prefix
            void ReplaceAssembly(string prefix)
            {
                var attr = doc.Attribute(XName.Get(prefix, xmlnsNs.NamespaceName));
                if (attr != null && attr.Value.Contains("System.Private.CoreLib"))
                {
                    attr.Value = attr.Value.Replace("System.Private.CoreLib", "mscorlib");
                }
            }

            ReplaceAssembly("scg");
            ReplaceAssembly("sco");

            // Add missing xmlns declarations
            EnsureNamespace(doc, xmlnsNs, "av", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            EnsureNamespace(doc, xmlnsNs, "sap", "http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation");
        }

        private static void EnsureNamespace(XElement doc, XNamespace xmlnsNs, string prefix, string uri)
        {
            if (!doc.Attributes().Any(a => a.Name.LocalName == prefix && a.Name.Namespace == xmlnsNs))
            {
                doc.Add(new XAttribute(XNamespace.Xmlns + prefix, uri));
            }
        }

        private static void ProcessNamespacesForImplementation(XElement doc)
        {
            XNamespace defaultNs = "http://schemas.microsoft.com/netfx/2009/xaml/activities";
            var nsImpl = doc.Element(defaultNs + "TextExpression.NamespacesForImplementation");
            
            if (nsImpl == null) return;

            nsImpl.RemoveNodes();

            const string nsImplBlock = @"
<scg:List x:TypeArguments=""x:String"" Capacity=""6"" 
          xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" 
          xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:String>Dev2.Common</x:String>
  <x:String>Dev2.Data.Decisions.Operations</x:String>
  <x:String>Dev2.Data.SystemTemplates.Models</x:String>
  <x:String>Dev2.DataList.Contract</x:String>
  <x:String>Dev2.DataList.Contract.Binary_Objects</x:String>
  <x:String>Unlimited.Applications.BusinessDesignStudio.Activities</x:String>
</scg:List>";

            AddParsedElements(nsImpl, nsImplBlock);
        }

        private static void ProcessReferencesForImplementation(XElement doc)
        {
            XNamespace defaultNs = "http://schemas.microsoft.com/netfx/2009/xaml/activities";
            var nsRImpl = doc.Element(defaultNs + "TextExpression.ReferencesForImplementation");
            
            if (nsRImpl == null) return;

            nsRImpl.RemoveNodes();

            const string referencesXml = @"
<sco:Collection xmlns:sco='clr-namespace:System.Collections.ObjectModel;assembly=mscorlib' 
                x:TypeArguments='AssemblyReference'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <AssemblyReference>Dev2.Common</AssemblyReference>
  <AssemblyReference>Dev2.Data</AssemblyReference>
  <AssemblyReference>Dev2.Activities</AssemblyReference>
</sco:Collection>";

            AddParsedElementsWithNamespace(nsRImpl, referencesXml, defaultNs);
        }

        private static void AddParsedElements(XElement parent, string xmlBlock)
        {
            var parsedElements = XElement.Parse($"<wrapper>{xmlBlock}</wrapper>").Elements();
            foreach (var node in parsedElements)
            {
                node.Attributes().Where(a => a.IsNamespaceDeclaration).ToList().ForEach(a => a.Remove());
                parent.Add(node);
            }
        }

        private static void AddParsedElementsWithNamespace(XElement parent, string xmlBlock, XNamespace defaultNs)
        {
            var parsedCollection = XElement.Parse($"<wrapper>{xmlBlock}</wrapper>").Elements();
            foreach (var node in parsedCollection)
            {
                node.Attributes().Where(a => a.IsNamespaceDeclaration).ToList().ForEach(a => a.Remove());

                foreach (var descendant in node.DescendantsAndSelf())
                {
                    if (descendant.Name.Namespace == XNamespace.None)
                    {
                        descendant.Name = XName.Get(descendant.Name.LocalName, defaultNs.NamespaceName);
                    }
                }

                parent.Add(node);
            }
        }
        
        /// <summary>
        /// Applies ForEach ID mapping to fix parent relationship references.
        /// This addresses the bug where nested nodes become orphaned when node IDs are regenerated
        /// but parent references are not updated accordingly.
        /// </summary>
        /// <param name="allNodes">All nodes from the X6 graph</param>
        private static void ApplyForEachIdMapping(List<Cell> allNodes)
        {
            // Phase 1: Create ID mapping for nodes that will have new IDs generated
            var idMapping = new Dictionary<string, string>();
            
            foreach (var node in allNodes)
            {
                // For this implementation, we assume all nodes get new IDs
                // In a more sophisticated version, you might only map IDs for nodes that actually change
                var oldId = node.id;
                var newId = Guid.NewGuid().ToString();
                idMapping[oldId] = newId;
                
                // Update the node's ID
                node.id = newId;
            }
            
            // Phase 2: Update ForEach parent references using the ID mapping
            foreach (var node in allNodes)
            {
                if (node.data.TryGetValue("isNestedInForEach", out var isNestedObj) && 
                    bool.TryParse(isNestedObj?.ToString(), out var isNested) && isNested &&
                    node.data.TryGetValue("forEachParentId", out var parentIdObj) && 
                    parentIdObj is string oldParentId && 
                    !string.IsNullOrWhiteSpace(oldParentId))
                {
                    // Update the parent reference if we have a mapping for it
                    if (idMapping.TryGetValue(oldParentId, out var newParentId))
                    {
                        node.data["forEachParentId"] = newParentId;
                        Dev2Logger.Info($"Updated ForEach parent reference: {oldParentId} -> {newParentId} for node {node.id}", GlobalConstants.WarewolfInfo);
                    }
                    else
                    {
                        Dev2Logger.Warn($"Could not find ID mapping for ForEach parent {oldParentId} for node {node.id}", GlobalConstants.WarewolfWarn);
                    }
                }
            }
            
            // Phase 3: Update edge connections to use new node IDs
            // Note: connections are handled separately in the main processing flow
        }
    }
}
