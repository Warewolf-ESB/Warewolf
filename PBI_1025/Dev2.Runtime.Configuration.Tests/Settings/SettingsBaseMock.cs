using System.Xml.Linq;
using Dev2.Runtime.Configuration.Settings;

namespace Dev2.Runtime.Configuration.Tests.Settings
{
    public class SettingsBaseMock : SettingsBase
    {
        public SettingsBaseMock(string settingName, string displayName)
            : base(settingName, displayName)
        {
        }

        public SettingsBaseMock(XElement xml)
            : base(xml)
        {
        }
    }
}
