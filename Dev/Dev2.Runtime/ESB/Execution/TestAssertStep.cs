using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Runtime.ESB.Execution
{
    public class TestAssertStep : DsfActivityAbstract<string>
    {
        private readonly IDev2Activity _originalActivity;
        private readonly List<IServiceTestOutput> _testOutputs;

        public TestAssertStep(IDev2Activity originalActivity, List<IServiceTestOutput> testOutputs)
        {
            _originalActivity = originalActivity;
            _testOutputs = testOutputs;
            var act = originalActivity as DsfBaseActivity;
            if (act != null)
                DisplayName = act.DisplayName;
        }


        #region Overrides of DsfNativeActivity<string>

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public IDev2Activity ExecuteToolWithAsserts(IDSFDataObject dataObject, int update)
        {
            var dev2Activity = _originalActivity.Execute(dataObject, update);
            foreach(var debugOutput in _testOutputs)
            {
               //to do assertion here
            }
            return dev2Activity;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return null;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return null;
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
        }

        public override List<string> GetOutputs()
        {
            return null;
        }

        #endregion
    }
}