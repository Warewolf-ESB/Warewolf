using System;
using System.Collections.Generic;


namespace Gma.DataStructures.StringSearch
{
    internal class StringComparer<T> : IEqualityComparer<T>
    {


        #region Implementation of IEqualityComparer<in string>

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(T x, T y)
        {
            var equal = string.Equals(x.ToString(), y.ToString(), StringComparison.OrdinalIgnoreCase);
            return equal;
        }


        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }
}