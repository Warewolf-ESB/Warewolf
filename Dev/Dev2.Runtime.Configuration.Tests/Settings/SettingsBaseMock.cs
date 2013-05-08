using System.Xml.Linq;
using Dev2.Runtime.Configuration.Settings;

namespace Dev2.Runtime.Configuration.Tests.Settings
{
    public class SettingsBaseMock : SettingsBase
    {
        public SettingsBaseMock(string settingName, string displayName, string webServerUri)
            : base(settingName, displayName,webServerUri)
        {
        }

        public SettingsBaseMock(XElement xml,string webServerUri)
            : base(xml,webServerUri)
        {
        }
    }
}
