using System.Activities.Statements;
using System.Windows;
using Microsoft.VisualBasic.Activities;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfFlowDecisionActivity : DsfFlowNodeActivity<bool>
    {
        #region Ctor

        public DsfFlowDecisionActivity()
            : base("Decision")
        {
        }

        #endregion

        #region CreateFlowNode

        protected override FlowNode CreateFlowNode()
        {
            var flowNode = new FlowDecision(this);
            return flowNode;
        }

        #endregion

        public override void UpdateForEachInputs(System.Collections.Generic.IList<System.Tuple<string, string>> updates, System.Activities.NativeActivityContext context)
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateForEachOutputs(System.Collections.Generic.IList<System.Tuple<string, string>> updates, System.Activities.NativeActivityContext context)
        {
            throw new System.NotImplementedException();
        }
    }



}
