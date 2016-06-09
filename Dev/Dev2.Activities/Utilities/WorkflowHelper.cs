
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.Decision;
using Microsoft.VisualBasic.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Utilities
{
    // BUG 9304 - 2013.05.08 - TWR - .NET 4.5 upgrade
    public class WorkflowHelper : IWorkflowHelper
    {
        #region Singleton Instance

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a syncRoot 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //

        // NOTE : This singleton instance causes memory leaks ;)


        #endregion

        #region SerializeWorkflow

        public StringBuilder SerializeWorkflow(ModelService modelService)
        {
            var builder = EnsureImplementation(modelService);
            var text = GetXamlDefinition(builder);

            return text;
        }

        public StringBuilder GetXamlDefinition(ActivityBuilder builder)
        {
            StringBuilder text = new StringBuilder();
            try
            {
                if(builder != null)
                {
                    var sb = new StringBuilder();
                    using(var sw = new StringWriter(sb))
                    {
                        var xamlXmlWriterSettings = new XamlXmlWriterSettings();
                        xamlXmlWriterSettings.AssumeValidInput = true;
                        var xw = ActivityXamlServices.CreateBuilderWriter(new XamlXmlWriter(sw, new XamlSchemaContext(),xamlXmlWriterSettings));                    
                        XamlServices.Save(xw, builder);
                        text = sb.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
                    }
                }
                text = SanitizeXaml(text);
            }
            catch(Exception e)
            {
                Dev2Logger.Error("Error loading XAML: ",e);
            }
            return text;
        }

        #endregion

        #region CreateWorkflow

        public ActivityBuilder CreateWorkflow(string displayName)
        {
            if(string.IsNullOrEmpty(displayName))
            {
                throw new ArgumentNullException("displayName");
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

        #endregion

        #region EnsureImplementation

        public ActivityBuilder EnsureImplementation(ModelService modelService)
        {
            if(modelService == null || modelService.Root == null)
            {
                return null;
            }

            var root = modelService.Root.GetCurrentValue();

            var builder = root as ActivityBuilder;
            if(builder != null)
            {
                var chart = builder.Implementation as Flowchart;
                if(chart != null)
                {
                    EnsureImplementation(builder, chart);
                }
            }
            return builder;
        }

        void EnsureImplementation(ActivityBuilder builder, Flowchart chart)
        {
            SetProperties(builder.Properties);
            FixExpressions(chart);
            SetVariables(chart.Variables);
            SetNamespaces(builder);
        }

        #endregion

        #region CompileExpressionsImpl

        public static ConcurrentDictionary<Guid, TextExpressionCompilerResults> Resultscache = GlobalConstants.Resultscache;
       
 
        #endregion


        #region SetProperties

        public void SetProperties(ICollection<DynamicActivityProperty> properties)
        {
            if(properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            properties.Clear();

            properties.Add(new DynamicActivityProperty { Name = "AmbientDataList", Type = typeof(InOutArgument<List<string>>) });
            properties.Add(new DynamicActivityProperty { Name = "ParentWorkflowInstanceId", Type = typeof(InOutArgument<Guid>) });
            properties.Add(new DynamicActivityProperty { Name = "ParentServiceName", Type = typeof(InOutArgument<string>) });
        }

        #endregion

        #region SetVariables

        public void SetVariables(Collection<Variable> variables)
        {
            if(variables == null)
            {
                throw new ArgumentNullException("variables");
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

        #endregion

        #region SetNamespaces

        public void SetNamespaces(object target)
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

            #region Set VB assembly references

            var vbSettings = VisualBasic.GetSettings(target) ?? new VisualBasicSettings();
            vbSettings.ImportReferences.Clear();

            foreach(var ns in namespaces.Keys)
            {
                vbSettings.ImportReferences.Add(new VisualBasicImportReference { Assembly = namespaces[ns].GetName().Name, Import = ns });
            }

            VisualBasic.SetSettings(target, vbSettings);

            #endregion
        }

        #endregion

        #region FixExpressions

        void FixExpressions(Flowchart chart, bool isServerInvocation = false)
        {
            foreach(var node in chart.Nodes)
            {
                var fd = node as FlowDecision;
                if(fd != null)
                {
                    var decisionActivity = fd.Condition as DsfFlowDecisionActivity;
                    //TryFixExpression(decisionActivity, GlobalConstants.InjectedDecisionHandlerOld, GlobalConstants.InjectedDecisionHandler);
                    if(isServerInvocation)
                    {
                        // CompileExpressionsImpl will strip out backslashes!!
                        TryFixExpression(decisionActivity, "\\", "\\\\");
                    }
                }

                //var fs = node as FlowSwitch<string>;
                //if(fs != null)
                //{
                //    TryFixExpression(fs.Expression as DsfFlowSwitchActivity, GlobalConstants.InjectedSwitchDataFetchOld, GlobalConstants.InjectedSwitchDataFetch);
                //}
            }
        }

        void TryFixExpression<TResult>(DsfFlowNodeActivity<TResult> activity, string oldExpr, string newExpr)
        {
            if(activity != null)
            {
                if(!string.IsNullOrEmpty(activity.ExpressionText))
                {
                    activity.ExpressionText = activity.ExpressionText.Replace(oldExpr, newExpr);
                }
            }
        }

        #endregion

        #region SanitizeXaml

        public StringBuilder SanitizeXaml(StringBuilder workflowXaml)
        {
            // A rehosted designer generates the following exception when trying to export (serialize) a workflow:
            //
            //    System.Xaml.XamlDuplicateMemberException: 'Symbol' property has already been set on 'Flowchart' 
            //
            // Clearing the following element resolves the issue
            return RemoveNodeValue(workflowXaml, "<sads:DebugSymbol.Symbol>");
        }

        #endregion

        #region RemoveNodeValue

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

        #endregion

        #region ToNamespaceType

        public string ToNamespaceTypeString(Type activityType)
        {
            // Name must be in the form Namespace.Type (and does not have to be related to the actual activity!)
            return string.Format("{0}.{1}", activityType.Namespace, activityType.Name.Replace("`", ""));
        }

        #endregion
    }
}
