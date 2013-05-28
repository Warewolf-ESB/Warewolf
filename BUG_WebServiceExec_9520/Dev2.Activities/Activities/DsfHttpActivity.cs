using System;
using System.Linq;
using System.Activities;
using System.Collections.Generic;
using Dev2.Common;



namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public class DsfHttpActivity : DsfActivityAbstract<bool> 
    {

        DsfAssignActivity _assignActivity;
        ActivityAction _delegate;
        string _httpMethod = "GET";

        public string HttpMethod {
            get {
                return _httpMethod;
            }
            set {
                string[] acceptable = new string[] { "GET", "POST" };

                if (!acceptable.Any(c => c.Equals(value, StringComparison.InvariantCultureIgnoreCase))){
                    throw new ArgumentOutOfRangeException("HttpMethod");
                }

                _httpMethod = value;
            }
        }
        public string FieldName { get; set; }
        public string Uri { get; set; }
        public string PostData { get; set; }

        public DsfHttpActivity() {
            _delegate = new ActivityAction {
                DisplayName = "AssignHttpWebRequest",
            };
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata) {
            base.CacheMetadata(metadata);
            _assignActivity = new DsfAssignActivity();
            metadata.AddChild(_assignActivity);
            metadata.AddDelegate(_delegate);
            
        }


        protected override void OnExecute(System.Activities.NativeActivityContext context) {
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
