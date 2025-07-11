using Dev2.Common.X6;
using Dev2.Utilities;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public class X6ToWorkflowConverter
    {
        private Dictionary<string, Activity> activityMap = new();
        private List<Cell> connections = new List<Cell>();

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
            catch (Exception)
            {

            }
            return null;
        }

        private ActivityBuilder X6JsonToActivityBuilder(X6WorkflowSaveModel x6Graph)
        {
            var workflowName = x6Graph.ResourceName ?? "ConvertedWorkflow";
            var activityBuilder = new ActivityBuilder
            {
                Name = workflowName
            };

            // Separate nodes and edges
            var nodes = x6Graph.Cells.Where(c => c.shape != "edge").ToList();
            connections = x6Graph.Cells.Where(c => c.shape == "edge").ToList();
            Cell startcell = null;

            foreach (var node in nodes)
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

            // Build the workflow structure
            activityBuilder.Implementation = BuildWorkflow(nodes, startcell);

            return activityBuilder;
        }

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
                var flowNode = new FlowStep
                {
                    Action = activityMap[node.id]
                };

                if (flowNode != null)
                {
                    flowNodes[node.id] = flowNode;

                    if (node.id == startcell.id)
                    {
                        startFlowNode = flowNode;
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
        /// Activity Factory: Creates Activity from X6 Json Cell
        /// </summary>
        /// <param name="node">X6 Json Cell</param>
        /// <param name="isStartNode">flag to indicate if node is a start node</param>
        /// <returns></returns>
        private static Activity CreateActivityFromNode(Cell node, out bool isStartNode)
        {
            isStartNode = false;

            if (!node.data.TryGetValue("type", out var typeObj) || typeObj is not string type || string.IsNullOrWhiteSpace(type))
                return null;

            var nodeType = type.ToLowerInvariant();
            if (nodeType == Constants.START)
            {
                isStartNode = true;
                return new WriteLine { Text = "Workflow Start Node" };
            }
            else if (nodeType.Contains("dsfdotnetmultiassignactivity"))
            {
                return CreateAssignActivity(node);
            }
            else if (nodeType.Contains("dsfdecision") || nodeType.Contains("flowdecision"))
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
            if (!node.data.TryGetValue(Constants.DISPLAYTEXT, out var displayObject)
                || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfDecision();
            activity.FromX6Json(node);
            return activity;
        }

        private static DsfDotNetMultiAssignActivity CreateAssignActivity(Cell node)
        {

            if (!node.data.TryGetValue(Constants.DISPLAYNAME, out var displayObject)
                || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfDotNetMultiAssignActivity();
            activity.FromX6Json(node);
            return activity;

        }

        private static string GetNodeType(Cell node)
        {
            node.data.TryGetValue(Constants.TYPE, out var typeObj);
            return typeObj as string;
        }

        //private void CreateConnections(Dictionary<string, FlowNode> flowNodes)
        //{
        //    foreach (var connection in connections)
        //    {
        //        var sourceId = connection.Source?.Id;
        //        var targetId = connection.Target?.Id;

        //        if (sourceId != null && targetId != null &&
        //            flowNodes.ContainsKey(sourceId) && flowNodes.ContainsKey(targetId))
        //        {
        //            var sourceNode = flowNodes[sourceId];
        //            var targetNode = flowNodes[targetId];

        //            if (sourceNode is FlowStep step)
        //            {
        //                step.Next = targetNode;
        //            }
        //        }
        //    }
        //}

        private void CreateConnections(Dictionary<string, FlowNode> flowNodes)
        {
            foreach (var connection in connections)
            {
                if (connection.Source?.Id is string sourceId &&
                    connection.Target?.Id is string targetId &&
                    flowNodes.TryGetValue(sourceId, out var sourceNode) &&
                    flowNodes.TryGetValue(targetId, out var targetNode) &&
                    sourceNode is FlowStep step)
                {
                    step.Next = targetNode;
                }
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

        public static StringBuilder AddReplaceNameSpace(StringBuilder xmlString)
        {
            try
            {
                var activityXaml = xmlString.ToString();
                var doc = XElement.Parse(activityXaml);
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
                void EnsureNamespace(string prefix, string uri)
                {
                    if (!doc.Attributes().Any(a => a.Name.LocalName == prefix && a.Name.Namespace == xmlnsNs))
                    {
                        doc.Add(new XAttribute(XNamespace.Xmlns + prefix, uri));
                    }
                }

                EnsureNamespace("av", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                EnsureNamespace("sap", "http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation");

                XNamespace defaultNs = "http://schemas.microsoft.com/netfx/2009/xaml/activities";

                // Replace content in TextExpression.NamespacesForImplementation
                var nsImpl = doc.Element(defaultNs + "TextExpression.NamespacesForImplementation");
                if (nsImpl != null)
                {
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

                    var parsedElements = XElement.Parse($"<wrapper>{nsImplBlock}</wrapper>").Elements();
                    foreach (var node in parsedElements)
                    {
                        node.Attributes().Where(a => a.IsNamespaceDeclaration).ToList().ForEach(a => a.Remove());
                        nsImpl.Add(node);
                    }
                }

                // Replace content in TextExpression.ReferencesForImplementation
                var nsRImpl = doc.Element(defaultNs + "TextExpression.ReferencesForImplementation");
                if (nsRImpl != null)
                {
                    nsRImpl.RemoveNodes();

                    const string referencesXml = @"
<sco:Collection xmlns:sco='clr-namespace:System.Collections.ObjectModel;assembly=mscorlib' 
                x:TypeArguments='AssemblyReference'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <AssemblyReference>Dev2.Common</AssemblyReference>
  <AssemblyReference>Dev2.Data</AssemblyReference>
  <AssemblyReference>Dev2.Activities</AssemblyReference>
</sco:Collection>";

                    var parsedCollection = XElement.Parse($"<wrapper>{referencesXml}</wrapper>").Elements();
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

                        nsRImpl.Add(node);
                    }
                }

                ReplaceDefaultNamespace(doc);

                return new StringBuilder(doc.ToString());
            }
            catch (Exception)
            {
                return xmlString;
            }
        }

    }
}
