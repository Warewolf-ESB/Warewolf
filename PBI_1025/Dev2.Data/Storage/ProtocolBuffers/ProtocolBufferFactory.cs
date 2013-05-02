using Dev2.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Data.Storage.ProtocolBuffers
{
    /// <summary>
    /// Caches all the protocol buffer implementations
    /// </summary>
    public class ProtocolBufferFactory : SpookyAction<IDev2ProtocolBuffer, string>
    {

        /// <summary>
        /// Create a base converter 
        /// </summary>
        /// <param name="typeOf"></param>
        /// <returns></returns>
        public IDev2ProtocolBuffer CreateConverter(string typeOf)
        {
            return FindMatch(typeOf);
        }

    }
}
