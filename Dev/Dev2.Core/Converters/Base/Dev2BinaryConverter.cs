using System;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common.Interfaces.Core.Convertors.Base;
using Dev2.DynamicServices;

namespace Dev2.Converters
{
    internal class Dev2BinaryConverter : IBaseConverter
    {

        public string ConvertToBase(byte[] payload)
        {
            StringBuilder result = new StringBuilder();

            // ReSharper disable ForCanBeConvertedToForeach
            for(int i = 0; i < payload.Length; i++)
            // ReSharper restore ForCanBeConvertedToForeach
            {
                string tmp = Convert.ToString(payload[i], 2);
                // zero pad the value
                for(int q = tmp.Length; q < 8; q++)
                {
                    result.Append("0");
                }
                result.Append(tmp);
            }

            return result.ToString();
        }

        public byte[] NeutralizeToCommon(string payload)
        {
            if(payload.Length >= 8)
            {
                var result = new byte[(payload.Length / 8)];

                int pos = 0;
                for(int i = 0; i < payload.Length; i += 8)
                {
                    result[pos] = Convert.ToByte((payload.Substring(i, 8)), 2);
                    pos++;
                }

                return result;
            }

            throw new ArgumentException(Resources.Dev2BinaryConverterPayloadToSmallException);
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
