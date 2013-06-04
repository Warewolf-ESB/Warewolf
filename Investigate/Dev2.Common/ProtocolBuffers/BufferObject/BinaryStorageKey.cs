using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Common.ProtocolBuffers.BufferObject
{
    public class BinaryStorageKey : AProtocolBuffer, IStringifyable
    {
        public Guid uid;
        public int nsHashCode;
        public int pos;

        public override byte[] ToByteArray()
        {
            byte[] result = new byte[24];

            // uid
            Buffer.BlockCopy(uid.ToByteArray(), 0, result, 0, 16);
            // nsHashCode
            Buffer.BlockCopy(BitConverter.GetBytes(nsHashCode), 0, result, 16, 4);
            // pos
            Buffer.BlockCopy(BitConverter.GetBytes(pos), 0, result, 20, 4);

            return result;
        }

        public override void ToObject(byte[] bytes)
        {
            byte[] uidBytes = new byte[16];
            byte[] intBytes = new byte[4];

            // uid
            Buffer.BlockCopy(bytes, 0, uidBytes, 0, 16);
            uid = new Guid(uidBytes);
            // nsHashCode
            Buffer.BlockCopy(bytes, 16, intBytes, 0, 4);
            nsHashCode = BitConverter.ToInt32(intBytes, 0);
            // pos 
            Buffer.BlockCopy(bytes, 20, intBytes, 0, 4);
            nsHashCode = BitConverter.ToInt32(intBytes, 0);
        }

        public void FromString(string str)
        {
            string[] parts = str.Split('|');

            if (parts.Length == 3)
            {
                Guid.TryParse(parts[0], out uid);

                Int32.TryParse(parts[1], out nsHashCode);

                Int32.TryParse(parts[2], out pos);
            }
        }
    }
}
