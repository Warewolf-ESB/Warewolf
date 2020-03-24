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
using Dev2.Common;
using Dev2.Common.Common;

namespace Dev2.Runtime.ServiceModel.Data
{
    public interface IWorkflow
    {
        string Comment { get; set; }
        XElement DataList { get; set; }
        Collection<FlowNode> FlowNodes { get; }
        string HelpLink { get; set; }
        string IconPath { get; set; }
        string Tags { get; set; }
        List<IWorkflowNode> WorkflowNodes { get; }
        StringBuilder XamlDefinition { get; set; }

        XElement ToXml();
    }

    public class Workflow : Resource, IWorkflow
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

            var action = xml.Descendants("Action").FirstOrDefault();
            if (action == null)
            {
                return;
            }

            XamlDefinition = action.ElementSafeStringBuilder("XamlDefinition");
            FlowNodes = GetFlowNodes(action);
            WorkflowNodes = GetWorkflowNodes();
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
                        list.Add(new WorkflowNode
                        {
                            ActivityID = activity.ActivityId,
                            UniqueID = Guid.Parse(activity.UniqueID),
                            StepDescription = activity.GetDisplayName()
                        });
                        break;

                    case nameof(FlowDecision):
                        activity = ((FlowDecision)node).Condition as IDev2Activity;
                        list.Add(new WorkflowNode
                        {
                            ActivityID = activity.ActivityId,
                            UniqueID = Guid.Parse(activity.UniqueID),
                            StepDescription = activity.GetDisplayName()
                        });
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
        public List<IWorkflowNode> WorkflowNodes { get; }

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
