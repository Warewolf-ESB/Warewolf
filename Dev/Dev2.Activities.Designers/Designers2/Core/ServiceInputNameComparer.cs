using Dev2.Common.Interfaces.DB;
using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace Dev2.Activities.Designers2.Core
{
    public class ServiceInputNameComparer : IEqualityComparer<IServiceInput>
    {
        public bool Equals(IServiceInput x, IServiceInput y)
        {
            return x.Name.Equals(y.Name, StringComparison.CurrentCulture);
        }

        public int GetHashCode(IServiceInput obj)
        {
            return 1;
        }
    }
}
