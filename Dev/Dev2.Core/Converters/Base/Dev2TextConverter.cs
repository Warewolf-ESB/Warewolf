using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Interfaces;

namespace Dev2.Converters {
    internal class Dev2TextConverter : IBaseConverter, ISpookyLoadable {

        public string ConvertToBase(byte[] payload) {
            return (System.Text.Encoding.UTF8.GetString(payload));
        }

        public byte[] NeutralizeToCommon(string payload) {
            UTF8Encoding encoder = new UTF8Encoding();
            return (encoder.GetBytes(payload));
        }

        public bool IsType(string payload) {
            bool result = (!Regex.IsMatch(payload, "^[01]+$")
                && !(System.Text.RegularExpressions.Regex.IsMatch(payload, @"\A\b[0-9a-fA-F]+\b\Z")
                     || System.Text.RegularExpressions.Regex.IsMatch(payload, @"\A\b(0[xX])?[0-9a-fA-F]+\b\Z")));

            return result;
        }

        public Enum HandlesType() {
            return enDev2BaseConvertType.Text;
        }
    }
}
