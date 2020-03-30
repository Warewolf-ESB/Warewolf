/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System;

namespace Warewolf.Configuration
{
    public class ServerSettingsData : IEquatable<ServerSettingsData>, IHasChanged
    {
        public ushort? WebServerPort { get; set; }
        public ushort? WebServerSslPort { get; set; }
        public string SslCertificateName { get; set; }
        public bool? CollectUsageStats { get; set; }
        public int? DaysToKeepTempFiles { get; set; }
        public string AuditFilePath { get; set; }
        public bool? EnableDetailedLogging { get; set; }
        public int? LogFlushInterval { get; set; }

        public bool Equals(ServerSettingsData other)
        {
            var equals = WebServerPort == other.WebServerPort;
            equals &= WebServerSslPort == other.WebServerSslPort;
            equals &= string.Equals(SslCertificateName, other.SslCertificateName, StringComparison.InvariantCultureIgnoreCase);
            equals &= CollectUsageStats == other.CollectUsageStats;
            equals &= DaysToKeepTempFiles == other.DaysToKeepTempFiles;
            equals &= string.Equals(AuditFilePath, other.AuditFilePath, StringComparison.InvariantCultureIgnoreCase);
            equals &= EnableDetailedLogging == other.EnableDetailedLogging;
            equals &= LogFlushInterval == other.LogFlushInterval;

            return equals;
        }

        public bool HasChanged { get; set; }
    }
}
