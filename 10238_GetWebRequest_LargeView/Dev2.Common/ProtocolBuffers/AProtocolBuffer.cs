namespace Dev2.Common.ProtocolBuffers
{
    public abstract class AProtocolBuffer : IDev2ProtocolBuffer
    {

        public abstract byte[] ToByteArray();

        public abstract void ToObject(byte[] bytes);

        public string HandlesType()
        {
            return (this.GetType().ToString());
        }
    }
}
