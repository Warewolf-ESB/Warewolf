using System.Activities;
using System.Collections.Generic;
using Dev2.Common;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Comparer
{
    internal class ActivityFuncComparer : IEqualityComparer<ActivityFunc<string, bool>>
    {
        public bool Equals(ActivityFunc<string, bool> x, ActivityFunc<string, bool> y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            IEqualityComparer<DelegateArgument> argumentComparer = new DelegateArgumentComparer();
            var argumentsAreEqual = argumentComparer.Equals(x.Argument, y.Argument);
            var resultAreEqual = argumentComparer.Equals(x.Result, y.Result);

            var @equals = string.Equals(x.DisplayName, y.DisplayName);
            var handlerActivityIsEqual = CommonEqualityOps.AreObjectsEqualUnSafe(x.Handler, y.Handler);
           

            //var handlerActivityIsEqual = x.Handler?.Equals(y.Handler) ?? true;//All activities have an implemantation of Equals at this stage :)
            return equals && argumentsAreEqual && resultAreEqual && handlerActivityIsEqual;

        }

        public int GetHashCode(ActivityFunc<string, bool> obj)
        {
            return obj.GetHashCode();
        }
    }
}
