using Dev2.Common.X6;
using Dev2.Utilities;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public class X6ToWorkflowConverter
    {
        private Dictionary<string, Activity> activityMap = new();
        private List<Cell> connections = new List<Cell>();

        public static StringBuilder ConvertJsonToXaml(Dictionary<string, System.Text.StringBuilder> values)
        {
            values.TryGetValue("ResourceJSON", out StringBuilder resourceDefinition);

            if (resourceDefinition != null && resourceDefinition.Length > 0)
            {
                var xaml = new X6ToWorkflowConverter().ConvertX6ToUiPath(resourceDefinition.ToString());
                return xaml;
            }

            return new StringBuilder();
        }

        public StringBuilder ConvertX6ToUiPath(string x6Json)
        {
            try
            {
                var x6Graph = JsonConvert.DeserializeObject<X6Graph>(x6Json);
                var activityBuilder = ConvertFromX6Graph(x6Graph);
                var flowChart = activityBuilder.Implementation as Flowchart;
                var workflowHelper = new WorkflowHelper();
                workflowHelper.EnsureImplementation(activityBuilder, flowChart);
                var workflowXaml = workflowHelper.GetXamlDefinition(activityBuilder);
                return workflowXaml;
            }
            catch (Exception)
            {

            }
            return null;
        }

        private ActivityBuilder ConvertFromX6Graph(X6Graph x6Graph)
        {
            var workflowName = x6Graph.ResourceName ?? "ConvertedWorkflow";
            var activityBuilder = new ActivityBuilder
            {
                Name = workflowName
            };

            // Separate nodes and edges
            var nodes = x6Graph.Cells.Where(c => c.Shape != "edge").ToList();
            connections = x6Graph.Cells.Where(c => c.Shape == "edge").ToList();

            foreach (var node in nodes)
            {
                var activity = CreateActivityFromNode(node);
                if (activity != null)
                {
                    activityMap[node.Id] = activity;
                }
            }

            // Build the workflow structure
            activityBuilder.Implementation = BuildSequentialWorkflow(nodes);
            return activityBuilder;
        }

        private Activity BuildSequentialWorkflow(List<Cell> nodes)
        {
            var sequence = new Sequence();
            var flowchart = new Flowchart();

            // Find the start node
            var startNode = nodes.FirstOrDefault(n => GetNodeType(n) == Constants.START);
            if (startNode == null) return sequence;

            // Build flowchart structure
            var flowNodes = new Dictionary<string, FlowNode>();

            foreach (var node in nodes)
            {
                //var nodeType = GetNodeType(node).ToLowerInvariant();

                var flowNode = new FlowStep
                {
                    Action = activityMap[node.Id]
                };
                flowchart.StartNode = flowNode;


                if (flowNode != null)
                {
                    flowNodes[node.Id] = flowNode;
                }
            }

            // Connect the flow nodes based on edges
            ConnectFlowNodes(flowNodes);

            return flowchart;
        }

        private static Activity CreateActivityFromNode(Cell node)
        {
            if (!node.Data.TryGetValue("type", out var typeObj) || typeObj is not string type || string.IsNullOrWhiteSpace(type))
                return null;

            var nodeType = type.ToLowerInvariant();
            if (nodeType == Constants.START)
            {
                return new WriteLine { Text = "Workflow Start Node" };
            }
            else if (nodeType.Contains("dsfdotnetmultiassignactivity"))
            {
                return CreateAssignActivity(node);
            }
            else if (nodeType.Contains("dsfdecision"))
            {
                return CreateDecisionActivity(node);
            }
            else
            {
                return new WriteLine { Text = "Unknow type" };
            }

        }

        private static DsfDecision CreateDecisionActivity(Cell node)
        {
            if (!node.Data.TryGetValue(Constants.DISPLAYNAME, out var displayObject)
                || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfDecision();
            activity.FromX6Graph(node);
            return activity;
        }

        private static DsfDotNetMultiAssignActivity CreateAssignActivity(Cell node)
        {

            if (!node.Data.TryGetValue(Constants.DISPLAYNAME, out var displayObject)
                || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfDotNetMultiAssignActivity();
            activity.FromX6Graph(node);
            return activity;

        }

        private static string GetNodeType(Cell node)
        {
            node.Data.TryGetValue(Constants.TYPE, out var typeObj);
            return typeObj as string;

        }

        private void ConnectFlowNodes(Dictionary<string, FlowNode> flowNodes)
        {
            foreach (var connection in connections)
            {
                var sourceId = connection.Source?.Id;
                var targetId = connection.Target?.Id;

                if (sourceId != null && targetId != null &&
                    flowNodes.ContainsKey(sourceId) && flowNodes.ContainsKey(targetId))
                {
                    var sourceNode = flowNodes[sourceId];
                    var targetNode = flowNodes[targetId];

                    if (sourceNode is FlowStep step)
                    {
                        step.Next = targetNode;
                    }
                }
            }
        }



    }
}
