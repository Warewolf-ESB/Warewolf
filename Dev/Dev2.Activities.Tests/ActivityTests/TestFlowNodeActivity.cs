
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    // BUG 9304 - 2013.05.08 - TWR - Created this test class

    public class TestFlowNodeActivity<TResult> : DsfFlowNodeActivity<TResult>
    {
        public TestFlowNodeActivity()
            : base("TestFlowNode", new Mock<IDebugDispatcher>().Object, false)
        {
            UniqueID = Guid.NewGuid().ToString();
            IsWorkflow = true;
            IsSimulationEnabled = false;
        }

        #region Overrides of DsfNativeActivity<TResult>

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
        }

        #endregion

    }
}
