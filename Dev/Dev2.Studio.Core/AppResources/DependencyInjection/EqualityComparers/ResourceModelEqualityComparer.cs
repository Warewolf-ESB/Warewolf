using System;
using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers
{
    public class ResourceModelEqualityComparer : IEqualityComparer<IResourceModel>
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<ResourceModelEqualityComparer> _current
            = new Lazy<ResourceModelEqualityComparer>(() => new ResourceModelEqualityComparer());

        private ResourceModelEqualityComparer()
        {

        }

        public static ResourceModelEqualityComparer Current
        {
            get
            {
                return _current.Value;
            }
        }

        public bool Equals(IResourceModel x, IResourceModel y)
        {

            //Check whether the compared objects reference the same data.
            if(ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if(ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.ID == y.ID;
        }

        public int GetHashCode(IResourceModel obj)
        {
            //Check whether the object is null
            if(ReferenceEquals(obj, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashProductName = obj.ResourceName == null ? 0 : obj.ResourceName.GetHashCode();

            //Get hash code for the Code field.
            return hashProductName;
        }
    }
}
