using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.Settings
{
    public class SecuritySettings : ISecuritySettings
    {
        #region CTOR

        public SecuritySettings()
        {
        }

        public SecuritySettings(XElement xml)
        {
        }

        #endregion


        #region ToXml

        public XElement ToXml()
        {
            return null;
        }

        #endregion
    }
}