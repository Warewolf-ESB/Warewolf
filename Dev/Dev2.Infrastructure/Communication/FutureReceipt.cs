using System;

namespace Dev2.Communication
{
    public class FutureReceipt
    {
        public Guid RequestID { get; set; }

        public int PartID { get; set; }

        public string User { get; set; }

        public string ToKey()
        {
            if(PartID < 0)
            {
                throw new Exception("Invalid PartID");
            }

            if(string.IsNullOrEmpty(User))
            {
                throw new Exception("Invalid User");
            }

            if(RequestID == Guid.Empty)
            {
                throw new Exception("Invalid RequestID");
            }

            return RequestID + "-" + PartID + "-" + User + "!";
        }
    }
}
