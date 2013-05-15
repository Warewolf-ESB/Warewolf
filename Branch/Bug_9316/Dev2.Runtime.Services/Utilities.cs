using System;
using System.Text;

namespace Dev2.Runtime
{
    public static class Utilities
    {
        #region GenerateString

        public static string GenerateString(this Random random, int length, string prefix = "", bool includeSpaces = false)
        {
            var modulo = length / 3;
            var builder = new StringBuilder(prefix);
            for(var k = 0; k < length; k++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                if(k % modulo != 0)
                {
                    ch = char.ToLower(ch);
                }
                builder.Append(ch);
            }
            return builder.ToString();
        }

        #endregion

    }
}