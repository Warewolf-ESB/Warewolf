using System;

namespace Dev2.Data.Storage.ProtocolBuffers
{
    [Serializable]
    public abstract class AProtocolBuffer
    {
        public abstract byte[] ToByteArray();

        public abstract void ToObject(byte[] bytes);

    }
}
