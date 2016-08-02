using System;

namespace Dev2.Common.ExtMethods
{
    public static class GuidExtensions
    {
        /// <summary>
        /// System.ArgumentNullException: input is null.
        /// System.FormatException: input is not in a recognized format.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static Guid ToGuid(this string guid) => Guid.Parse(guid);
    }
}
