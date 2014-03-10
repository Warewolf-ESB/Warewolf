using Dev2;
using Dev2.Activities;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using System;
using System.Activities;
using System.Collections.Generic;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfCommentActivity : DsfActivityAbstract<string>
    {

        public DsfCommentActivity()
        {
            DisplayName = "Comment";
        }
        public string Text { get; set; }


        #region Get Debug Inputs/Outputs

        public List<DebugItem> GetDebugInputs()
        {
            return DebugItem.EmptyList;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            List<DebugItem> result = new List<DebugItem>();
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = Text });
            result.Add(itemToAdd);
            return result;
        }

        #endregion Get Inputs/Outputs


        #region GetForEachInputs/Outputs

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            InitializeDebug(context.GetExtension<IDSFDataObject>());
            DispatchDebugState(context, StateType.Before);

            DispatchDebugState(context, StateType.After);
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null && updates.Count == 1)
            {
                Text = updates[0].Item2;
            }
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return DsfForEachItem.EmptyList;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return new List<DsfForEachItem>
            {
                new DsfForEachItem
                {
                    Value = Text,
                    Name = Text
                }
            };
        }

        #endregion
    }
}
