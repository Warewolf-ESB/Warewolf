using System;

namespace Dev2.Common
{
    public class ServiceNotAuthorizedException : Exception
    {
        public ServiceNotAuthorizedException(string msg) : base(msg)
        {

        }
    }
}
