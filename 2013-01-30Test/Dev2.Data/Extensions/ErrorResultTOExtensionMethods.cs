using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Dev2.DataList.Contract.Extensions
{
    public static class ErrorResultTOExtensionMethods
    {
        private static BinaryFormatter _binaryFormatter = new BinaryFormatter();

        public static byte[] ToByteArray(this ErrorResultTO errors)
        {
            MemoryStream ms = new MemoryStream();
            _binaryFormatter.Serialize(ms, errors);
            byte[] result = ms.ToArray();
            ms.Close();
            ms.Dispose();

            return result;
        }

        public static ErrorResultTO FromByteArray(byte[] errorsData)
        {
            MemoryStream ms = new MemoryStream(errorsData);
            ms.Seek(0, 0);
            ErrorResultTO result = (ErrorResultTO)_binaryFormatter.Deserialize(ms);
            ms.Close();
            ms.Dispose();

            return result;
        }
    }
}
