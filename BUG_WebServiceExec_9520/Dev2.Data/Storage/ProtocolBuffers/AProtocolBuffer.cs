using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Data.Storage.ProtocolBuffers
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
