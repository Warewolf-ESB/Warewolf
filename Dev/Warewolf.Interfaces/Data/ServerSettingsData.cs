using System;

namespace Warewolf.Configuration
{
    public class ServerSettingsData : IEquatable<ServerSettingsData>
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
    }
}
