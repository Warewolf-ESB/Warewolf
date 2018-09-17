using System;

namespace Dev2.Common.Interfaces.Core
{
    public class ServerSettingsData : IServerSettingsData, IEquatable<ServerSettingsData>
    {
        public ushort WebServerPort { get; set; }
        public ushort WebServerSslPort { get; set; }
        public string SslCertificateName { get; set; }
        public bool CollectUsageStats { get; set; }
        public int DaysToKeepTempFiles { get; set; }
        public string AuditsFilePath { get; set; }

        public bool Equals(ServerSettingsData other)
        {
            var equals = WebServerPort == other.WebServerPort;
            equals &= WebServerSslPort == other.WebServerSslPort;
            equals &= string.Equals(SslCertificateName, other.SslCertificateName, StringComparison.InvariantCultureIgnoreCase);
            equals &= DaysToKeepTempFiles == other.DaysToKeepTempFiles;
            equals &= string.Equals(AuditsFilePath, other.AuditsFilePath, StringComparison.InvariantCultureIgnoreCase);

            return equals;
        }

        public bool Equals(IServerSettingsData other)
        {
            var equals = WebServerPort == other.WebServerPort;
            equals &= WebServerSslPort == other.WebServerSslPort;
            equals &= string.Equals(SslCertificateName, other.SslCertificateName, StringComparison.InvariantCultureIgnoreCase);
            equals &= DaysToKeepTempFiles == other.DaysToKeepTempFiles;
            equals &= string.Equals(AuditsFilePath, other.AuditsFilePath, StringComparison.InvariantCultureIgnoreCase);

            return equals;
        }
    }
}
