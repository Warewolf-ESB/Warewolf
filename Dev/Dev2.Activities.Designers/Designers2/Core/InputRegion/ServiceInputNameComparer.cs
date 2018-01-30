using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Activities.Designers2.Core.InputRegion
{
    class ServiceInputNameComparer : IEqualityComparer<IServiceInput>
    {
        public bool Equals(IServiceInput x, IServiceInput y) => x.Name.Equals(y.Name);

        public int GetHashCode(IServiceInput obj) => obj.GetHashCode();
    }
}