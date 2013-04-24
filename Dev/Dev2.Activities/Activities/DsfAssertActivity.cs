using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Common;
using Microsoft.VisualBasic.Activities;


namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class AssertActivity : DsfActivityAbstract<bool>
    {
        public string DataTags { get; set; }
        public string DataExpression { get; set; }



        public AssertActivity()
            : base()
        {
            Result = new VisualBasicReference<bool> { ExpressionText = "IsValid" };
        }

        protected override void OnExecute(NativeActivityContext context)
        {
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
