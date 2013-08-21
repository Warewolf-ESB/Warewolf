using Dev2.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Data.Storage
{
    /// <summary>
    /// Create a byte streamable object for persist storage, based upon Google's ProtoBuf
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDev2ProtocolBuffer : ISpookyLoadable<string>
    {

        /// <summary>
        /// Push an object into a storage byte array ;)
        /// </summary>
        /// <returns></returns>
        byte[] ToByteArray();

        /// <summary>
        /// Convert a storable byte array into an object ;)
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        void ToObject(byte[] bytes); 
    }
}
