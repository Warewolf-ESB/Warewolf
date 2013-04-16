using System.Collections.Generic;
using System.Activities;
using Dev2.Common;


namespace Unlimited.Applications.BusinessDesignStudio.Activities.Activities {

    public sealed class DsfDataValidationActivity : DsfActivityAbstract<bool> {

        public IDictionary<string, string> DataValidationMap { get; set; }


        protected override void OnExecute(NativeActivityContext context) {
            throw new System.NotImplementedException(GlobalConstants.NoLongerSupportedMsg);
        }

        public override void UpdateForEachInputs(IList<System.Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<System.Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
