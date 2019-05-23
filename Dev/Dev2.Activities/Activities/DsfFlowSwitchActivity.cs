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
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Diagnostics;
using Dev2.Interfaces;
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
    }
}
