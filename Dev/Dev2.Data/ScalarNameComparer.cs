using System;
using System.Collections.Generic;

namespace Dev2.Data
{
    public class ScalarNameComparer : IEqualityComparer<IScalar>
    {
        #region Implementation of IEqualityComparer<in IScalar>

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object of type <paramref name="x"/> to compare.</param><param name="y">The second object of type <paramref name="y"/> to compare.</param>
        public bool Equals(IScalar x, IScalar y)
        {
            if (x == null)
            {
                return false;
            }

            if (y == null)
            {
                return false;
            }

            if (x.Name == null && y.Name == null)
            {
                return true;
            }

            if (x.Name != null)
            {
                return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
        public int GetHashCode(IScalar obj)
        {
            return obj.Name.GetHashCode();
        }

        #endregion
    }
}