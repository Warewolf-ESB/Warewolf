using System;

namespace Dev2.Communication
{
    /// <summary>
    /// Used to fetch execution request payloads
    /// NEVER MAKE THE PROPERTIES GETTER PRIVATE AS PER SONAR
    /// THIS WILL CAUSE SIGNALR'S SERIALIZATION TO FREAK OUT AND PASS EMPTY REQUESTID VALUES THROUGH
    /// </summary>
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
