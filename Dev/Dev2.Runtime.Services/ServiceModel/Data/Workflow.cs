#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
#if (WINDOWS || NETFRAMEWORK)
using System.Activities.Presentation.View;
#endif
using System.Activities.Statements;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xaml;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Warewolf.Data;
using Warewolf.Data.Options;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class Workflow : Resource, IWarewolfWorkflow
    {
        private List<IWorkflowNode> _workflowNodes = new List<IWorkflowNode>();
        private List<IWorkflowNode> _workflowNodesForHtml;
        private Collection<FlowNode> _flowNodes;

        public Workflow()
        {
            ResourceType = "WorkflowService";
            DataList = new XElement("DataList");
        }

        public Workflow(Collection<FlowNode> flowNodes)
        {
            _flowNodes = flowNodes;
        }

        public Workflow(XElement xml)
            : base(xml)
        {
            ResourceType = "WorkflowService";
            DataList = xml.Element("DataList") ?? new XElement("DataList");
            Comment = xml.ElementSafe("Comment");
            IconPath = xml.ElementSafe("IconPath");
            Tags = xml.ElementSafe("Tags");
            HelpLink = xml.ElementSafe("HelpLink");
            Name = xml.ElementSafe("DisplayName");

            var action = xml.Descendants("Action").FirstOrDefault();
            if (action == null)
            {
                return;
            }

            XamlDefinition = action.ElementSafeStringBuilder("XamlDefinition");
        }

        public Workflow Clone()
        {
            var clone = MemberwiseClone() as Workflow;

            return clone;
        }

        private List<IWorkflowNode> GetWorkflowNodesForHtml()
        {
            var nodeTree = new WorkflowNode();

            foreach (var node in FlowNodes)
            {
                var workflowNode = GetWorkflowNodeFrom(node);
                if (workflowNode != null)
                {
                    nodeTree.Add(workflowNode);
                }
            }

            return nodeTree.NextNodes;
        }

        private List<IWorkflowNode> GetWorkflowNodes()
        {
            foreach (var node in FlowNodes)
            {
                _ = GetWorkflowNodeFrom(node);
            }

            return _workflowNodes;
        }

        private IWorkflowNode GetWorkflowNodeFrom(FlowNode flowNode)
        {
            var nodeType = flowNode.GetType().Name;
            switch (nodeType)
            {
                case nameof(FlowStep):
                    {
                        return CalculateFlowStep((FlowStep)flowNode);
                    }
                case nameof(FlowDecision):
                    {
                        return CalculateFlowDecision((FlowDecision)flowNode);
                    }
                case "FlowSwitch`1":
                    {
                        return CalculateFlowSwitch((FlowSwitch<string>)flowNode);
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        private IWorkflowNode CalculateFlowSwitch(FlowSwitch<string> node)
        {
            var wfTree = WorkflowNodeFrom(node.Expression as IDev2Activity);

            foreach (var item in node.Cases.Values)
            {
                wfTree.Add(GetWorkflowNodeFrom(item));
            }

            return wfTree;
        }

        private IWorkflowNode CalculateFlowDecision(FlowDecision node)
        {
            var wfTree = WorkflowNodeFrom(node.Condition as IDev2Activity);

            if (wfTree != null)
            {
                if (IsFlowStep(node.True))
                {
                    var activityTrue = ((FlowStep)node.True).Action as IDev2Activity;
                    wfTree.Add(WorkflowNodeFrom(activityTrue));
                }

                if (!IsFlowStep(node.True) && node.True != null)
                {
                    wfTree.Add(GetWorkflowNodeFrom(node.True));
                }

                if (IsFlowStep(node.False))
                {
                    var activityFalse = ((FlowStep)node.False).Action as IDev2Activity;
                    wfTree.Add(WorkflowNodeFrom(activityFalse));
                }

                if (!IsFlowStep(node.False) && node.False != null)
                {
                    wfTree.Add(GetWorkflowNodeFrom(node.False));
                }
            }

            return wfTree;
        }


        private IWorkflowNode CalculateFlowStep(FlowStep flowNode)
        {
            if (IsFlowStep(flowNode))
            {
                return WorkflowNodeFrom(flowNode?.Action as IDev2Activity);
            }

            return null;
        }

        private static bool IsFlowStep(FlowNode flowNode)
        {
            return flowNode is FlowStep;
        }

        private bool IsDsfComment(IDev2Activity activity)
        {
            return activity.GetType().Name is "DsfCommentActivity";
        }

        private IWorkflowNode WorkflowNodeFrom(IDev2Activity activity)
        {
            if (activity != null && !IsDsfComment(activity))
            {
                var workflowNode = new WorkflowNode
                {
                    ActivityID = activity.ActivityId != Guid.Empty ? activity.ActivityId : Guid.Parse(activity.UniqueID),
                    UniqueID = Guid.Parse(activity.UniqueID),
                    StepDescription = activity.GetDisplayName(),
                    ChildNodes = activity.GetChildrenNodes()
                    .Select(o => WorkflowNodeFrom(o))
                    .ToList()
                };

                if (!_workflowNodes.Any(o => o.UniqueID == workflowNode.UniqueID))
                {
                    _workflowNodes.Add(workflowNode);
                }

                return workflowNode;
            }
            return null;
        }

        private Collection<FlowNode> GetFlowNodes()
        {
            var builder = ReadXamlDefinition();
            if (builder is null)
            {
                return new Collection<FlowNode>();
            }

            return ((Flowchart)builder.Implementation).Nodes;
        }

        private ActivityBuilder ReadXamlDefinition()
        {
            var xamlStr = RootActivity?.ToString();
            if (string.IsNullOrEmpty(xamlStr))
            {
                return null;
            }
            try
            {
                if (xamlStr.Length != 0)
                {
                    using (var sw = new StringReader(xamlStr))
                    {
                        var xamlXmlWriterSettings = new XamlXmlReaderSettings
#if (WINDOWS || NETFRAMEWORK)
                        {
                            LocalAssembly = System.Reflection.Assembly.GetAssembly(typeof(VirtualizedContainerService))
                        };
#else
				        ();
#endif
                        var xw = ActivityXamlServices.CreateBuilderReader(new XamlXmlReader(sw, new XamlSchemaContext(), xamlXmlWriterSettings));
                        var load = XamlServices.Load(xw);
                        return load as ActivityBuilder;
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error loading XAML: ", e, GlobalConstants.WarewolfError);
            }
            return null;
        }

        public StringBuilder XamlDefinition { get; set; }
        public new XElement DataList { get; set; }

        public string Comment { get; set; }
        public string IconPath { get; set; }
        public string Tags { get; set; }
        public string HelpLink { get; set; }
        public Collection<FlowNode> FlowNodes => _flowNodes ?? (_flowNodes = GetFlowNodes());
        public List<IWorkflowNode> WorkflowNodesForHtml => _workflowNodesForHtml ?? (_workflowNodesForHtml = GetWorkflowNodesForHtml());
        public List<IWorkflowNode> WorkflowNodes => _workflowNodes.Count != 0 ? _workflowNodes : (_workflowNodes = GetWorkflowNodes());
        public string Name { get; set; }

        public override XElement ToXml()
        {
            var result = base.ToXml();

            var serviceDefinition = XamlDefinition.ToXElement();
            var cleanServiceDefinition = NamespaceNormalizer.Normalize(serviceDefinition);

            serviceDefinition.Name = "XamlDefinition";
            result.Add(new XElement("Comment", Comment ?? string.Empty));
            result.Add(new XElement("IconPath", IconPath ?? string.Empty));
            result.Add(new XElement("Tags", Tags ?? string.Empty));
            result.Add(new XElement("HelpLink", HelpLink ?? string.Empty));
            result.Add(DataList);
            result.Add(new XElement("Action", new XAttribute("Name", "InvokeWorkflow"), new XAttribute("Type", "Workflow"),
                cleanServiceDefinition)
                );
            return result;
        }



    }



    public static class NamespaceNormalizer
    {
        public static XElement Normalize(XElement element)
        {
            // Step 1: Collect namespace declarations in order
            var nsDeclarations = element
                .DescendantsAndSelf()
                .Attributes()
                .Where(a => a.IsNamespaceDeclaration)
                .GroupBy(a => a.Name.LocalName)
                .ToList();

            // Step 2: Resolve conflicts
            var usedUris = new HashSet<string>();
            var aliasMap = new Dictionary<string, XNamespace>();

            foreach (var group in nsDeclarations)
            {
                var alias = group.Key;
                var bindings = group.Select(a => a.Value).Distinct().ToList();

                if (bindings.Count == 1 && usedUris.Add(bindings[0]))
                {
                    // unique alias and URI — keep as-is
                    aliasMap[alias] = bindings[0];
                }
                else
                {
                    // same alias used for multiple URIs ? assign new aliases
                    foreach (var uri in bindings)
                    {
                        if (!usedUris.Add(uri)) continue;

                        string newAlias = GetUniqueAlias(aliasMap.Keys, alias);
                        aliasMap[newAlias] = uri;
                    }
                }
            }

            // Step 3: Rebind element tree using resolved aliasMap
            return RebindWithNamespaceAliases(element, aliasMap);
        }

        private static XElement RebindWithNamespaceAliases(XElement element, Dictionary<string, XNamespace> aliasMap)
        {
            var currentNs = element.Name.Namespace;
            var prefix = aliasMap.FirstOrDefault(p => p.Value == currentNs).Key;
            var newName = prefix != null ? aliasMap[prefix] + element.Name.LocalName : element.Name.LocalName;

            var newElement = new XElement(newName,
                element.Attributes()
                    .Where(a => !a.IsNamespaceDeclaration)
                    .Select(a =>
                        a.Name.Namespace == XNamespace.None
                            ? new XAttribute(a.Name.LocalName, a.Value)
                            : new XAttribute(
                                aliasMap.FirstOrDefault(p => p.Value == a.Name.Namespace).Value + a.Name.LocalName,
                                a.Value)
                    ),
                element.Elements().Select(child => RebindWithNamespaceAliases(child, aliasMap))
            );

            // Add namespace declarations at top-most level
            if (element.Parent == null)
            {
                foreach (var ns in aliasMap)
                {
                    if (!string.IsNullOrWhiteSpace(ns.Key) && ns.Key != "xmlns")
                    {
                        newElement.Add(new XAttribute(XNamespace.Xmlns + ns.Key, ns.Value));
                    }
                }

            }

            return newElement;
        }

        private static string GetUniqueAlias(IEnumerable<string> existingAliases, string baseAlias)
        {
            int index = 1;
            string newAlias = baseAlias + index;
            while (existingAliases.Contains(newAlias))
            {
                index++;
                newAlias = baseAlias + index;
            }
            return newAlias;
        }
    }

}
