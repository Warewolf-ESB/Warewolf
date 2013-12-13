using System;

namespace Dev2.Communication
{
    public class FutureReceipt 
    {
        public Guid RequestID { get; set; }

        public int PartID { get; set; }

        public string ToKey()
        {
            return RequestID +"-"+ PartID;
        }
    }
}
