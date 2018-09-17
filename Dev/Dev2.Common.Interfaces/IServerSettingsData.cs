namespace Dev2.Common.Interfaces
{
    public interface IServerSettingsData
    {
        ushort WebServerPort { get; set; }
        ushort WebServerSslPort { get; set; }
        string SslCertificateName { get; set; }
        bool CollectUsageStats { get; set; }
        int DaysToKeepTempFiles { get; set; }
        string AuditsFilePath { get; set; }
    }
}
