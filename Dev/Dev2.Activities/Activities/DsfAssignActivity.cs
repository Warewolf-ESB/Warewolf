using Dev2.Activities;
using Dev2.Diagnostics;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // TODO: DELETE UNUSED
    public class DsfAssignActivity : DsfActivityAbstract<string>
    {

        public string FieldName { get; set; }
        public string FieldValue { get; set; }

        public bool UpdateAllOccurrences { get; set; }
        public bool CreateBookmark { get; set; }
        public string ServiceHost { get; set; }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        public DsfAssignActivity()
            : base("Assign")
        {
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }



        protected override void OnExecute(NativeActivityContext context)
        {
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


        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return DsfForEachItem.EmptyList;
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After, FieldValue);
        }

        #endregion

    }
}
