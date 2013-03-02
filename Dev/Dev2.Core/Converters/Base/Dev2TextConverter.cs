using System;
using System.Text;

namespace Dev2.Converters {
    internal class Dev2TextConverter : IBaseConverter {

        public string ConvertToBase(byte[] payload) {
            return (System.Text.Encoding.UTF8.GetString(payload));
        }

        public byte[] NeutralizeToCommon(string payload) {
            UTF8Encoding encoder = new UTF8Encoding();
            return (encoder.GetBytes(payload));
        }

        public bool IsType(string payload) {
            //bool result = (!Regex.IsMatch(payload, "^[01]+$")
            //    && !(System.Text.RegularExpressions.Regex.IsMatch(payload, @"\A\b[0-9a-fA-F]+\b\Z")
            //         || System.Text.RegularExpressions.Regex.IsMatch(payload, @"\A\b(0[xX])?[0-9a-fA-F]+\b\Z")));

            //2013.02.13: Ashley Lewis - Bug 8725, Task 8836
            return true;
        }

        public Enum HandlesType() {
            return enDev2BaseConvertType.Text;
        }
    }
}
