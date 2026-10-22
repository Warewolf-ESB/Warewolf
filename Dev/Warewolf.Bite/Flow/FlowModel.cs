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

namespace Warewolf.Bite.Flow
{
    /// <summary>
    /// The control-flow skeleton of a .bite <c>Flowchart</c>, as plain objects rather than WF <c>FlowNode</c>s.
    /// Successors are filled in after the whole document is read, so <c>x:Reference</c> may point forwards.
    /// </summary>
    public abstract class FlowNodeModel
    {
        public string? Name { get; set; }
    }

    public sealed class FlowchartModel
    {
        public FlowNodeModel? StartNode { get; set; }
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
    }
}
