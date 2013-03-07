using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.Settings
{
    public class SecuritySettings : SettingsBase
    {
        #region CTOR

        public SecuritySettings()
            : base("Security")
        {
        }

        public SecuritySettings(XElement xml)
            : base(xml)
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