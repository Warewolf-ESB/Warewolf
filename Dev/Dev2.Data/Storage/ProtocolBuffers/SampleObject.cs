using System;
using System.Text;

namespace Dev2.Data.Storage.ProtocolBuffers
{
    public class SampleObject : AProtocolBuffer
    {

        public int TheInt { get; set; }

        public string TheString { get; set; }

        public override byte[] ToByteArray()
        {
            // account for the 4 bytes of int and how ever long string is?!
            int strLen = TheString.Length;
            int len = 4 + strLen;

            byte[] result = new byte[len];

            Buffer.BlockCopy(BitConverter.GetBytes(TheInt), 0, result, 0, 4);

            Buffer.BlockCopy(Encoding.UTF8.GetBytes(TheString), 0, result, 4, strLen);

            return result;
        }

        public override void ToObject(byte[] bytes)
        {

            byte[] intBuffer = new byte[4];

            Buffer.BlockCopy(bytes, 0, intBuffer, 0, 4);

            TheInt = BitConverter.ToInt32(intBuffer, 0);

            int strLen = (bytes.Length - 4);

            TheString = Encoding.UTF8.GetString(bytes, 4, strLen);

        }

    }
}
