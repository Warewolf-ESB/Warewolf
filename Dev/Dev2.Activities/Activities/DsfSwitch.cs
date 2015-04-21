using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Data.SystemTemplates.Models;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities
{
    public class DsfSwitch:DsfActivityAbstract<string>
    {

        public IEnumerable<IDev2Activity> Switches { get; set; }
        public IEnumerable<IDev2Activity> Default { get; set; }
        public Dev2DecisionStack Conditions { get; set; }

        #region Overrides of DsfNativeActivity<string>

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
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

        protected override void ExecuteTool(IDSFDataObject dataObject)
        {
        }

        #endregion
    }
}