using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Studio.Core.AppResources.ExtensionMethods {
    public static class EnumerableExtensionMethods
    {
        /// <summary>
        /// Returns the maximum value or null if sequence is empty.
        /// </summary>
        /// <param name="that">The sequence to retrieve the maximum value from.
        /// </param>
        /// <returns>The maximum value or null.</returns>
        public static T? MaxOrNullable<T>(this IEnumerable<T> that)
            where T : struct, IComparable {
            if (!that.Any()) {
                return null;
            }
            return that.Max();
        }

        /// <summary>
        /// Returns the minimum value or null if sequence is empty.
        /// </summary>
        /// <param name="that">The sequence to retrieve the minimum value from.
        /// </param>
        /// <returns>The minimum value or null.</returns>
        public static T? MinOrNullable<T>(this IEnumerable<T> that)
            where T : struct, IComparable {
            if (!that.Any()) {
                return null;
            }
            return that.Min();
        }
    }
}
