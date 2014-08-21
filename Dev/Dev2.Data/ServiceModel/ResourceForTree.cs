using System;
using Dev2.Common.Interfaces.Data;

// ReSharper disable CheckNamespace
namespace Dev2.Runtime.ServiceModel.Data
{
    public class ResourceForTree : IComparable<ResourceForTree>, IResourceForTree
    {
        public Guid UniqueID { get; set; }
        public Guid ResourceID { get; set; }
        public String ResourceName { get; set; }
        public ResourceType ResourceType { get; set; }

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return ResourceName;
        }

        #endregion

        #region Implementation of IComparable<in ResourceForTree>

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(ResourceForTree other)
        {
            return ResourceID.CompareTo(other.ResourceID);
        }

        #endregion
    }
}