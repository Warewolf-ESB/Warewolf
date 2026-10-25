/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

#nullable enable

using System.Collections.Generic;

namespace Dev2.Activities.Flow
{
    /// <summary>
    /// A workflow's definition as authored: the control-flow skeleton of its Flowchart, as plain objects rather than
    /// WF <c>FlowNode</c>s, holding the activities themselves. Readers (XAML, the JSON body) produce it, writers and
    /// the X6 converters consume it, and <c>FlowGraphBuilder</c> links it into the executable graph.
    /// Successors may be filled in after all nodes exist, so references can point forwards.
    /// </summary>
    public abstract class FlowNodeModel
    {
        /// <summary>The node's name (XAML <c>x:Name</c>, JSON node id), when it has one.</summary>
        public string? Name { get; set; }
    }

    public sealed class FlowchartModel
    {
        /// <summary>The workflow's name: XAML <c>x:Class</c>.</summary>
        public string? Name { get; set; }

        public FlowNodeModel? StartNode { get; set; }

        /// <summary>
        /// Every node the flowchart declares, reachable or not, in document order. Execution only walks from
        /// <see cref="StartNode"/>; the rest is designer content, which a writer must not drop.
        /// </summary>
        public List<FlowNodeModel> Nodes { get; } = new List<FlowNodeModel>();
    }

    public sealed class FlowStepModel : FlowNodeModel
    {
        public object? Action { get; set; }
        public FlowNodeModel? Next { get; set; }
    }

    public sealed class FlowDecisionModel : FlowNodeModel
    {
        public string? DisplayName { get; set; }
        public object? Condition { get; set; }
        public FlowNodeModel? True { get; set; }
        public FlowNodeModel? False { get; set; }
    }

    public sealed class FlowSwitchModel : FlowNodeModel
    {
        public string? DisplayName { get; set; }
        public object? Expression { get; set; }
        public FlowNodeModel? Default { get; set; }

        /// <summary>Cases in document order, as WF's <c>FlowSwitch.Cases</c> dictionary would hold them.</summary>
        public List<KeyValuePair<string, FlowNodeModel?>> Cases { get; } = new List<KeyValuePair<string, FlowNodeModel?>>();

        /// <summary><c>Cases[key] = target</c>, as on WF's dictionary: replaces the key's target, or adds it last.</summary>
        public void SetCase(string key, FlowNodeModel? target)
        {
            var entry = new KeyValuePair<string, FlowNodeModel?>(key, target);
            var index = Cases.FindIndex(c => c.Key == key);
            if (index >= 0)
            {
                Cases[index] = entry;
            }
            else
            {
                Cases.Add(entry);
            }
        }
    }
}
