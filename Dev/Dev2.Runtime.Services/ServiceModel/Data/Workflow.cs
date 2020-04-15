#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xaml;
using System.Xml.Linq;
using Dev2;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Warewolf.Data;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class Workflow : Resource, IWarewolfWorkflow
    {
        #region CTOR

        public Workflow()
        {
            ResourceType = "WorkflowService";
            DataList = new XElement("DataList");
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
            //DisplayName = xml.ElementSafe("Name");

            var action = xml.Descendants("Action").FirstOrDefault();
            if (action == null)
            {
                return;
            }

            XamlDefinition = action.ElementSafeStringBuilder("XamlDefinition");
            FlowNodes = GetFlowNodes(action);
            WorkflowNodesForHtml = GetWorkflowNodesForHtml();
            WorkflowNodes = GetWorkflowNodes();
        }

        private List<IWorkflowNode> GetWorkflowNodesForHtml()
        {
            var list = new List<IWorkflowNode>();
            var childNodes = new List<IWorkflowNode>();
            foreach (var node in FlowNodes)
            {
                var nodeType = node.GetType().Name;
                IDev2Activity activity;
                switch (node.GetType().Name)
                {
                    case nameof(FlowStep):
                        activity = ((FlowStep)node).Action as IDev2Activity;
                        if (activity.GetType().Name is "DsfCommentActivity")
                        {
                            //Do nothing. DsfCommentActivity should not be part of coverage
                        }
                        else
                        {
                            var foundInChildNodes = childNodes.Find(o => o.UniqueID.ToString() == activity.UniqueID);
                            if (childNodes.Count is 0 || foundInChildNodes is null)
                            {
                                list.Add(new WorkflowNode
                                {
                                    ActivityID = activity.ActivityId,
                                    UniqueID = Guid.Parse(activity.UniqueID),
                                    StepDescription = activity.GetDisplayName()
                                });
                            }
                        }
                        break;

                    case nameof(FlowDecision):
                        List<IWorkflowNode> nodes;
                        CalculateFlowDecisionNodes(list, childNodes, node, out activity, out nodes);

                        break;
                    case "FlowSwitch`1":
                        nodes = new List<IWorkflowNode>();

                        foreach (var item in ((FlowSwitch<string>)node).Cases.Values)
                        {
                            var from = ((FlowStep)item).Action as IDev2Activity;
                            if (from.GetType().Name is "DsfCommentActivity")
                            {
                                //Do nothing. DsfCommentActivity should not be part of coverage
                            }
                            else
                            {
                                nodes.Add(new WorkflowNode
                                {
                                    ActivityID = from.ActivityId,
                                    UniqueID = Guid.Parse(from.UniqueID),
                                    StepDescription = from.GetDisplayName()
                                });
                            }
                        }
                        list.AddRange(nodes);
                        break;

                    default:
                        break;
                }
            }

            return list;
        }

        private void CalculateFlowDecisionNodes(List<IWorkflowNode> list, List<IWorkflowNode> childNodes, FlowNode node, out IDev2Activity activity, out List<IWorkflowNode> nodes)
        {
            var flowDecision = ((FlowDecision)node);

            nodes = GetFlowStepFromFlowNode(flowDecision.False);
            if (nodes.Count > 0)
            {
                childNodes.AddRange(nodes);
            }
            else
            {
                var falseA = (((FlowStep)flowDecision.False)?.Action as IDev2Activity);
                if (falseA.GetType().Name is "DsfCommentActivity")
                {
                    //Do nothing. DsfCommentActivity should not be part of coverage
                }
                else
                {
                    var falseArm = new WorkflowNode
                    {
                        ActivityID = falseA.ActivityId,
                        UniqueID = Guid.Parse(falseA.UniqueID),
                        StepDescription = falseA.GetDisplayName(),
                    };
                    childNodes.Add(falseArm);
                }
            }

            nodes = GetFlowStepFromFlowNode(flowDecision.True);
            if (nodes.Count > 0)
            {
                childNodes.AddRange(nodes);
            }
            else
            {
                var trueA = ((FlowStep)flowDecision.True)?.Action as IDev2Activity;
                if (trueA.GetType().Name is "DsfCommentActivity")
                {
                    //Do nothing. DsfCommentActivity should not be part of coverage
                }
                else
                {
                    var trueArm = new WorkflowNode
                    {
                        ActivityID = trueA.ActivityId,
                        UniqueID = Guid.Parse(trueA.UniqueID),
                        StepDescription = trueA.GetDisplayName(),
                    };
                    childNodes.Add(trueArm);
                }
            }

            activity = flowDecision.Condition as IDev2Activity;
            list.Add(new WorkflowNode
            {
                ActivityID = activity.ActivityId,
                UniqueID = Guid.Parse(activity.UniqueID),
                StepDescription = activity.GetDisplayName(),
                NextNodes = childNodes
            });
        }

        private List<IWorkflowNode> GetFlowStepFromFlowNode(FlowNode flowNode)
        {
            var flowNodeTypeName = flowNode.GetType().Name;
            var nodes = new List<IWorkflowNode>();
            var list = new List<IWorkflowNode>();
            var childNodes = new List<IWorkflowNode>();
            IDev2Activity activity;
            if (flowNodeTypeName == "FlowSwitch`1")
            {
                return CalculateFlowSwitch(flowNode, nodes);
            }
            else if (flowNodeTypeName == nameof(FlowDecision))
            {
                CalculateFlowDecisionNodes(list, childNodes, flowNode, out activity, out nodes);
            }
            return nodes;

        }

        private static List<IWorkflowNode> CalculateFlowSwitch(FlowNode flowNode, List<IWorkflowNode> nodes)
        {
            var childNodes = new List<IWorkflowNode>();
            var fromParent = ((FlowSwitch<string>)flowNode);
            
            foreach (var item in ((FlowSwitch<string>)flowNode).Cases.Values)
            {
                var from = ((FlowStep)item).Action as IDev2Activity;
                childNodes.Add(new WorkflowNode
                {
                    ActivityID = from.ActivityId,
                    UniqueID = Guid.Parse(from.UniqueID),
                    StepDescription = from.GetDisplayName()
                });

            }

            nodes.Add(new WorkflowNode
            {
                StepDescription = fromParent.DisplayName,
                NextNodes = childNodes
            });
            return nodes;
        }

        private List<IWorkflowNode> GetWorkflowNodes()
        {
            var list = new List<IWorkflowNode>();
            foreach (var node in FlowNodes)
            {
                var nodeType = node.GetType().Name;
                IDev2Activity activity;
                switch (node.GetType().Name)
                {
                    case nameof(FlowStep):
                        activity = ((FlowStep)node).Action as IDev2Activity;
                        if (activity.GetType().Name is "DsfCommentActivity")
                        {
                            //Do nothing. DsfCommentActivity should not be part of coverage
                        }
                        else
                        {
                            list.Add(new WorkflowNode
                            {
                                ActivityID = activity.ActivityId,
                                UniqueID = Guid.Parse(activity.UniqueID),
                                StepDescription = activity.GetDisplayName()
                            });
                        }
                        break;

                    case nameof(FlowDecision):
                        var flowDecision = ((FlowDecision)node);

                        activity = flowDecision.Condition as IDev2Activity;
                        if (activity.GetType().Name is "DsfCommentActivity")
                        {
                            //Do nothing. DsfCommentActivity should not be part of coverage
                        }
                        else
                        {
                            list.Add(new WorkflowNode
                            {
                                ActivityID = activity.ActivityId,
                                UniqueID = Guid.Parse(activity.UniqueID),
                                StepDescription = activity.GetDisplayName()
                            });
                        }
                        break;

                    case nameof(FlowSwitch<FlowNode>):
                        var nodes = new List<IWorkflowNode>();

                        foreach (var item in ((FlowSwitch<string>)node).Cases.Values)
                        {
                            var from = ((FlowStep)item).Action as IDev2Activity;
                            if (from.GetType().Name is "DsfCommentActivity")
                            {
                                //Do nothing. DsfCommentActivity should not be part of coverage
                            }
                            else
                            {
                                nodes.Add(new WorkflowNode
                                {
                                    ActivityID = from.ActivityId,
                                    UniqueID = Guid.Parse(from.UniqueID),
                                    StepDescription = from.GetDisplayName()
                                });
                            }
                        }
                        list.AddRange(nodes);
                        break;

                    default:
                        break;
                }
            }

            return list;
        }

        private Collection<FlowNode> GetFlowNodes(XElement action)
        {
            var builder = ReadXamlDefinition(action.ElementSafe("XamlDefinition").ToStringBuilder());

            return ((Flowchart)builder.Implementation).Nodes;
        }

        private ActivityBuilder ReadXamlDefinition(StringBuilder xaml)
        {
            try
            {
                if (xaml != null && xaml.Length != 0)
                {
                    using (var sw = new StringReader(xaml.ToString()))
                    {
                        var xamlXmlWriterSettings = new XamlXmlReaderSettings
                        {
                            LocalAssembly = System.Reflection.Assembly.GetAssembly(typeof(VirtualizedContainerService))
                        };
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

        #endregion

        public StringBuilder XamlDefinition { get; set; }
        public new XElement DataList { get; set; }

        public string Comment { get; set; }
        public string IconPath { get; set; }
        public string Tags { get; set; }
        public string HelpLink { get; set; }
        public Collection<FlowNode> FlowNodes { get; private set; }
        public List<IWorkflowNode> WorkflowNodesForHtml { get; }
        public List<IWorkflowNode> WorkflowNodes { get; }
        public string Name { get; set; }

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            var serviceDefinition = XamlDefinition.ToXElement();
            serviceDefinition.Name = "XamlDefinition";
            result.Add(new XElement("Comment", Comment ?? string.Empty));
            result.Add(new XElement("IconPath", IconPath ?? string.Empty));
            result.Add(new XElement("Tags", Tags ?? string.Empty));
            result.Add(new XElement("HelpLink", HelpLink ?? string.Empty));
            result.Add(DataList);
            result.Add(new XElement("Action", new XAttribute("Name", "InvokeWorkflow"), new XAttribute("Type", "Workflow"),
                serviceDefinition)
                );
            return result;
        }

        #endregion

    }
}
