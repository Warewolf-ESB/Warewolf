using System;

namespace Dev2.Runtime.Exceptions
{
    public class ServiceNotAuthorizedException:Exception
    {
        public ServiceNotAuthorizedException(string msg):base(msg)
        {
            
        }
    }
}
