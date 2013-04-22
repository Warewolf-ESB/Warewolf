using Dev2.Runtime.Security;
using System.Collections.Specialized;

namespace Dev2.Tests.Runtime.Services
{
    public class HostSecureConfigMock : HostSecureConfig
    {
        public NameValueCollection SaveConfigSettings { get; private set; }
        public int SaveConfigHitCount { get; private set; }
        public int ProtectConfigHitCount { get; private set; }

        public HostSecureConfigMock(NameValueCollection settings)
            : base(settings, true)
        {
        }

        protected override void SaveConfig(NameValueCollection secureSettings)
        {
            SaveConfigSettings = secureSettings;
            SaveConfigHitCount++;
        }

        protected override void ProtectConfig()
        {
            ProtectConfigHitCount++;
        }
    }
}
