using System;
using System.Text;

namespace Dev2.Converters
{
    internal class Dev2Base64Converter : IBaseConverter
    {
        public bool IsType(string payload)
        {
            bool result = false;
            try
            {
                // ReSharper disable ReturnValueOfPureMethodIsNotUsed
                Convert.FromBase64String(payload);
                // ReSharper restore ReturnValueOfPureMethodIsNotUsed
                result = true;
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // if error is thrown we know it is not a valid base64 string
            }

            return result;

        }

        public string ConvertToBase(byte[] payload)
        {
            return Convert.ToBase64String(payload);
        }

        public byte[] NeutralizeToCommon(string payload)
        {

            byte[] decoded = Convert.FromBase64String(payload);
            string tmp = Encoding.UTF8.GetString(decoded);

            UTF8Encoding encoder = new UTF8Encoding();
            return (encoder.GetBytes(tmp));

        }

        public Enum HandlesType()
        {
            return enDev2BaseConvertType.Base64;
        }

    }
}
