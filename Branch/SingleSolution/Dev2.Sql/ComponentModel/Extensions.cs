
using System;

namespace Warewolf.ComponentModel
{
    public static class Extensions
    {
        #region ToStringSafe

        public static string ToStringSafe(this string s)
        {
            return string.IsNullOrEmpty(s) ? string.Empty : s;

        }

        public static string ToStringSafe(this object obj)
        {
            return obj == null || Convert.IsDBNull(obj) ? string.Empty : obj.ToString();
        }

        #endregion
    }
}
