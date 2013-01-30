using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DynamicServices.Test {
    public static class TestHelper {

        public static byte[] ConvertStringToByteArray(string payload) {
            char[] charArray = payload.ToCharArray();

            byte[] byteArray = new byte[charArray.Length];



            for (int i = 0; i < charArray.Length; i++) {

                byteArray[i] = Convert.ToByte(charArray[i]);

            }

            return byteArray;
        }

        public static string ConvertByteArrayToString(byte[] payload) {
            string result = ASCIIEncoding.ASCII.GetString(payload);

            return result;
        }
    }
}
