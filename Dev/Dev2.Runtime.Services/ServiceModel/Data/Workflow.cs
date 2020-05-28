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
using System.Activities.Validation;
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
using Warewolf.Data;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class Workflow : Resource, IWarewolfWorkflow
    {
        private List<IWorkflowNode> _workflowNodes;
        private List<IWorkflowNode> _workflowNodesForHtml;
        private Collection<FlowNode> _flowNodes;

        #region CTOR

        public Workflow()
        {
            ResourceType = "WorkflowService";
            DataList = new XElement("DataList");
        }

        public Workflow(Collection<FlowNode> flowNodes)//, List<IWorkflowNode> workflowNodes)
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
            //DisplayName = xml.ElementSafe("Name");

            var action = xml.Descendants("Action").FirstOrDefault();
            if (action == null)
            {
                return;
            }

            XamlDefinition = action.ElementSafeStringBuilder("XamlDefinition");
        }

        private List<IWorkflowNode> GetWorkflowNodesForHtml()
        {
            var nodeTree = new WorkflowNode();

            foreach (var node in FlowNodes)
            {
                var workflowNode = GetWorkflowNodeFrom(node);
                if (workflowNode != null)
                    nodeTree.Add(workflowNode);
            }

            return nodeTree.NextNodes;
        }

        private IWorkflowNode GetWorkflowNodeFrom(FlowNode flowNode)
        {
            var nodeType = flowNode.GetType().Name;
            switch (nodeType)
            {
                case nameof(FlowStep):
                    return CalculateFlowStep((FlowStep)flowNode);
                    break;
                case nameof(FlowDecision):
                    return CalculateFlowDecision((FlowDecision)flowNode);
                    break;
                case "FlowSwitch`1":
                    return CalculateFlowSwitch((FlowSwitch<string>)flowNode);
                    break;
                default:
                    return new WorkflowNode { MockSelected = true, StepDescription = "{0} is not a valid FlowNode type" };
                    break;
            }
        }

        private IWorkflowNode CalculateFlowSwitch(FlowSwitch<string> node)
        {
            var wfTree = new WorkflowNode
            {
                StepDescription = node.DisplayName
            };

            foreach (var item in ((FlowSwitch<string>)node).Cases.Values)
            {
               wfTree.Add(GetWorkflowNodeFrom(item));
            }
            return wfTree;
        }

        private IWorkflowNode CalculateFlowDecision(FlowDecision node)
        {
            var wfTree = new WorkflowNode
            {
                StepDescription = node.DisplayName
            };

            IDev2Activity activityTrue;
            if (IsFlowStep(node.True))
            {
                activityTrue = ((FlowStep)node.True)?.Action as IDev2Activity;
                wfTree.Add(WorkflowNodeFrom(activityTrue));
            }
            if (!IsFlowStep(node.True))
                wfTree.Add(GetWorkflowNodeFrom(node.True));

            IDev2Activity activityFalse;
            if (IsFlowStep(node.False))
            {
                activityFalse = ((FlowStep)node.False)?.Action as IDev2Activity;
                wfTree.Add(WorkflowNodeFrom(activityFalse));
            }
            if (!IsFlowStep(node.False))
                wfTree.Add(GetWorkflowNodeFrom(node.False));

            return wfTree;
        }

        private static bool IsFlowStep(FlowNode flowNode)
        {
            return (flowNode as FlowStep) != null ? true : false;
        }

        private IWorkflowNode CalculateFlowStep(FlowStep flowNow)
        {
            var activity = ((FlowStep)flowNow)?.Action as IDev2Activity;
            if (activity is null)
            {

            }
            if (!IsDsfComment(activity))
                return WorkflowNodeFrom(activity);

            return null;
        }

        private bool IsDsfComment(IDev2Activity activity)
        {
            return activity.GetType().Name is "DsfCommentActivity";
        }

        private static IWorkflowNode WorkflowNodeFrom(IDev2Activity activity)
        {
            return new WorkflowNode
            {
                ActivityID = activity.ActivityId,
                UniqueID = Guid.Parse(activity.UniqueID),
                StepDescription = activity.GetDisplayName(),
            };
        }

        private Collection<FlowNode> GetFlowNodes()
        {
            var builder = ReadXamlDefinition();

            return ((Flowchart)builder.Implementation).Nodes;
        }

        private ActivityBuilder ReadXamlDefinition()
        {
            var xamlStr = RootActivity.ToString();
            try
            {
                if (xamlStr.Length != 0)
                {
                    using (var sw = new StringReader(xamlStr))
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
        public Collection<FlowNode> FlowNodes => _flowNodes ?? (_flowNodes = GetFlowNodes());
        public List<IWorkflowNode> WorkflowNodesForHtml => _workflowNodesForHtml ?? (_workflowNodesForHtml = GetWorkflowNodesForHtml());
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
