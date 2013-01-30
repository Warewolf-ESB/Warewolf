using System;
using System.Collections.Generic;
using System.Linq;
using System.Activities;
using Dev2;
using Dev2.Common;


namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public class DsfRemoveActivity : DsfActivityAbstract<bool> {
        public string RemoveText { get; set; }

        public DsfRemoveActivity() : base("Remove") {

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
