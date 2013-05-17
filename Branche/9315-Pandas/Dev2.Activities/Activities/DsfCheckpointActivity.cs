using System;
using System.Collections.Generic;
using System.Activities;
using Dev2;
using Dev2.Common;


namespace Unlimited.Applications.BusinessDesignStudio.Activities {

    public sealed class DsfCheckpointActivity : DsfActivityAbstract<bool> {
        public DsfCheckpointActivity() : base("Checkpoint") {

        }

        public string IconPath {
            get {
                return "http://localhost/businessdesignstudio/images/checkpoint.png";
            }
        }

        protected override void OnExecute(NativeActivityContext context) {
            throw new NotImplementedException(GlobalConstants.NoLongerSupportedMsg);
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }
    }
}
