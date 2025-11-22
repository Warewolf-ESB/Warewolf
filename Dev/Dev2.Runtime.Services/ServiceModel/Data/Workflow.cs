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
using System.Security;
using System.Text;
using System.Xaml;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data;
using Dropbox.Api.Files;
using Elastic.Clients.Elasticsearch.MachineLearning;
using Warewolf.Data;
using Warewolf.Data.Options;
using Warewolf.Resource.Errors;
using static Dropbox.Api.TeamLog.LoginMethod;

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

        public Workflow(XElement xml, bool loadExtra)
            : this(xml)
        {

            string version = xml.Attribute("Version")?.Value;
            if (Version.TryParse(version, out var versionString))
                this.Version = versionString;

            string serverID = xml.Attribute("ServerID")?.Value;
            if (Guid.TryParse(serverID, out Guid serverGuid))
                this.ServerID = serverGuid;

            string serverVersion = xml.Attribute("ServerVersion")?.Value;

            Version serverversion;
            if (string.IsNullOrEmpty(serverVersion) && Version.TryParse(serverVersion, out serverversion))
                this.ServerVersion = serverversion;

            string category = xml.Element("Category")?.Value;
            this.Category = category;

            string unitTestTargetWorkflowService = xml.Element("UnitTestTargetWorkflowService")?.Value;
            this.UnitTestTargetWorkflowService = unitTestTargetWorkflowService;


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
        public Guid ServerID { get; set; }
        public string Category { get; set; }
        public string UnitTestTargetWorkflowService { get; set; }
        public Version ServerVersion { get; set; }

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


            //return new XElement("Service",
            //            new XAttribute("ID", ID),
            //            new XAttribute("Version", Version?.ToString() ?? "1.0"),
            //            new XAttribute("ServerID", ServerID.ToString()),
            //            new XAttribute("Name", ResourceName ?? string.Empty),
            //            new XAttribute("ResourceType", ResourceType),
            //            new XAttribute("IsValid", IsValid),
            //        new XElement("DisplayName", ResourceName ?? string.Empty),
            //        new XElement("Category", Category ?? string.Empty),
            //        new XElement("IsNewWorkflow", IsNewWorkflow),
            //        new XElement("AuthorRoles", string.Empty),
            //        new XElement("Comment", Comment ?? string.Empty),
            //        new XElement("Tags", Tags ?? string.Empty),
            //        new XElement("HelpLink", HelpLink ?? string.Empty),
            //        new XElement("UnitTestTargetWorkflowService", UnitTestTargetWorkflowService ?? string.Empty),
            //        dataList,
            //        new XElement("Action",
            //            new XAttribute("Name", "InvokeWorkflow"),
            //            new XAttribute("Type", "Workflow"),
            //        new XElement("XamlDefinition", xaml)),
            //        new XElement("ErrorMessages", WriteErrors()));
        }

        public StringBuilder ToServiceDefinition()
        {
            if (ResourceType == "WorkflowService")
            {
                StringBuilder result = WorkflowServiceResourceType();
                return result;
            }
            //if (ResourceType == "Source" || ResourceType == "Server")
            //{
            //    StringBuilder result = SourceOrServerResourceType(prepairForDeployment);
            //    return result;
            //}
            throw new Exception(ErrorResource.ToServiceDefinitionDoesNotRupportResourcesOfTypeSource);
        }

        private StringBuilder WorkflowServiceResourceType()
        {
            var result = new StringBuilder();

            var xaml = new StringBuilder(XmlSanitizer.EncodeXmlAttributeValues(XamlDefinition.ToString()));

            var service = CreateWorkflowXElement(xaml);
            var xws = new XmlWriterSettings { OmitXmlDeclaration = true };
            using (XmlWriter xwriter = XmlWriter.Create(result, xws))
            {
                service.Save(xwriter);
            }

            return result;
        }

        XElement CreateWorkflowXElement(StringBuilder xaml)
        {
            var dataList = this.DataList == null ? new XElement("DataList") : this.DataList;
            var service = CreateServiceElement(xaml, dataList);
            return service;
        }

        private XElement CreateServiceElement(StringBuilder xaml, XElement dataList)
        {
            var xamlString = xaml.ToString();
            if (!xamlString.StartsWith("XamlDefinition", StringComparison.OrdinalIgnoreCase))
            {
                xamlString = string.Concat("<XamlDefinition>", xamlString, "</XamlDefinition>");
            }
            return new XElement("Service",
                        new XAttribute("ID", ResourceID),
                        new XAttribute("Version", Version?.ToString() ?? "1.0"),
                        new XAttribute("ServerID", ServerID.ToString()),
                        new XAttribute("Name", ResourceName ?? string.Empty),
                        new XAttribute("ResourceType", ResourceType),
                        new XAttribute("IsValid", IsValid),
                        new XAttribute("ServerVersion", this.ServerVersion?.ToString() ?? "0.0.0.0"),
                    new XElement("DisplayName", ResourceName ?? string.Empty),
                    new XElement("Category", Category ?? string.Empty),
                    new XElement("IsNewWorkflow", IsNewResource),
                    new XElement("AuthorRoles", string.Empty),
                    new XElement("Comment", Comment ?? string.Empty),
                    new XElement("Tags", Tags ?? string.Empty),
                    new XElement("HelpLink", HelpLink ?? string.Empty),
                    new XElement("UnitTestTargetWorkflowService", UnitTestTargetWorkflowService ?? string.Empty),
                    dataList,
                    new XElement("Action",
                        new XAttribute("Name", "InvokeWorkflow"),
                        new XAttribute("Type", "Workflow"),
                    XElement.Parse(xamlString)),
                    new XElement("ErrorMessages", WriteErrors()),
                    CreateVersionInfoElement());
        }

        private XElement CreateVersionInfoElement()
        {
            return (this.VersionInfo != null)
                ? new XElement("VersionInfo",
                        new XAttribute("DateTimeStamp", this.VersionInfo.DateTimeStamp.ToString("O")),
                        new XAttribute("Reason", this.VersionInfo.Reason ?? string.Empty),
                        new XAttribute("User", this.VersionInfo.User ?? string.Empty),
                        new XAttribute("VersionNumber", this.VersionInfo.VersionNumber ?? string.Empty),
                        new XAttribute("ResourceId", this.VersionInfo.ResourceId.ToString()),
                        new XAttribute("VersionId", this.VersionInfo.VersionId.ToString()))
                : null;
        }

        List<XElement> WriteErrors()
        {
            if (Errors == null || Errors.Count == 0)
            {
                return null;
            }

            var errorElements = new List<XElement>();
            foreach (var errorInfo in Errors)
            {
                var xElement = new XElement("ErrorMessage");
                xElement.Add(new XAttribute("InstanceID", errorInfo.InstanceID));
                xElement.Add(new XAttribute("Message", errorInfo.Message ?? ""));
                xElement.Add(new XAttribute("ErrorType", errorInfo.ErrorType));
                xElement.Add(new XAttribute("FixType", errorInfo.FixType));
                if (!string.IsNullOrEmpty(errorInfo.FixData))
                {
                    xElement.Add(new XCData(errorInfo.FixData));
                }
                errorElements.Add(xElement);
            }

            return errorElements;
        }

        public string GetSavePath()
        {
            if (!string.IsNullOrEmpty(Category))
            {
                var savePath = Category;
                var resourceNameIndex = Category.LastIndexOf(ResourceName, StringComparison.InvariantCultureIgnoreCase);
                if (resourceNameIndex >= 0)
                {
                    savePath = Category.Substring(0, resourceNameIndex);
                }
                return savePath;
            }
            return "";
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



    public static class XmlSanitizer
    {
        public static string EncodeXmlAttributeValues(string xml)
        {
            var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);

            foreach (var element in doc.Descendants())
            {
                var attrs = element.Attributes().ToList(); // Prevent collection modification during iteration
                foreach (var attr in attrs)
                {
                    var escaped = EscapeXmlAttribute(attr.Value);
                    attr.Value = escaped;
                }
            }

            return doc.ToString(SaveOptions.DisableFormatting);
        }

        private static string EscapeXmlAttribute(string value)
        {
            return SecurityElement.Escape(value);
        }
    }


}
