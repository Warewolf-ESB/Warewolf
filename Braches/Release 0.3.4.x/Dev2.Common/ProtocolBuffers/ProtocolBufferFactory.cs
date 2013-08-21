namespace Dev2.Common.ProtocolBuffers
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
