using System;
using System.Collections.Generic;

namespace Dev2.Runtime.DynamicProxy
{
    internal class DiscoveryClientProtocol
    {
        public bool AllowAutoRedirect { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public Dictionary<string, object> Documents { get; internal set; }

        public static void DiscoverAny(string wsdlUri)
        {
            throw new NotImplementedException();
        }

        public static void ResolveAll()
        {
            throw new NotImplementedException();
        }
    }
}