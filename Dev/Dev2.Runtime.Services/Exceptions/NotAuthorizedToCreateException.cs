using System;

namespace Dev2.Runtime.Exceptions
{
    public class NotAuthorizedToCreateException:Exception
    {
        public NotAuthorizedToCreateException(string msg):base(msg)
        {
            
        }
    }
}
