using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.Settings
{
    public class SecuritySettings : SettingsBase
    {
        public new const string SettingName = "Security";
        
        #region CTOR

        public SecuritySettings(string webServerUri)
            : base(SettingName, "Security",webServerUri)
        {
        }

        public SecuritySettings(XElement xml, string webServerUri)
            : base(xml,webServerUri)
        {
        }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();

            return result;
        }

        #endregion
    }
}