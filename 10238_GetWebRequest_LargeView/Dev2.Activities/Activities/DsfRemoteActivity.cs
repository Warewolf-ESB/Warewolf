using System;
using System.Collections.Generic;
using System.Activities;


namespace Unlimited.Applications.BusinessDesignStudio.Activities {

    public class DsfRemoteActivity : DsfActivityAbstract<bool> {
        public string ServiceAddress { get; set; }

        protected override bool CanInduceIdle {
            get {
                return true;
            }
        }

        public DsfRemoteActivity() : base() {}

        protected override void CacheMetadata(NativeActivityMetadata metadata) {
            base.CacheMetadata(metadata);
        }

        protected override void OnExecute(NativeActivityContext context) {
            throw new NotImplementedException("Nothing here...");
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
