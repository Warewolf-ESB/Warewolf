using Dev2.Activities.Exchange;
using Dev2.Activities.DateAndTime;
using Dev2.Activities.RabbitMQ.Consume;
using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Activities.RedisCache;
using Dev2.Activities.SelectAndApply;
using Dev2.Activities.WorkflowConverters;
 
using Dev2.Common.X6;
using Dev2.Data.SystemTemplates.Models;
using Dev2.WorkflowConverters;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.PathOperations;
using Dev2.Common;

namespace Dev2.Activities.WF
{
    /// <summary>
    /// Converts workflow to X6 based Json
    /// </summary>
    public partial class WorkflowToX6Converter
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
            try
            {
                var graphData = new X6WorkflowLoadModel { WorkflowXml = xml, ActivityNodeMap = new Dictionary<Activity, Cell>() };
                var activityNodeMap = new Dictionary<Activity, string>(64);

                var startNode = CreateStartNode();
                graphData.Nodes.Add(startNode);

                if (workflow.Implementation != null)
                {
                    ProcessActivity(workflow.Implementation, graphData, activityNodeMap, startNode.id);
                }

                var json = JsonConvert.SerializeObject(graphData);
                return json;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"ConvertToX6Json failed: {ex.Message}", ex, GlobalConstants.WarewolfError);
                throw;
            }
        }

        private string ProcessActivity(Activity activity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            if (activity == null) return previousNodeId;

            string nodeId;

            if (activity is not Flowchart)
            {
                nodeId = CommonHelper.GenerateNodeId();
                activityNodeMap[activity] = nodeId;

                if (!HasNestedActivities(activity))//if (activity is not DsfForEachActivity)
                {
                    var node = CreateActivityNode(activity, nodeId);
                    graphData.Nodes.Add(node);
                    graphData.ActivityNodeMap[activity] = node;

                    if (!string.IsNullOrEmpty(previousNodeId))
                    {
                        graphData.Edges.Add(CommonHelper.CreateEdge(previousNodeId, nodeId));
                    }
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
                DsfForEachActivity forEachActivity => ProcessDsfForEachActivity(forEachActivity, graphData, activityNodeMap, nodeId, previousNodeId),
                DsfSequenceActivity sequenceActivity => ProcessDsfSequenceActivity(sequenceActivity, graphData, activityNodeMap, previousNodeId),
                DsfSelectAndApplyActivity selectAndApplyActivity => ProcessDsfSelectAndApplyActivity(selectAndApplyActivity, graphData, activityNodeMap, previousNodeId),
                RedisCacheActivity redisCacheActivity => ProcessRedisCacheActivity(redisCacheActivity, graphData, activityNodeMap, previousNodeId),
                SuspendExecutionActivity suspendExecutionActivity => ProcessSuspendExecutionActivity(suspendExecutionActivity, graphData, activityNodeMap, previousNodeId),
                ManualResumptionActivity manualResumptionActivity => ProcessManualResumptionActivity(manualResumptionActivity, graphData, activityNodeMap, previousNodeId),
                GateActivity gateActivity => ProcessGateActivity(gateActivity, graphData, activityNodeMap, previousNodeId),
                _ => ProcessGenericActivity(activity, graphData, activityNodeMap, nodeId)
            };
        }

        

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

            // Process the start node first
            var lastNodeId = ProcessFlowNode(flowchart.StartNode, graphData, activityNodeMap, parentNodeId);

            // Process additional nodes in the flowchart.Nodes collection
            // This ensures all nodes are processed, including disconnected ones
            if (flowchart.Nodes != null)
            {
                foreach (var node in flowchart.Nodes)
                {
                    // Skip the start node as it's already processed
                    if (node == flowchart.StartNode)
                        continue;

                    // Extract the activity from the flow node
                    var activity = GetActivityFromFlowNode(node);
                    if (activity == null)
                        continue;

                    // Check if this activity has already been processed
                    if (!activityNodeMap.ContainsKey(activity))
                    {
                        // Activity not yet processed - process the node without creating an edge from parentNodeId
                        // This handles disconnected nodes that aren't reachable from StartNode
                        // The edges will be created during normal flow traversal
                        ProcessFlowNode(node, graphData, activityNodeMap, null);
                    }
                    // If activity is already processed, it means it was reached during the normal flow traversal
                    // from StartNode, so we don't need to do anything - edges are already correct
                }
            }

            return lastNodeId;
        }

        /// <summary>
        /// Extracts the Activity from a FlowNode
        /// </summary>
        /// <param name="flowNode">The flow node to extract activity from</param>
        /// <returns>The activity contained in the flow node, or null if none found</returns>
        private static Activity GetActivityFromFlowNode(FlowNode flowNode)
        {
            return flowNode switch
            {
                FlowStep flowStep => flowStep.Action,
                FlowDecision flowDecision => flowDecision.Condition,
                FlowSwitch<string> flowSwitch => flowSwitch.Expression,
                FlowSwitch<object> flowSwitchObj => flowSwitchObj.Expression,
                _ => null
            };
        }

        /// <summary>
        /// Checks if an edge already exists between source and target nodes
        /// </summary>
        /// <param name="graphData">The X6 graph data</param>
        /// <param name="sourceId">Source node ID</param>
        /// <param name="targetId">Target node ID</param>
        /// <param name="label">Optional edge label</param>
        /// <returns>True if edge exists, false otherwise</returns>
        private static bool EdgeExists(X6WorkflowLoadModel graphData, string sourceId, string targetId, string label = null)
        {
            return graphData.Edges.Exists(e => 
                e.Source?.Id == sourceId && 
                e.Target?.Id == targetId && 
                (string.IsNullOrEmpty(label) || e.label == label));
        }

        /// <summary>
        /// Safely creates an edge only if it doesn't already exist
        /// </summary>
        /// <param name="graphData">The X6 graph data</param>
        /// <param name="sourceId">Source node ID</param>
        /// <param name="targetId">Target node ID</param>
        /// <param name="label">Optional edge label</param>
        private static void CreateEdgeIfNotExists(X6WorkflowLoadModel graphData, string sourceId, string targetId, string label = "")
        {
            if (string.IsNullOrEmpty(sourceId) || string.IsNullOrEmpty(targetId))
                return;
                
            if (!EdgeExists(graphData, sourceId, targetId, label))
            {
                graphData.Edges.Add(CommonHelper.CreateEdge(sourceId, targetId, label));
            }
        }

        /// <summary>
        /// Processes a flow node or creates an edge if the node is already processed
        /// </summary>
        /// <param name="flowNode">The flow node to process</param>
        /// <param name="graphData">The X6 graph data</param>
        /// <param name="activityNodeMap">Map of activities to their node IDs</param>
        /// <param name="sourceNodeId">The source node ID for edge creation</param>
        /// <param name="edgeLabel">Optional label for the edge</param>
        /// <returns>The node ID of the processed or existing node</returns>
        private string ProcessOrLinkFlowNode(FlowNode flowNode, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string sourceNodeId, string edgeLabel = "")
        {
            if (flowNode == null) return sourceNodeId;
            
            var activity = GetActivityFromFlowNode(flowNode);
            
            if (activity != null && activityNodeMap.TryGetValue(activity, out var existingNodeId))
            {
                // Node already processed - just create edge if needed
                CreateEdgeIfNotExists(graphData, sourceNodeId, existingNodeId, edgeLabel);
                return existingNodeId;
            }
            else
            {
                // Node not yet processed - process it and create edge with label
                var targetNodeId = ProcessFlowNode(flowNode, graphData, activityNodeMap, null);
                
                // Create edge from source to the newly processed node with the appropriate label
                CreateEdgeIfNotExists(graphData, sourceNodeId, targetNodeId, edgeLabel);
                
                return targetNodeId;
            }
        }

        private string ProcessFlowNode(FlowNode flowNode, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            return flowNode switch
            {
                FlowStep flowStep => ProcessFlowStep(flowStep, graphData, activityNodeMap, previousNodeId),
                FlowDecision flowDecision => ProcessFlowDecision(flowDecision, graphData, activityNodeMap, previousNodeId),
                FlowSwitch<string> flowSwitchString => ProcessFlowSwitch(flowSwitchString, graphData, activityNodeMap, previousNodeId),
                _ => previousNodeId
            };
        }

        private string ProcessFlowStep(FlowStep flowStep, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            var nodeId = ProcessActivity(flowStep.Action, graphData, activityNodeMap, previousNodeId);

            if (flowStep.Next != null)
            {
                ProcessOrLinkFlowNode(flowStep.Next, graphData, activityNodeMap, nodeId);
            }

            return nodeId;
        }

        private string ProcessFlowDecision(FlowDecision flowDecision, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            var decisionNodeId = CommonHelper.GenerateNodeId();
            var decisionNode = CreateDecisionNode(flowDecision, decisionNodeId);
            graphData.Nodes.Add(decisionNode);
            //graphData.ActivityNodeMap[flowDecision] = decisionNode;

            CreateEdgeIfNotExists(graphData, previousNodeId, decisionNodeId);

            // Process True branch
            if (flowDecision.True != null)
            {
                ProcessOrLinkFlowNode(flowDecision.True, graphData, activityNodeMap, decisionNodeId, Constants.TRUE);
            }

            // Process False branch
            if (flowDecision.False != null)
            {
                ProcessOrLinkFlowNode(flowDecision.False, graphData, activityNodeMap, decisionNodeId, Constants.FALSE);
            }

            return decisionNodeId;
        }

        private string ProcessFlowSwitch(FlowSwitch<string> flowSwitch, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            var switchNodeId = CommonHelper.GenerateNodeId();
            //var switchNode = CreateSwitchNodeString(flowSwitch, switchNodeId);
            var helper = new SwitchActivityDataHelper(this._currentX, this._currentY);
            var switchNode = helper.CreateSwitchNodeString(flowSwitch, switchNodeId);

            this._currentX = helper.CurrentX;
            this._currentY = helper.CurrentY;

            // Store the switch activity in the activity map if it has an expression
            if (flowSwitch.Expression != null)
            {
                activityNodeMap[flowSwitch.Expression] = switchNodeId;
            }

            graphData.Nodes.Add(switchNode);
            //graphData.ActivityNodeMap[flowSwitch] = switchNode;

            CreateEdgeIfNotExists(graphData, previousNodeId, switchNodeId);

            // Process each case
            foreach (var caseItem in flowSwitch.Cases)
            {
                var label = caseItem.Key?.ToString() ?? "Case";
                ProcessOrLinkFlowNode(caseItem.Value, graphData, activityNodeMap, switchNodeId, label);
            }

            // Process default case
            if (flowSwitch.Default != null)
            {
                ProcessOrLinkFlowNode(flowSwitch.Default, graphData, activityNodeMap, switchNodeId, "Default");
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
            CreateEdgeIfNotExists(graphData, bodyNodeId, parentNodeId, "Loop");
            return bodyNodeId;
        }

        private string ProcessDoWhileActivity(DoWhile doWhileActivity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            if (doWhileActivity.Body == null) return parentNodeId;

            var bodyNodeId = ProcessActivity(doWhileActivity.Body, graphData, activityNodeMap, parentNodeId);
            // Create loop back edge
            CreateEdgeIfNotExists(graphData, bodyNodeId, parentNodeId, "Loop");
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

        private string ProcessDsfForEachActivity(DsfForEachActivity forEachActivity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId, string previousNodeId)
        {
            // The nodeId should already be generated and stored in activityNodeMap by ProcessActivity
            if (!activityNodeMap.TryGetValue(forEachActivity, out var forEachNodeId))
            {
                forEachNodeId = CommonHelper.GenerateNodeId();
                activityNodeMap[forEachActivity] = forEachNodeId;
            }

            // Create the ForEach node - the ToX6Json method will handle embedding child activities
            var forEachNode = CreateForEachNode(forEachActivity, forEachNodeId);
            graphData.Nodes.Add(forEachNode);
            graphData.ActivityNodeMap[forEachActivity] = forEachNode;

            // Create edge from previous node to this ForEach node
            CreateEdgeIfNotExists(graphData, previousNodeId, forEachNodeId);

            // Process nested activities from DataFunc.Handler
            ProcessForEachNestedActivities(forEachActivity, forEachNodeId, graphData, activityNodeMap);

            return forEachNodeId;
        }

        /// <summary>
        /// Processes nested activities within a ForEach activity and creates separate X6 nodes for them
        /// </summary>
        /// <param name="forEachActivity">The ForEach activity containing nested activities</param>
        /// <param name="forEachNodeId">The node ID of the parent ForEach activity</param>
        /// <param name="graphData">The X6 graph data to add nodes to</param>
        /// <param name="activityNodeMap">Map of activities to their node IDs</param>
        private void ProcessForEachNestedActivities(DsfForEachActivity forEachActivity, string forEachNodeId,
            X6WorkflowLoadModel graphData, Dictionary<Activity, string> activityNodeMap)
        {
            // Check if there's a nested activity in the DataFunc.Handler
            var nestedActivity = forEachActivity.DataFunc?.Handler;
            if (nestedActivity == null) return;

            // Generate a unique node ID for the nested activity
            var nestedNodeId = CommonHelper.GenerateNodeId();
            activityNodeMap[nestedActivity] = nestedNodeId;

            // Create the nested activity node
            var nestedNode = CreateActivityNode(nestedActivity, nestedNodeId);

            // Add nesting metadata to indicate this activity is nested within the ForEach
            nestedNode.data[Constants.ISNESTED_INFOREACH] = true;
            nestedNode.data[Constants.PARENTID_FOREACH] = forEachNodeId;

            // Add the nested node to the graph
            graphData.Nodes.Add(nestedNode);
            graphData.ActivityNodeMap[nestedActivity] = nestedNode;

            // Recursively process any further nested activities (e.g., if the nested activity is itself a container)
            ProcessNestedActivityChildren(nestedActivity, graphData, activityNodeMap, forEachNodeId);
        }

        /// <summary>
        /// Recursively processes any child activities of a nested activity
        /// </summary>
        /// <param name="parentActivity">The parent activity to check for children</param>
        /// <param name="graphData">The X6 graph data to add nodes to</param>
        /// <param name="activityNodeMap">Map of activities to their node IDs</param>
        /// <param name="parentActivityId">The ID of the root ForEach parent</param>
        private void ProcessNestedActivityChildren(Activity parentActivity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentActivityId)
        {
            // Get child activities using the existing GetChildActivities method
            ActivityPropertiesReaderHelper.GetChildActivities(parentActivity, _tempChildActivities);

            foreach (var childActivity in _tempChildActivities.ToList())
            {
                // Skip if we've already processed this activity
                if (activityNodeMap.ContainsKey(childActivity)) continue;

                var childNodeId = CommonHelper.GenerateNodeId();
                activityNodeMap[childActivity] = childNodeId;

                var childNode = CreateActivityNode(childActivity, childNodeId);

                // Add nesting metadata
                childNode.data[Constants.ISNESTED_INFOREACH] = true;
#pragma warning disable CC0021 // Use nameof
                childNode.data[Constants.PARENTID_FOREACH] = parentActivityId;
#pragma warning restore CC0021 // Use nameof

                graphData.Nodes.Add(childNode);
                graphData.ActivityNodeMap[childActivity] = childNode;

                // Recursively process further nested children
                ProcessNestedActivityChildren(childActivity, graphData, activityNodeMap, parentActivityId);
            }
        }

        private string ProcessGenericActivity(Activity activity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string parentNodeId)
        {
            // Use cached child activities
            ActivityPropertiesReaderHelper.GetChildActivities(activity, _tempChildActivities);

            var childActivities = _tempChildActivities.ToList();
            var currentNodeId = parentNodeId;
            bool activityHasNestedActivities = HasNestedActivities(activity);
            for (int i = 0; i < childActivities.Count; i++)
            {
                currentNodeId = ProcessActivity(childActivities[i], graphData, activityNodeMap, activityHasNestedActivities ? parentNodeId : currentNodeId);
            }

            return currentNodeId;
        }

        private static bool HasNestedActivities(Activity activity)
        {
            return activity switch
            {
                Sequence => true,
                //Flowchart => true,
                DsfForEachActivity => true,
                DsfSequenceActivity => true,
                DsfSelectAndApplyActivity => true,
                RedisCacheActivity => true,
                SuspendExecutionActivity => true,
                ManualResumptionActivity => true,
                GateActivity => true,
                _ => false
            };
        }

        //private static void GetChildActivities(Activity activity, List<Activity> children)
        //{
        //    children.Clear();
        //    var activityType = activity.GetType();

        //    // Use cached properties to avoid repeated reflection
        //    if (!_childActivityPropertiesCache.TryGetValue(activityType, out var properties))
        //    {
        //        properties = activityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //        _childActivityPropertiesCache[activityType] = properties;
        //    }

        //    for (int i = 0; i < properties.Length; i++)
        //    {
        //        var prop = properties[i];

        //        if (typeof(Activity).IsAssignableFrom(prop.PropertyType))
        //        {
        //            if (prop.GetValue(activity) is Activity childActivity)
        //            {
        //                children.Add(childActivity);
        //            }
        //        }
        //        else if (typeof(ICollection<Activity>).IsAssignableFrom(prop.PropertyType) && prop.GetValue(activity) is ICollection<Activity> childActivities)
        //        {
        //            children.AddRange(childActivities);
        //        }

        //    }
        //}

        private Cell CreateStartNode()
        {
            var node = new Cell
            {
                id = CommonHelper.GenerateNodeId(),
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

            // Use direct type comparison instead of typeof() for better performance
            if (activity is DsfDotNetMultiAssignActivity multiAssign)
            {
                multiAssign.ToX6Json(cell);
            }
            else if (activity is DsfDotNetMultiAssignObjectActivity multiAssignObjectActivity)
            {
                multiAssignObjectActivity.ToX6Json(cell);
            }
            else if (activity is DsfDecision decision)
            {
                cell = CreateDecisionNode(decision, nodeId);
            }
            else if (activity is DsfForEachActivity forEachActivity)
            {
                cell = CreateForEachNode(forEachActivity, nodeId);
            }
            else if (activity is DsfSequenceActivity sequenceActivity)
            {
                cell = CreateSequenceNode(sequenceActivity, nodeId);
            }
            else if (activity is DsfSelectAndApplyActivity selectAndApplyActivity)
            {
                cell = CreateSelectAndApplyActivity(selectAndApplyActivity, nodeId);
            }
            else if (activity is DsfDataMergeActivity dataMergeActivity)
            {
                cell = CreateDataMergeActivity(dataMergeActivity, nodeId);
            }
            else if (activity is DsfDataSplitActivity dataSplitActivity)
            {
                cell = CreateDataSplitActivity(dataSplitActivity, nodeId);
            }
            else if (activity is DsfBaseConvertActivity baseConvertActivity)
            {
                cell = CreateBaseConvertActivity(baseConvertActivity, nodeId);
            }
            else if (activity is DsfReplaceActivity replaceActivity)
            {
                cell = CreateReplaceActivity(replaceActivity, nodeId);
            }
            else if (activity is DsfCaseConvertActivity caseConvertActivity)
            {
                cell = CreateCaseConvertActivity(caseConvertActivity, nodeId);
            }
            else if (activity is DsfIndexActivity findIndexActivity)
            {
                cell = CreateFindIndexActivity(findIndexActivity, nodeId);
            }
            else if (activity is DsfFileRead fileReadActivity)
            {
                cell = CreateFileReadActivity(fileReadActivity, nodeId);
            }
            else if (activity is FileReadWithBase64 fileReadWithBase64Activity)
            {
                cell = CreateFileReadWithBase64Activity(fileReadWithBase64Activity, nodeId);
            }
            else if (activity is WebGetActivity webGetActivity)
            {
                cell = CreateWebGetActivity(webGetActivity, nodeId);
            }
            else if (activity is DsfWebGetRequestWithTimeoutActivity webRequestWithTimeoutActivity)
            {
                cell = CreateWebRequestWithTimeoutActivity(webRequestWithTimeoutActivity, nodeId);
            }
            else if (activity is WebPostActivityNew webPostActivityNew)
            {
                cell = CreateWebPostActivity(webPostActivityNew, nodeId);
            }
            else if (activity is WebPutActivity webPutActivity)
            {
                cell = CreateWebPutActivity(webPutActivity, nodeId);
            }
            else if (activity is DsfWebDeleteActivity webDeleteActivity)
            {
                cell = CreateWebDeleteActivity(webDeleteActivity, nodeId);
            }
            else if (activity is DsfSqlServerDatabaseActivity sqlServerDatabaseActivity)
            {
                cell = CreateSqlServerDatabaseActivity(sqlServerDatabaseActivity, nodeId);
            }
            else if (activity is DsfPostgreSqlActivity postgresqlDatabaseActivity)
            {
                cell = CreatePostgreSQLDatabaseActivity(postgresqlDatabaseActivity, nodeId);
            }
            else if (activity is DsfMySqlDatabaseActivity mySqlDatabaseActivity)
            {
                cell = CreateMySqlDatabaseActivity(mySqlDatabaseActivity, nodeId);
            }
            else if (activity is DsfSqlBulkInsertActivity sqlBulkInsertActivity)
            {
                cell = CreateSqlBulkInsertActivity(sqlBulkInsertActivity, nodeId);
            }
            else if (activity is DsfOracleDatabaseActivity oracleDatabaseActivity)
            {
                cell = CreateOracleDatabaseActivity(oracleDatabaseActivity, nodeId);
            }
            else if (activity is AdvancedRecordsetActivity advancedRecordsetActivity)
            {
                cell = CreateAdvancedRecordsetActivity(advancedRecordsetActivity, nodeId);
            }
            else if (activity is RedisCache.RedisCacheActivity redisCacheActivity)
            {
                cell = CreateRedisCacheActivity(redisCacheActivity, nodeId);
            }
            else if (activity is RedisRemove.RedisRemoveActivity redisRemoveActivity)
            {
                return CreateRedisRemoveActivity(redisRemoveActivity, nodeId);
            }
            else if (activity is DsfFindRecordsMultipleCriteriaActivity findRecordsActivity)
            {
                cell = CreateFindRecordsMultipleCriteriaActivity(findRecordsActivity, nodeId);
            }
            else if (activity is DsfDeleteRecordNullHandlerActivity deleteRecordNullHanlderActivity)
            {
                cell = CreateDsfDeleteRecordNullHandlerActivity(deleteRecordNullHanlderActivity, nodeId);
            }
            else if (activity is DsfDeleteRecordActivity deleteRecordActivity)
            {
                cell = CreateDsfDeleteRecordActivity(deleteRecordActivity, nodeId);
            }
            else if (activity is DsfSortRecordsActivity sortRecordsActivity)
            {
                cell = CreateDsfSortRecordsActivity(sortRecordsActivity, nodeId);
			}
			else if (activity is DsfCountRecordsetNullHandlerActivity countRecordsetActivity)
			{
				cell = CreateCountRecordsetActivity(countRecordsetActivity, nodeId);
			}
			else if (activity is DsfRecordsetNullhandlerLengthActivity recordsetLengthActivity)
			{
				cell = CreateRecordsetLengthActivity(recordsetLengthActivity, nodeId);
			}
            else if (activity is DsfUniqueActivity uniqueActivity)
            {
                cell = CreateDsfUniqueRecordsActivity(uniqueActivity, nodeId);
            }
            else if (activity is DsfPublishRabbitMQActivity publishDsfRabbitMQActivity)
            {
                cell = CreateDsfPublishRabbitMQActivity(publishDsfRabbitMQActivity, nodeId);
            }
            else if (activity is PublishRabbitMQActivity publishRabbitMQActivity)
            {
                cell = CreatePublishRabbitMQActivity(publishRabbitMQActivity, nodeId);
            }
            else if (activity is DsfConsumeRabbitMQActivity consumeRabbitMQActivity)
            {
                cell = CreateDsfConsumeRabbitMQActivity(consumeRabbitMQActivity, nodeId);
            }
            else if (activity is DsfFolderReadActivity folderReadActivity)
            {
                cell = CreateFolderReadActivity(folderReadActivity, nodeId);
            }
            else if (activity is DsfFolderRead folderRead)
            {
                cell = CreateFolderRead(folderRead, nodeId);
            }
            else if (activity is DsfPathCreate pathCreateActivity)
            {
                cell = CreatePathCreateActivity(pathCreateActivity, nodeId);
            }
            else if (activity is DsfPathCopy pathCopyActivity)
            {
                cell = CreatePathCopyActivity(pathCopyActivity, nodeId);
            }
            else if (activity is DsfPathMove pathMoveActivity)
            {
                cell = CreatePathMoveActivity(pathMoveActivity, nodeId);
            }
            else if (activity is DsfPathRename pathRenameActivity)
            {
                cell = CreatePathRenameActivity(pathRenameActivity, nodeId);
            }
            else if (activity is DsfZip zipActivity)
            {
                cell = CreateZipActivity(zipActivity, nodeId);
            }
            else if (activity is DsfPathDelete pathDeleteActivity)
            {
                cell = CreatePathDeleteActivity(pathDeleteActivity, nodeId);
            }
            else if (activity is DsfUnZip unZipActivity)
            {
                cell = CreateUnZipActivity(unZipActivity, nodeId);
            }
            else if (activity is DsfCommentActivity commentActivity)
            {
                cell = CreateCommentActivity(commentActivity, nodeId);
            }
            else if (activity is DsfFileWrite dsfFileWriteActivity)
            {
                cell = CreateFileWriteActivity(dsfFileWriteActivity, nodeId);
            }
            else if (activity is FileWriteActivity pathFileWriteActivity)
            {
                cell = CreateFileWriteActivity(pathFileWriteActivity, nodeId);
            }
            else if (activity is DsfExecuteCommandLineActivity dsfExecuteCommandLineActivity)
            {
                cell = CreateCommandLineActivity(dsfExecuteCommandLineActivity, nodeId);
            }
            else if (activity is Scripting.DsfJavascriptActivity javascriptActivity)
            {
                cell = CreateJavascriptActivity(javascriptActivity, nodeId);
            }
            else if (activity is Scripting.DsfRubyActivity rubyActivity)
            {
                cell = CreateRubyActivity(rubyActivity, nodeId);
            }
            else if (activity is Scripting.DsfPythonActivity pythonscriptActivity)
            {
                cell = CreatePythonActivity(pythonscriptActivity, nodeId);
            }
            else if (activity is DateAndTime.DsfDotNetDateTimeActivity dotNetDateTimeActivity)
            {
                cell = CreateDotNetDateTimeActivity(dotNetDateTimeActivity, nodeId);
            }
            else if (activity is DsfDateTimeActivity dateTimeActivity)
            {
                cell = CreateDateTimeActivity(dateTimeActivity, nodeId);
            }
            else if (activity is SuspendExecutionActivity suspendExecutionActivity)
            {
                cell = CreateSuspendExecutionActivity(suspendExecutionActivity, nodeId);
            }
            else if(activity is ManualResumptionActivity manualResumptionActivity)
            {
                cell = CreateManualResumptionActivity(manualResumptionActivity, nodeId);
            }
            else if (activity is DsfSendEmailActivity sendEmailActivity)
            {
                cell = CreateSendEmailActivity(sendEmailActivity, nodeId);
            }
            else if (activity is DsfExchangeEmailNewActivity exchangeEmailActivity)
            {
                cell = CreateExchangeEmailActivity(exchangeEmailActivity, nodeId);
            }
            else if (activity is DsfCreateJsonActivity createJsonActivity)
            {
                cell = CreateCreateJsonActivity(createJsonActivity, nodeId);
            }
            else if (activity is DsfXPathActivity xpathActivity)
            {
                cell = CreateXPathActivity(xpathActivity, nodeId);
            }
            else if (activity is GateActivity gateActivity)
            {
                cell = CreateGateActivity(gateActivity, nodeId);
            }
            else if (activity is DsfRandomActivity randomActivity)
            {
                cell = CreateRandomActivity(randomActivity, nodeId);
            }
            else if (activity is DsfNumberFormatActivity numberFormatActivity)
            {
                cell = CreateNumberFormatActivity(numberFormatActivity, nodeId);
            }
            else if (activity is DsfDotNetCalculateActivity calculateActivity)
            {
                cell = CreateCalculateActivity(calculateActivity, nodeId);
            }
            else if (activity is DsfDotNetAggregateCalculateActivity dotnetAggregateCalculateActivity)
            {
                cell = CreateDotNetAggregateCalculateActivity(dotnetAggregateCalculateActivity, nodeId);
            }
            else if (activity is DsfAggregateCalculateActivity aggregateCalculateActivity)
            {
                cell = CreateAggregateCalculateActivity(aggregateCalculateActivity, nodeId);
            }
            else if (activity is DsfDateTimeDifferenceActivity dateTimeDifferenceActivity)
            {
                cell = CreateDateTimeDifferenceActivity(dateTimeDifferenceActivity, nodeId);
            }
            else if (activity is DsfDotNetDateTimeDifferenceActivity dotnetDateTimeDifferenceActivity)
            {
                cell = CreateDotnetDateTimeDifferenceActivity(dotnetDateTimeDifferenceActivity, nodeId);
            }
            else if (activity is DsfDotNetGatherSystemInformationActivity dotnetGatherSystemInfoActivity)
            {
                cell = CreateDotNetGatherSystemInformationActivity(dotnetGatherSystemInfoActivity, nodeId);
            }
            else if (activity is DsfGatherSystemInformationActivity gatherSystemInfoActivity)
            {
                cell = CreateGatherSystemInformationActivity(gatherSystemInfoActivity, nodeId);
            }
            else if (activity is DsfODBCDatabaseActivity odbcDatabaseActivity)
            {
                cell = CreateODBCDatabaseActivity(odbcDatabaseActivity, nodeId);
            }
            else if (activity is DsfWorkflowActivity workflowActivity)
            {
                cell = CreateDsfWorkflowActivity(workflowActivity, nodeId);
            }
            else
            {
                cell.shape = Constants.RECT;
            }

            cell.position = new Position(_currentX, _currentY);
            if (string.IsNullOrEmpty(cell.label))
                cell.label = GetActivityLabel(activity);
            if (!cell.data.ContainsKey(Constants.TYPE))
                cell.data.Add(Constants.TYPE, activityType);
            if (!cell.data.ContainsKey(Constants.DISPLAYNAME))
                cell.data.Add(Constants.DISPLAYNAME, activity.DisplayName);

            cell.data.Add(Constants.PROPERTIES, ExtractActivityProperties(activity));
            ExtractForEachNestingInfo(activity, cell);

            return cell;
        }
        

        /// <summary>
        /// Extracts ForEach nesting information from an activity's annotations and adds it to the cell data
        /// </summary>
        /// <param name="activity">The activity to extract nesting info from</param>
        /// <param name="cell">The X6 cell to add nesting info to</param>
        private static void ExtractForEachNestingInfo(Activity activity, Cell cell)
        {
            try
            {
                // Look for ForEach nesting information in the activity's annotations
                var annotationsProperty = activity.GetType().GetProperty("Annotations");
                if (annotationsProperty?.GetValue(activity) is System.Collections.ObjectModel.Collection<object> annotations)
                {
                    // Find the ForEachNestingInfo annotation
                    foreach (var annotation in annotations)
                    {
                        if (annotation is Dictionary<string, object> annotationDict &&
                            annotationDict.TryGetValue("_annotationType", out var annotationType) &&
                            annotationType?.ToString() == "ForEachNestingInfo")
                        {
                            // Extract the nesting information
                            if (annotationDict.TryGetValue("isNestedInForEach", out var isNestedObj) &&
                                bool.TryParse(isNestedObj?.ToString(), out var isNested))
                            {
                                cell.data["isNestedInForEach"] = isNested;
                            }

                            if (annotationDict.TryGetValue("forEachParentId", out var parentIdObj) &&
                                parentIdObj is string parentId && !string.IsNullOrEmpty(parentId))
                            {
                                cell.data["forEachParentId"] = parentId;
                            }

                            break; // Found our annotation, no need to continue
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If extraction fails, continue without nesting info
                // This ensures the conversion doesn't fail due to annotation issues
            }
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

        private Cell CreateForEachNode(DsfForEachActivity forEachActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = forEachActivity.DisplayName ?? Constants.DISPLAYNAME_FOREACH,
                data = new Dictionary<string, object>()
            };

            // Update position for next node
            _currentY += 150;

            // Use the existing ToX6Json method from DsfForEachActivity
            forEachActivity.ToX6Json(cell);

            return cell;
        }

        #region unused code
        //private Cell CreateSwitchNode(FlowSwitch<object> flowSwitch, string nodeId)
        //{
        //    var cell = new Cell
        //    {
        //        id = nodeId,
        //        shape = Constants.POLYGON,
        //        position = new Position(_currentX, _currentY),
        //        label = Constants.SWITCH,
        //        data = new Dictionary<string, object>
        //        {
        //            [Consta
        //            nts.TYPE] = Constants.FLOWSWITCH,
        //            [Constants.EXPRESSION] = GetCleanExpressionText(flowSwitch.Expression) ?? Constants.SWITCH
        //        }
        //    };

        //    // Update position for next node
        //    _currentY += 150;

        //    // Try to access the underlying activity through reflection if needed
        //    var expression = flowSwitch.Expression;
        //    if (expression.GetType().Name.Contains(nameof(DsfFlowSwitchActivity)))
        //    {
        //        ProcessSwitchActivityReflection(expression, flowSwitch, cell);
        //    }
        //    else
        //    {
        //        Console.WriteLine($"[X6Convert-Switch] Using basic processing for expression type: {expression.GetType().Name}");
        //        // Basic processing for other expression types
        //        cell.data[Constants.DISPLAYNAME] = expression.DisplayName ?? Constants.SWITCH;
        //        cell.label = expression.DisplayName ?? Constants.SWITCH;
        //    }

        //    return cell;
        //}

        //private Cell CreateSwitchNodeString(FlowSwitch<string> flowSwitch, string nodeId)
        //{

        //    var cell = new Cell
        //    {
        //        id = nodeId,
        //        shape = Constants.POLYGON,
        //        position = new Position(_currentX, _currentY),
        //        label = Constants.SWITCH,
        //        data = new Dictionary<string, object>
        //        {
        //            [Constants.TYPE] = Constants.FLOWSWITCH,
        //            [Constants.EXPRESSION] = flowSwitch.Expression?.ToString() ?? Constants.SWITCH
        //        }
        //    };

        //    // Update position for next node
        //    _currentY += 150;

        //    // If the expression is a DsfFlowSwitchActivity, extract more detailed information
        //    if (flowSwitch.Expression != null)
        //    {
        //        var expression = flowSwitch.Expression;

        //        if (expression.GetType().Name.Contains(nameof(DsfFlowSwitchActivity)))
        //        {
        //            ProcessSwitchActivityReflectionString(expression, flowSwitch, cell);
        //            //SwitchActivityHelper.ProcessSwitchActivityString(expression, flowSwitch, cell);
        //        }
        //        else
        //        {
        //            // Basic processing for other expression types
        //            cell.data[Constants.DISPLAYNAME] = expression.DisplayName ?? Constants.SWITCH;
        //            cell.label = expression.DisplayName ?? Constants.SWITCH;
        //        }
        //    }

        //    return cell;
        //}

        //private static void ProcessSwitchActivityReflection(Activity expression, FlowSwitch<object> flowSwitch, Cell cell)
        //{
        //    try
        //    {
        //        // Use reflection to access DsfFlowSwitchActivity properties
        //        var type = expression.GetType();

        //        var displayNameProperty = type.GetProperty("DisplayName");
        //        var expressionTextProperty = type.GetProperty("ExpressionText");
        //        var uniqueIdProperty = type.GetProperty("UniqueID");

        //        var displayName = displayNameProperty?.GetValue(expression) as string ?? Constants.SWITCH;
        //        var expressionText = expressionTextProperty?.GetValue(expression) as string;
        //        var uniqueId = uniqueIdProperty?.GetValue(expression) as string;

        //        cell.data[Constants.DISPLAYNAME] = displayName;
        //        cell.label = displayName;

        //        if (!string.IsNullOrEmpty(expressionText))
        //        {
        //            var cleanVariable = ExtractSwitchVariable(expressionText);
        //            if (!string.IsNullOrEmpty(cleanVariable))
        //            {
        //                cell.data[Constants.EXPRESSION] = $"[[{cleanVariable}]]";
        //            }

        //            var switchExpressionJson = CreateSwitchExpressionJson(expressionText, flowSwitch);
        //            cell.data["switchExpression"] = switchExpressionJson;
        //        }

        //        if (!string.IsNullOrEmpty(uniqueId))
        //        {
        //            cell.data["UniqueID"] = uniqueId;
        //        }
        //    }
        //    catch
        //    {
        //        // Fallback to basic processing
        //        cell.data[Constants.DISPLAYNAME] = expression.DisplayName ?? Constants.SWITCH;
        //        cell.label = expression.DisplayName ?? Constants.SWITCH;
        //    }
        //}

        //private static void ProcessSwitchActivityReflectionString(Activity expression, FlowSwitch<string> flowSwitch, Cell cell)
        //{
        //    try
        //    {
        //        // Use reflection to access DsfFlowSwitchActivity properties
        //        var type = expression.GetType();

        //        var displayNameProperty = type.GetProperty("DisplayName");
        //        var expressionTextProperty = type.GetProperty("ExpressionText");
        //        var uniqueIdProperty = type.GetProperty("UniqueID");

        //        var onErrorVariableProperty = type.GetProperty("OnErrorVariable");
        //        var onErrorWorkflowProperty = type.GetProperty("OnErrorWorkflow");
        //        var isEndedOnErrorProperty = type.GetProperty("IsEndedOnError");

        //        var onErrorVariable = (string)(onErrorVariableProperty?.GetValue(expression) ?? string.Empty);
        //        var onErrorWorkflow = (string)(onErrorWorkflowProperty?.GetValue(expression) ?? string.Empty);
        //        var isEndedOnError = (bool)(isEndedOnErrorProperty?.GetValue(expression) ?? false);

        //        DsfNativeActivity<object>.SetOnErrorData(cell, isEndedOnError, onErrorVariable, onErrorWorkflow);

        //        var displayName = displayNameProperty?.GetValue(expression) as string ?? Constants.SWITCH;
        //        var expressionText = expressionTextProperty?.GetValue(expression) as string;
        //        var uniqueId = uniqueIdProperty?.GetValue(expression) as string;

        //        cell.data[Constants.DISPLAYNAME] = displayName;
        //        cell.label = displayName;

        //        if (!string.IsNullOrEmpty(expressionText))
        //        {
        //            var cleanVariable = ExtractSwitchVariable(expressionText);
        //            if (!string.IsNullOrEmpty(cleanVariable))
        //            {
        //                cell.data[Constants.EXPRESSION] = $"[[{cleanVariable}]]";
        //            }

        //            var switchExpressionJson = CreateSwitchExpressionJsonString(expressionText, flowSwitch);
        //            cell.data["switchExpression"] = switchExpressionJson;
        //        }

        //        if (!string.IsNullOrEmpty(uniqueId))
        //        {
        //            cell.data["UniqueID"] = uniqueId;
        //        }
        //    }
        //    catch
        //    {
        //        // Fallback to basic processing
        //        cell.data[Constants.DISPLAYNAME] = expression.DisplayName ?? Constants.SWITCH;
        //        cell.label = expression.DisplayName ?? Constants.SWITCH;
        //    }
        //}

        //private static string CreateSwitchExpressionJson(string expressionText, FlowSwitch<object> flowSwitch)
        //{
        //    try
        //    {
        //        var switchVariable = ExtractSwitchVariable(expressionText);

        //        var cases = flowSwitch.Cases.Select(c =>
        //        {
        //            var key = c.Key?.ToString();
        //            var value = c.Key?.ToString();
        //            return new { Key = key, Value = value };
        //        }).ToList();

        //        var defaultCase = flowSwitch.Default != null ? "Default" : null;

        //        var switchExpression = new
        //        {
        //            SwitchVariable = switchVariable,
        //            Cases = cases,
        //            DefaultCase = defaultCase
        //        };

        //        var result = JsonConvert.SerializeObject(switchExpression);
        //        return result;
        //    }
        //    catch
        //    {
        //        // If serialization fails, return a basic expression
        //        var fallback = JsonConvert.SerializeObject(new { SwitchVariable = "variable", Cases = new object[0] });
        //        Console.WriteLine($"[X6Convert-Switch-JSON] Fallback JSON: {fallback}");
        //        return fallback;
        //    }
        //}

        //private static string CreateSwitchExpressionJsonString(string expressionText, FlowSwitch<string> flowSwitch)
        //{
        //    try
        //    {
        //        var switchVariable = ExtractSwitchVariable(expressionText);

        //        var cases = flowSwitch.Cases.Select(c =>
        //        {
        //            var key = c.Key?.ToString();
        //            var value = c.Key?.ToString();
        //            return new { Key = key, Value = value };
        //        }).ToList();

        //        var defaultCase = flowSwitch.Default != null ? "Default" : null;

        //        var switchExpression = new
        //        {
        //            SwitchVariable = switchVariable,
        //            Cases = cases,
        //            DefaultCase = defaultCase
        //        };

        //        return JsonConvert.SerializeObject(switchExpression);
        //    }
        //    catch
        //    {
        //        // If serialization fails, return a basic expression
        //        return JsonConvert.SerializeObject(new { SwitchVariable = "variable", Cases = new object[0] });
        //    }
        //}

        //private static string ExtractSwitchVariable(string expressionText)
        //{
        //    if (string.IsNullOrEmpty(expressionText))
        //    {
        //        return "variable";
        //    }

        //    // Try to extract variable name from expression like: Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData("[[hello]]",AmbientDataList)
        //    var match = System.Text.RegularExpressions.Regex.Match(expressionText, @"\[\[([^\]]+)\]\]");
        //    if (match.Success)
        //    {
        //        return match.Groups[1].Value;
        //    }

        //    // Fallback to basic extraction
        //    var startIndex = expressionText.IndexOf("\"[[") + 3;
        //    var endIndex = expressionText.IndexOf("]]\"");
        //    if (startIndex > 2 && endIndex > startIndex)
        //    {
        //        return expressionText.Substring(startIndex, endIndex - startIndex);
        //    }

        //    return "variable";
        //}

        //private static string GetCleanExpressionText(Activity expression)
        //{
        //    if (expression == null) return null;

        //    // For DsfFlowSwitchActivity, get the clean ExpressionText directly
        //    if (expression is IFlowNodeActivity flowNodeActivity)
        //    {
        //        var expressionText = flowNodeActivity.ExpressionText;
        //        if (!string.IsNullOrEmpty(expressionText))
        //        {
        //            var cleanVariable = ExtractSwitchVariable(expressionText);
        //            if (!string.IsNullOrEmpty(cleanVariable))
        //            {
        //                return $"[[{cleanVariable}]]";
        //            }
        //        }
        //    }

        //    // Fallback to basic approach
        //    return expression.ToString();
        //} 
        #endregion



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
                _ => CommonHelper.GenerateNodeId()
            };
        }
    }

}

