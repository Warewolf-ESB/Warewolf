using System;
using System.Collections.Generic;
using Dev2.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers
{
    public class WebsiteLayoutObjectViewModelEqualityComparer : IEqualityComparer<ILayoutObjectViewModel>
    {
        public bool Equals(ILayoutObjectViewModel x, ILayoutObjectViewModel y)
        {
            return x.WebpartServiceDisplayName.Equals(y.WebpartServiceDisplayName, StringComparison.InvariantCultureIgnoreCase)
                && x.WebpartServiceName.Equals(y.WebpartServiceName, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(ILayoutObjectViewModel obj)
        {
            return obj.WebpartServiceDisplayName.ToLower().GetHashCode() + obj.WebpartServiceName.ToLower().GetHashCode();
        }
    }
}
