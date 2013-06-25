using System.Collections.Generic;
using System.Linq;
using System.Activities;
using Dev2;
using System.Text.RegularExpressions;
using Dev2.Common;


namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public sealed class DsfTagCountActivity : DsfActivityAbstract<bool> {

        public string TagName { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata) {

            base.CacheMetadata(metadata);
        }

        public DsfTagCountActivity() : base() {

        }

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
