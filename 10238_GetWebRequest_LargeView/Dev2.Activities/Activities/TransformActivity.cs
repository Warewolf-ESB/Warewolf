using System;
using System.Collections.Generic;
using System.Activities;


namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public class TransformActivity : DsfActivityAbstract<bool> {

        public string Transformation { get; set; }
        public string TransformElementName { get; set; }
        public bool Aggregate { get; set; }
        public string RootTag { get; set; }
        public bool RemoveSourceTagsAfterTransformation { get; set; }


        public TransformActivity() : base() {}

        protected override void CacheMetadata(NativeActivityMetadata metadata) {
            base.CacheMetadata(metadata);
        }



        protected override void OnExecute(NativeActivityContext context) {
            throw new NotImplementedException("TransformActivity Deprecated");
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
