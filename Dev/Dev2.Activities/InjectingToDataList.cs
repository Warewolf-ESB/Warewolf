using System;
using System.Collections.Generic;
using System.Activities;
using Microsoft.VisualBasic.Activities;
using Dev2.DataList.Contract.Binary_Objects;


namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public class InjectingToDataList : DsfActivityAbstract<bool> {

        public string ServiceHost { get; set; }


        public InjectingToDataList() {
            AmbientDataList = new VisualBasicReference<List<string>> { ExpressionText = "AmbientDataList" };
        }

        public void InjectToDataList(string eval, string FieldName, NativeActivityContext context) {

            
        }

        protected override void OnExecute(NativeActivityContext test) {
            throw new NotImplementedException("???");
        }

        #region Overridden ActivityAbstact Methods

        public override IBinaryDataList GetInputs()
        {
            return null;
        }

        public override IBinaryDataList GetOutputs()
        {
            return null;
        }

        #endregion Overridden ActivityAbstact Methods

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
