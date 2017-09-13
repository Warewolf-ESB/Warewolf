using System;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Comparer
{
    internal class FlowSwitchActivityComparer : IEqualityComparer<DsfFlowSwitchActivity>
    {
        public bool Equals(DsfFlowSwitchActivity x, DsfFlowSwitchActivity y)
        {
            if (x == null && y == null) return true;
            if ((x == null && y != null) || (x != null && y == null)) return false;
            return x != null
                && y != null
                && string.Equals(x.DisplayName, y.DisplayName)
                && string.Equals(x.ExpressionText, y.ExpressionText);
        }
        public int GetHashCode(DsfFlowSwitchActivity obj)
        {
            return 1;
        }
    }
}
