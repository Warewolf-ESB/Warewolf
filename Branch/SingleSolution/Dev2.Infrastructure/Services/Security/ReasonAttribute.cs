using System;

namespace Dev2.Services.Security
{
    public class ReasonAttribute : Attribute
    {
        public ReasonAttribute(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; private set; }
    }
}