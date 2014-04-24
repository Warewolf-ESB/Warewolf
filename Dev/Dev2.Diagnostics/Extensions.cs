using System;

namespace Dev2.Diagnostics
{
    public static class Extensions
    {
        public static bool ContainsSafe(this string s, string filter)
        {
            if(string.IsNullOrEmpty(filter))
            {
                return true;
            }
            if(!string.IsNullOrEmpty(s))
            {
                return s.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1;
            }
            return false;
        }

    }
}
