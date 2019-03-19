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
using System.Activities.Expressions;
using System.Activities.Presentation.Services;
using System.Activities.Statements;
using System.Activities.XamlIntegration;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xaml;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.Decision;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Utilities
{
    public class WorkflowHelper : IWorkflowHelper
    {
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a syncRoot 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //

        // NOTE : This singleton instance causes memory leaks ;)

        public StringBuilder SerializeWorkflow(ModelService modelService)
        {
            var builder = GetActivityBuilder(modelService);
            var text = GetXamlDefinition(builder);

            return text;
        }

        public StringBuilder GetXamlDefinition(ActivityBuilder builder)
        {
            var text = new StringBuilder();
            try
            {
                if(builder != null)
                {
                    var sb = new StringBuilder();
                    using(var sw = new StringWriter(sb))
                    {
                        var xamlXmlWriterSettings = new XamlXmlWriterSettings { AssumeValidInput = true };
                        var xamlSchemaContext = new XamlSchemaContext();
                        var xw = ActivityXamlServices.CreateBuilderWriter(new XamlXmlWriter(sw, xamlSchemaContext,xamlXmlWriterSettings));                    
                        XamlServices.Save(xw, builder);
                        text = sb.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
                    }
                }
                text = SanitizeXaml(text);
            }
            catch(Exception e)
            {
                Dev2Logger.Error("Error loading XAML: ",e, GlobalConstants.WarewolfError);
            }
            return text;
        }

        public ActivityBuilder ReadXamlDefinition(StringBuilder xaml)
        {
            try
            {
                if(xaml != null && xaml.Length!=0)
                {
                    using(var sw = new StringReader(xaml.ToString()))
                    {
                        var xamlXmlWriterSettings = new XamlXmlReaderSettings();
                        var xw = ActivityXamlServices.CreateBuilderReader(new XamlXmlReader(sw, new XamlSchemaContext(),xamlXmlWriterSettings));
                        var load = XamlServices.Load(xw);
                        return load as ActivityBuilder;
                    }
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Error("Error loading XAML: ",e, GlobalConstants.WarewolfError);
            }
            return null;
        }

        public ActivityBuilder CreateWorkflow(string displayName)
        {
            if(string.IsNullOrEmpty(displayName))
            {
                throw new ArgumentNullException(nameof(displayName));
            }

            var chart = new Flowchart
            {
                DisplayName = displayName
            };

            var builder = new ActivityBuilder
            {
                Name = displayName,
                Implementation = chart
            };

            EnsureImplementation(builder, chart);

            return builder;
        }

        public ActivityBuilder EnsureImplementation(ModelService modelService)
        {
            var builder = GetActivityBuilder(modelService);
            var chart = builder?.Implementation as Flowchart;
            if(chart != null)
            {
                EnsureImplementation(builder, chart);
            }
            return builder;
        }

        public static ActivityBuilder GetActivityBuilder(ModelService modelService)
        {
            if (modelService?.Root == null)
            {
                return null;
            }

            var root = modelService.Root.GetCurrentValue();

            var builder = root as ActivityBuilder;
            return builder;
        }

        void EnsureImplementation(ActivityBuilder builder, Flowchart chart)
        {
            FixExpressions(chart);
            SetVariables(chart.Variables);
            SetNamespaces(builder);
        }

        public static ConcurrentDictionary<Guid, TextExpressionCompilerResults> Resultscache { get => resultscache; set => resultscache = value; }
        static ConcurrentDictionary<Guid, TextExpressionCompilerResults> resultscache = GlobalConstants.Resultscache;

        void SetNamespaces(object target)
        {
            var dev2ActivitiesAssembly = typeof(WorkflowHelper).Assembly;
            var dev2CommonAssembly = typeof(GlobalConstants).Assembly;
            var dev2DataAssembly = typeof(Dev2DataListDecisionHandler).Assembly;

            var namespaces = new Dictionary<string, Assembly>
            {
                { "Dev2.Common", dev2CommonAssembly },
                { "Dev2.Data.Decisions.Operations", dev2DataAssembly },
                { "Dev2.Data.SystemTemplates.Models", dev2DataAssembly },
                { "Dev2.DataList.Contract", dev2DataAssembly },
                { "Dev2.DataList.Contract.Binary_Objects", dev2DataAssembly },
                { "Unlimited.Applications.BusinessDesignStudio.Activities", dev2ActivitiesAssembly }
            };

            #region Set C# assembly references

            // http://stackoverflow.com/questions/16086612/wf-4-5-using-c-sharp-expressions-with-external-class-references
            // http://blogs.msdn.com/b/tilovell/archive/2012/05/25/wf4-5-using-csharpvalue-lt-t-gt-and-csharpreference-lt-t-gt-in-net-4-5-compiling-expressions-and-changes-in-visual-studio-generated-xaml.aspx

            TextExpression.SetReferencesForImplementation(target, namespaces.Values.Distinct().Select(a => new AssemblyReference { Assembly = a }).ToArray());

            var impl = new AttachableMemberIdentifier(typeof(TextExpression), "NamespacesForImplementation");

            AttachablePropertyServices.SetProperty(target, impl, namespaces.Keys.ToList());

            #endregion
        }

        void FixExpressions(Flowchart chart, bool isServerInvocation = false)
        {
            foreach(var node in chart.Nodes)
            {
                var fd = node as FlowDecision;
                if(fd != null)
                {
                    var decisionActivity = fd.Condition as DsfFlowDecisionActivity;
                    if(isServerInvocation)
                    {
                        // CompileExpressionsImpl will strip out backslashes!!
                        TryFixExpression(decisionActivity, "\\", "\\\\");
                    }
                }
            }
        }

        void TryFixExpression<TResult>(DsfFlowNodeActivity<TResult> activity, string oldExpr, string newExpr)
        {
            if(!string.IsNullOrEmpty(activity?.ExpressionText))
            {
                activity.ExpressionText = activity.ExpressionText.Replace(oldExpr, newExpr);
            }
        }

        public StringBuilder SanitizeXaml(StringBuilder workflowXaml) => RemoveNodeValue(workflowXaml, "<sads:DebugSymbol.Symbol>");

        StringBuilder RemoveNodeValue(StringBuilder xml, string nodeName)
        {
            if(xml == null || xml.Length == 0)
            {
                return xml;
            }

            var startIdx = xml.IndexOf(nodeName, 0, true);

            if(startIdx == -1)
            {
                return xml;
            }
            startIdx += nodeName.Length;
            var endIdx = xml.IndexOf(nodeName.Insert(1, "/"), startIdx, true);
            var length = endIdx - startIdx;
            return length > 0 ? xml.Remove(startIdx, length) : xml;
        }

        void SetVariables(Collection<Variable> variables)
        {
            try
            {
                if (variables == null)
                {
                    throw new ArgumentNullException(nameof(variables));
                }

                variables.Clear();
                variables.Add(new Variable<List<string>> { Name = "InstructionList" });
                variables.Add(new Variable<string> { Name = "LastResult" });
                variables.Add(new Variable<bool> { Name = "HasError" });
                variables.Add(new Variable<string> { Name = "ExplicitDataList" });
                variables.Add(new Variable<bool> { Name = "IsValid" });
                variables.Add(new Variable<Unlimited.Applications.BusinessDesignStudio.Activities.Util> { Name = "t" });
                variables.Add(new Variable<Dev2DataListDecisionHandler> { Name = "Dev2DecisionHandler" });
            }
            catch (Exception)
            {
                variables = new Collection<Variable>();
                variables.Clear();
            }
        }


        public static bool AreWorkflowsEqual(string left, string right)
        {
            if (left != "")
            {
                var xmlDoc_Left = new XmlDocument();
                xmlDoc_Left.LoadXml(left);
                var xmlDoc_Right = new XmlDocument();
                xmlDoc_Right.LoadXml(right);
                var eq = CompareWorkflows(xmlDoc_Left, xmlDoc_Right);
                return eq;
            }
            return false;
        }
#pragma warning disable S1541 // Methods and properties should not be too complex
        private static bool CompareWorkflows(XmlNode lnode, XmlNode rnode)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (lnode is null || rnode is null)
            {
                var nodeEq = CompareNodeAttributes(lnode, rnode);
                return nodeEq;
            }
            if (SkipNode(lnode))
            {
                return true;
            }

            bool result = lnode.Name.Equals(rnode.Name);
            if (!result)
            {
                return result;
            }
            if (lnode.Name.Equals("#text"))
            {
                result = lnode.InnerText.Equals(rnode.InnerText);
                if (!result)
                {
                    return result;
                }
            }
            if (lnode.HasChildNodes)
            {
                for (var i = 0; i < lnode.ChildNodes.Count; i++)
                {
                    var eq = CompareWorkflows(lnode.ChildNodes[i], rnode.ChildNodes[i]);
                    if (!eq)
                    {
                        return eq;
                    }
                }
            }

            if (!(lnode.Attributes is null))
            {
                result = CompareAttributes(lnode, rnode);
            }

            return result;
        }

        private static bool CompareNodeAttributes(XmlNode lnode, XmlNode rnode)
        {
            var nodeEq = false;
            if (lnode.Name.Equals("dci:NameValue"))
            {
                var leftNodeAttrName = lnode.Attributes[0].Value;
                var leftNodeAttrValue = lnode.Attributes[0].Value;
                if (string.IsNullOrWhiteSpace(leftNodeAttrName) && string.IsNullOrWhiteSpace(leftNodeAttrValue))
                {
                    nodeEq = true;
                }
            }
            else
            {
                nodeEq = lnode == rnode;
            }

            return nodeEq;
        }

        private static bool CompareAttributes(XmlNode lnode, XmlNode rnode)
        {
            if (lnode.Attributes.Count != rnode.Attributes.Count)
            {
                return false;
            }
            for (int i = 0; i < lnode.Attributes.Count; i++)
            {
                var eq = lnode.Attributes[i].Name.Equals(rnode.Attributes[i].Name);
                if (!eq)
                {
                    return eq;
                }
                if (SkipAttribute(lnode.Attributes[i]))
                {
                    continue;
                }
                eq &= lnode.Attributes[i].Value.Equals(rnode.Attributes[i].Value);
                if (!eq)
                {
                    return eq;
                }
            }
            return true;
        }

        static bool SkipAttribute(XmlAttribute attr)
        {
            var eq = false;

            eq |= attr.Name.Equals("sap:VirtualizedContainerService.HintSize");
            eq |= attr.Name.Equals("Capacity");

            return eq;
        }

        static bool SkipNode(XmlNode node)
        {
            var eq = false;

            eq |= node.Name.Equals("av:Size");
            eq |= node.Name.Equals("sap:VirtualizedContainerService.HintSize");
            eq |= node.Name.Equals("av:PointCollection");

            return eq;
        }
    }
}
