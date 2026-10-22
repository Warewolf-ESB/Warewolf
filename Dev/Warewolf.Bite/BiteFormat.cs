/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Text;
using Dev2.Activities.WF;
using Warewolf.Bite.Xaml;

namespace Warewolf.Bite
{
    /// <summary>
    /// Conversions between a workflow's XAML definition and the X6 graph JSON that Web Studio and the MCP tools edit
    /// (WF removal T3.3). Both directions go through <see cref="Dev2.Activities.Flow.FlowchartModel"/>, so neither
    /// loads the XAML through WF nor writes it with WF's XAML writer.
    /// </summary>
    public static class BiteFormat
    {
        /// <summary>X6 graph JSON to a XAML definition that today's engines load unchanged.</summary>
        /// <exception cref="EmptyWorkflowGraphException">The graph has no start node.</exception>
        public static StringBuilder X6ToXaml(string x6Json) =>
            new StringBuilder(BiteXamlWriter.Write(new X6ToWorkflowConverter().X6JsonToModel(x6Json)));

        /// <summary>A Web Studio save request's <c>ResourceJSON</c> argument as a XAML definition; empty without one.</summary>
        public static StringBuilder X6ToXaml(Dictionary<string, StringBuilder> values) =>
            values.TryGetValue("ResourceJSON", out var resourceDefinition) && resourceDefinition?.Length > 0
                ? X6ToXaml(resourceDefinition.ToString())
                : new StringBuilder();

        /// <summary>
        /// A XAML definition as X6 graph JSON. <paramref name="resourceId"/> seeds stable node ids for activities
        /// saved without a UniqueID; without it they are derived from the XAML itself.
        /// </summary>
        /// <exception cref="BiteFormatException">The XAML is malformed or uses a type outside the allowlist.</exception>
        public static string XamlToX6(string xamlDefinition, string? resourceId = null) =>
            new WorkflowToX6Converter().ConvertToX6Json(
                new BiteReader().ReadModel(xamlDefinition, resourceId ?? BiteReader.SeedForXaml(xamlDefinition)),
                xamlDefinition);
    }
}
