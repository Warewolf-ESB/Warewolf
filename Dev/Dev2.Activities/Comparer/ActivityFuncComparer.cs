using System.Activities;
using System.Collections.Generic;

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
            bool sourceIsSame;
            if (x.Handler == null && y.Handler == null)
            {
                sourceIsSame = true;
            }

            else if (x.Handler == null || y.Handler == null)
            {
                sourceIsSame = false;
            }
            else
            {
                sourceIsSame = x.Handler.Equals(y.Handler);
            }
            if (!sourceIsSame)
            {
                return false;
            }

            var handlerActivityIsEqual = x.Handler?.Equals(y.Handler) ?? true;//All activities have an implemantation of Equals at this stage :)
            return equals && argumentsAreEqual && resultAreEqual && handlerActivityIsEqual;

        }

        public int GetHashCode(ActivityFunc<string, bool> obj)
        {
            return obj.GetHashCode();
        }
    }
}
