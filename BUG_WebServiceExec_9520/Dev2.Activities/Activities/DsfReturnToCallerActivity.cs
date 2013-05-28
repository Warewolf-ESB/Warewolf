using System;
using System.Collections.Generic;
using System.Text;
using System.Activities;
using Dev2;
using System.Net;
using System.IO;
using Dev2.Common;


namespace Unlimited.Applications.BusinessDesignStudio.Activities {

    public class DsfReturnToCallerActivity : DsfActivityAbstract<bool> {

        protected override bool CanInduceIdle {
            get {
                return true;
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata) {
            base.CacheMetadata(metadata);
            //metadata.AddDelegate(_delegate);
        }

        public DsfReturnToCallerActivity() : base() {}


        //IDSFDataObject dataObject;

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
