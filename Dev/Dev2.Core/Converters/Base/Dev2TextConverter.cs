using System;
using System.Text;
using Dev2.Common.Interfaces.Core.Convertors.Base;

namespace Dev2.Converters {
    internal class Dev2TextConverter : IBaseConverter {

        public string ConvertToBase(byte[] payload) {
            return (Encoding.UTF8.GetString(payload));
        }

        public byte[] NeutralizeToCommon(string payload) {
            UTF8Encoding encoder = new UTF8Encoding();
            return (encoder.GetBytes(payload));
        }

        public bool IsType(string payload) {
          
            //2013.02.13: Ashley Lewis - Bug 8725, Task 8836
            return true;
        }

        public Enum HandlesType() {
            return enDev2BaseConvertType.Text;
        }
    }
}
