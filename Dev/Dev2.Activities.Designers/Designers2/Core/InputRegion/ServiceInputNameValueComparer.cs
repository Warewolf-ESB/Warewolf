using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Activities.Designers2.Core.InputRegion
{
    internal class ServiceInputNameValueComparer : IEqualityComparer<IServiceInput>
    {
        public bool Equals(IServiceInput x, IServiceInput y)
        {
            return x.Value.Equals(y.Value) && x.Name.Equals(y.Name);
        }

        public int GetHashCode(IServiceInput obj)
        {
            return obj.GetHashCode();
        }
    }
}