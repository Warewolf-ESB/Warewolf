using System;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.Storage.ProtocolBuffers
{
    /// <summary>
    /// Used to track aliases when shaping ;)
    /// </summary>
    [Serializable]
    public class BinaryDataListAlias
    {
        public Guid MasterKeyID;

        public bool IsCOW;

        public string MasterKey;

        public string ChildKey;

        public string MasterNamespace;

        public string MasterColumn;

        public IBinaryDataListEntry MasterEntry;

        //public override byte[] ToByteArray()
        //{

        //    var keyIDBytes = MasterKeyID.ToByteArray();
        //    var keyIDBytesLen = 16; // 16 bytes ;)

        //    var mKeyBytes = Encoding.UTF8.GetBytes(MasterKey);
        //    var mKeyBytesLen = mKeyBytes.Length;

        //    var cKeyBytes = Encoding.UTF8.GetBytes(ChildKey);
        //    var cKeyBytesLen = cKeyBytes.Length;

        //    // create the buffer ;)
        //    byte[] result = new byte[(keyIDBytesLen+mKeyBytesLen+cKeyBytesLen+(sizeof(int)*2))];
        //    int offSet = 0;

        //    // copy the MasterKeyID value
        //    Buffer.BlockCopy(keyIDBytes, 0, result, offSet, keyIDBytesLen);

        //    // copy the size of the MasterKey ;)
        //    offSet += keyIDBytesLen;
        //    Buffer.BlockCopy(BitConverter.GetBytes(mKeyBytesLen),0,result,offSet,sizeof(int));

        //    // copy the is COW flag
        //    offSet += sizeof (int);
        //    Buffer.BlockCopy(BitConverter.GetBytes(IsCOW), 0, result, offSet, sizeof(bool));

        //    // copy the size of the ChildKey ;)
        //    offSet += sizeof(bool);
        //    Buffer.BlockCopy(BitConverter.GetBytes(cKeyBytesLen), 0, result, offSet, sizeof(int));

        //    // copy master key data ;)
        //    offSet += sizeof (int);
        //    Buffer.BlockCopy(mKeyBytes, 0, result, offSet, mKeyBytesLen);

        //    // copy child key data ;)
        //    offSet += sizeof(int);
        //    Buffer.BlockCopy(cKeyBytes, 0, result, offSet, cKeyBytesLen);


        //    return result;

        //}

        //public override void ToObject(byte[] bytes)
        //{

        //    byte[] intBuffer = new byte[sizeof(int)];
        //    int offSet = 0;

        //    // unpack the master ID guid
        //    byte[] guidBuffer = new byte[16];
        //    Buffer.BlockCopy(bytes, offSet, guidBuffer, 0, 16);
        //    MasterKeyID = new Guid(guidBuffer);

        //    // unpack Master Key Len
        //    offSet += 16;
        //    Buffer.BlockCopy(bytes, offSet, intBuffer, 0, sizeof(int));
        //    var mKeyLen = BitConverter.ToInt32(intBuffer, 0);

        //    // unpack the IsCOW flag
        //    offSet += sizeof(int);
        //    byte[] boolBytes = new byte[sizeof(bool)];
        //    Buffer.BlockCopy(bytes, offSet, boolBytes, 0, sizeof(bool));
        //    IsCOW = BitConverter.ToBoolean(boolBytes, 0);

        //    // unpack Child Key Len
        //    offSet += sizeof (bool);
        //    Buffer.BlockCopy(bytes, offSet, intBuffer, 0, sizeof(int));
        //    var cKeyLen = BitConverter.ToInt32(intBuffer, 0);

        //    // unpack the Master Key String
        //    offSet += sizeof (int);
        //    var mBuffer = new byte[mKeyLen];
        //    Buffer.BlockCopy(bytes, offSet, mBuffer, 0, mKeyLen);
        //    MasterKey = Encoding.UTF8.GetString(mBuffer);

        //    // unpack the Child Key String
        //    offSet += sizeof(int);
        //    var cBuffer = new byte[cKeyLen];
        //    Buffer.BlockCopy(bytes, offSet, cBuffer, 0, cKeyLen);
        //    ChildKey = Encoding.UTF8.GetString(cBuffer);
        //}
    }
}
