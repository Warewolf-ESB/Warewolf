
namespace Dev2.Warewolf.Security.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns true if this <paramref name="value"/> is Base64 encoded. If <paramref name="value"/> is null or empty then false is returned
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsBase64Encoded(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            return System.Text.RegularExpressions.Regex.IsMatch(value, @"^([A-Za-z0-9+/]{4})*([A-Za-z0-9+/]{4}|[A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{2}==)$");
        }
    }
}
