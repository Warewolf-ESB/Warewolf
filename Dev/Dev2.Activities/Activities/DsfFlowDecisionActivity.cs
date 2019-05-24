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
using Dev2.Activities;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Warewolf.Core;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    [ToolDescriptorInfo("ControlFlow-Descision", "Decision", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Control Flow", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Flow_Decision")]
    public class DsfFlowDecisionActivity : DsfFlowNodeActivity<bool>
    {
        public DsfFlowDecisionActivity()
            : base("Decision")
        {
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            throw new NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            throw new NotImplementedException();
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
        }

        public void SetDebugOutputs(List<DebugItem> result)
        {
            _debugOutputs = result;
        }

        public void SetDebugInputs(List<DebugItem> val)
        {
            _debugInputs = val;
        }

        public override List<string> GetOutputs() => new List<string>();

        public override IEnumerable<StateVariable> GetState()
        {
            return new StateVariable[0];
        }
    }
}
