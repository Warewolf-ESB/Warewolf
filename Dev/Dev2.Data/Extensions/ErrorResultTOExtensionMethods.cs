using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Dev2.DataList.Contract.Extensions
{
    public static class ErrorResultTOExtensionMethods
    {
        private static readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

        public static byte[] ToByteArray(this ErrorResultTO errors)
        {
            byte[] result;
            using(MemoryStream ms = new MemoryStream())
            {
                _binaryFormatter.Serialize(ms, errors);
                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
            }

            return result;
        }

        public static ErrorResultTO FromByteArray(byte[] errorsData)
        {
            ErrorResultTO result;

            using(MemoryStream ms = new MemoryStream(errorsData))
            {
                ms.Seek(0, 0);
                result = (ErrorResultTO)_binaryFormatter.Deserialize(ms);
                ms.Close();
                ms.Dispose();
            }

            return result;
        }
    }
}
