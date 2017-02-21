/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Interfaces;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    public class TestActivity : DsfNativeActivity<string>
    {

        public TestActivity()
            : this(new Mock<IDebugDispatcher>().Object)
        {
        }

        public TestActivity(IDebugDispatcher dispatcher)
            : base(false, "TestActivity", dispatcher)
        {
            UniqueGuid = Guid.NewGuid();
            UniqueID = UniqueGuid.ToString();
            IsWorkflow = true;
            IsSimulationEnabled = false;
        }

        public override List<string> GetOutputs()
        {
            return new List<string>();
        }
        public Guid UniqueGuid { get; private set; }
        public bool ErrorExecute { get; set; }

        protected override void OnExecute(NativeActivityContext context)
        {
            if (ErrorExecute)
            {
                throw new Exception("Error in execution");
            }
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

        public IDebugState TestInitializeDebugState(StateType stateType, IDSFDataObject dataObject, Guid remoteID, bool hasError, string errorMessage)
        {
            InitializeDebugState(stateType, dataObject, remoteID, hasError, errorMessage);
            return GetDebugState();
        }

        public IDebugState TestDispatchDebugState(IDSFDataObject dataObject, StateType stateType)
        {
            DispatchDebugState(dataObject, stateType,0);
            return DebugState;
        }
    }

    
}
