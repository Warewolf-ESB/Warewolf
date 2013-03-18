using Dev2.Activities;
using Dev2.Diagnostics;
using System.Activities;
using System.Collections.Generic;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public sealed class DsfCommentActivity : CodeActivity
    {

        public DsfCommentActivity()
        {
            DisplayName = "Comment";
        }
        public string Text { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            // Emtpy for a reason...
        }

        #region Get Debug Inputs/Outputs

        public IList<IDebugItem> GetDebugInputs()
        {
            return DebugItem.EmptyList;
        }

        public IList<IDebugItem> GetDebugOutputs()
        {
            IList<IDebugItem> result = new List<IDebugItem>();
            DebugItem itemToAdd = new DebugItem();            
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = Text });
            result.Add(itemToAdd);
            return result;
        }

        #endregion Get Inputs/Outputs


        #region GetForEachInputs/Outputs

        public IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return DsfForEachItem.EmptyList;
        }

        public IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return new List<DsfForEachItem>
            {
                new DsfForEachItem
                {
                    Value = Text
                }
            };
        }

        #endregion
    }
}
