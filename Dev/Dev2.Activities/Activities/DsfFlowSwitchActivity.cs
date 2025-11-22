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
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Common.X6;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Newtonsoft.Json;
using Warewolf.Core;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    [ToolDescriptorInfo("ControlFlow-Switch", "Switch", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Control Flow", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Flow_Switch")]
    public class DsfFlowSwitchActivity : DsfFlowNodeActivity<string>
    {
        public DsfFlowSwitchActivity()
            : base("Switch")
        {
        }

        public DsfFlowSwitchActivity(string displayName, IDebugDispatcher debugDispatcher)
            : this(displayName, debugDispatcher, false)
        {
        }

        public DsfFlowSwitchActivity(string displayName, IDebugDispatcher debugDispatcher, bool isAsync)
            : base(displayName, debugDispatcher, isAsync)
        {
        }

        public override List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)> ArmConnectors()
        {
            var armConnectors = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            return armConnectors;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
        }

        public void SetDebugInputs(List<DebugItem> debugInputs)
        {
            _debugInputs = debugInputs;
        }

        public void SetDebugOutputs(List<DebugItem> debugOutputs)
        {
            _debugOutputs = debugOutputs;
        }

        public override List<string> GetOutputs() => new List<string>();

        public override IEnumerable<StateVariable> GetState()
        {
            return new StateVariable[0];
        }

        public override void FromX6Json(Dev2.Common.X6.Cell node)
        {
            if (node == null || node.data == null) return;

            base.FromX6Json(node);

            var displayName = "Switch";
            if (node.data.TryGetValue(Constants.DISPLAYTEXT, out var displayObject) && displayObject is string displayText && !string.IsNullOrWhiteSpace(displayText))
            {
                displayName = displayText;
            }

            this.DisplayName = displayName;

            // Extract switch variable from node data
            if (node.data.TryGetValue("switchVariable", out var switchVarObj) && switchVarObj is string switchVariable)
            {
                // Create the proper expression text format for switch
                this.ExpressionText = string.Join("", GlobalConstants.InjectedSwitchDataFetch,
                                                     "(\"", switchVariable, "\",",
                                                     GlobalConstants.InjectedDecisionDataListVariable,
                                                     ")");
            }

            if (node.data.TryGetValue("switchExpression", out var switchExprObj) && switchExprObj is string switchExprJson)
            {
                try
                {
                    var switchExpression = JsonConvert.DeserializeObject<dynamic>(switchExprJson);

                    // Extract switch variable from expression if not already set
                    if (string.IsNullOrEmpty(this.ExpressionText) && switchExpression?.SwitchVariable != null)
                    {
                        var switchVar = switchExpression.SwitchVariable.ToString();
                        this.ExpressionText = string.Join("", GlobalConstants.InjectedSwitchDataFetch,
                                                             "(\"", switchVar, "\",",
                                                             GlobalConstants.InjectedDecisionDataListVariable,
                                                             ")");
                    }
                }
                catch (JsonException)
                {
                    // If JSON parsing fails, continue with default values for
                }
            }

            // Set other properties if available
            this.UniqueID = node.data.TryGetValue("UniqueID", out var uniqueIdObj) && uniqueIdObj is string uniqueId
                ? uniqueId
                : Guid.NewGuid().ToString();

        }

        public override void ToX6Json(Cell cell)
        {
            if (cell.data == null) cell.data = new Dictionary<string, object>();

            base.ToX6Json(cell);
        }
    }
}
