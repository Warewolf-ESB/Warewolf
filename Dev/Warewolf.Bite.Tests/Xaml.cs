/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2;

namespace Warewolf.Bite.Tests
{
    /// <summary>Small hand-written workflows in the .bite XAML dialect, one rule per test.</summary>
    static class Xaml
    {
        const string Namespaces =
            "xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" " +
            "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" " +
            "xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" " +
            "xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" " +
            "xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" " +
            "xmlns:da=\"clr-namespace:Dev2.Activities;assembly=Dev2.Activities\" " +
            "xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\"";

        public const string DecisionExpression =
            "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(&quot;{!TheStack!:[{!Col1!:![[a]]!,!Col2!:!1!,!Col3!:!!,!PopulatedColumnCount!:2,!EvaluationFn!:!IsEqual!}],!TotalDecisions!:1,!ModelName!:!Dev2DecisionStack!,!Mode!:!AND!,!TrueArmText!:!True!,!FalseArmText!:!False!}&quot;,AmbientDataList)";

        public const string SwitchExpression =
            "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(&quot;[[a]]&quot;,AmbientDataList)";

        /// <summary>An &lt;Activity&gt; root whose implementation is a Flowchart with the given content.</summary>
        public static string Workflow(string flowchartContent, string extraNamespaces = "") =>
            $"<Activity x:Class=\"Test\" {Namespaces} {extraNamespaces}><Flowchart>{flowchartContent}</Flowchart></Activity>";

        public static string Comment(string uniqueId, string displayName = "c", string extra = "") =>
            $"<uaba:DsfCommentActivity DisplayName=\"{displayName}\" UniqueID=\"{uniqueId}\" {extra} />";

        public static string Decision(string uniqueId) =>
            $"<uaba:DsfFlowDecisionActivity DisplayName=\"d\" ExpressionText=\"{DecisionExpression}\" UniqueID=\"{uniqueId}\" />";

        public static IDev2Activity? Read(string xaml, string resourceId = "11111111-1111-1111-1111-111111111111") =>
            new BiteReader().ReadXaml(xaml, resourceId);
    }
}
