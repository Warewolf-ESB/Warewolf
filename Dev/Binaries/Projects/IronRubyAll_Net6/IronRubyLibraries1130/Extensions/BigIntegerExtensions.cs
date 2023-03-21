using Microsoft.Scripting.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Scripting.Math.Extensions
{
    public static class BigIntegerExtensions
    {
        public static double ToFloat64(this BigInteger self)
        {
            if (TryToFloat64(self, out var result))
            {
                return result;
            }

            throw new OverflowException("Value was either too large or too small for a Double.");
        }

        public static bool TryToFloat64(this BigInteger self, out double result)
        {
            return StringUtils.TryParseDouble(self.ToString(10), NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out result);
        }

        public static string BigIntegerToString(this BigInteger self, uint[] d, int sign, int radix, bool lowerCase)
        {
            return MathUtils.BigIntegerToString(d, sign, radix, lowerCase);
        }

    }
}
