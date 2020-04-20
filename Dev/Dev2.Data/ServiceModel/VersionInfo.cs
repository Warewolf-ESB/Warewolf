/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Xml.Linq;
using Dev2.Common.Common;
using Warewolf.Data;

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
                var versionXml = XElement.Parse(xml);
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

        public override string ToString() => $"ResourceId:{ResourceId} Version:{VersionId} ";

        #endregion
    }
}