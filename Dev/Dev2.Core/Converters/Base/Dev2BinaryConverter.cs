using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Interfaces;

namespace Dev2.Converters
{
    internal class Dev2BinaryConverter : IBaseConverter,ISpookyLoadable
    {

        public string ConvertToBase(byte[] payload)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < payload.Length; i++)
            {
                string tmp = Convert.ToString(payload[i], 2);
                // zero pad the value
                for (int q = tmp.Length; q < 8; q++)
                {
                    result.Append("0");
                }
                result.Append(tmp);
            }

            return result.ToString();
        }

        public byte[] NeutralizeToCommon(string payload)
        {

            byte[] result = new byte[(payload.Length / 8)];
            char[] chars = payload.ToCharArray();

            int pos = 0;
            for (int i = 0; i < payload.Length; i += 8)
            {
                result[pos] = Convert.ToByte((payload.Substring(i, 8)), 2);
                pos++;
            }

            return result;
        }

        public bool IsType(string payload)
        {
            return Regex.IsMatch(payload, "^[01]+$");
        }

        public Enum HandlesType()
        {
            return enDev2BaseConvertType.Binary;
        }

    }
}
