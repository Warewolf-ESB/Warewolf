using System;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Versioning;

namespace Dev2.Runtime.ServiceModel.Data
{
    [Serializable]
    public class VersionInfo : IVersionInfo
    {
        public  VersionInfo(DateTime dateTimeStamp, string  reason, string user, string versionNumber, Guid resourceId ,Guid versionId)
        {
            DateTimeStamp = dateTimeStamp;
            Reason = reason;
            User = user;
            VersionNumber = versionNumber;
            ResourceId = resourceId;
            VersionId = versionId;

        }
        public VersionInfo()
        {

        }
        public VersionInfo(string xml, Guid resourceId)
        {
            if(string.IsNullOrEmpty(xml))
            {
                DateTimeStamp = DateTime.Now;
                Reason = "Save";
                User = "Unknown";
                VersionNumber = "1";
                ResourceId = resourceId;
                VersionId = Guid.NewGuid();
            }
            else
            {
                XElement versionXml = XElement.Parse(xml);
                DateTimeStamp = DateTime.Parse(versionXml.AttributeSafe("DateTimeStamp"));
                Reason = versionXml.AttributeSafe("Reason");
                User = versionXml.AttributeSafe("User");
                VersionNumber = versionXml.AttributeSafe("VersionNumber");
                ResourceId = Guid.Parse(versionXml.AttributeSafe("ResourceId"));
                VersionId = Guid.Parse(versionXml.AttributeSafe("VersionId"));
            }
        }

        #region Implementation of IVersionInfo

        public DateTime DateTimeStamp { get; set; }
        public string Reason { get; set; }
        public string User { get; set; }
        public string VersionNumber { get; set; }
        public Guid ResourceId { get; set; }
        public Guid VersionId { get; set; }

        public override string ToString()
        {
            return $"ResourceId:{ResourceId} Version:{VersionId} ";
        }

        #endregion
    }
}