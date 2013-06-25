using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using Microsoft.VisualBasic.Activities;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfFlowSwitchActivity : DsfFlowNodeActivity<string>
    {
        #region Ctor

        public DsfFlowSwitchActivity()
            : base("Switch")
        {
            //_expression = new VisualBasicValue<string>();
        }

        #endregion

        #region CreateFlowNode

        protected override FlowNode CreateFlowNode()
        {
            return new FlowSwitch<string>
            {
                Expression = this
            };
        }

        #endregion

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
