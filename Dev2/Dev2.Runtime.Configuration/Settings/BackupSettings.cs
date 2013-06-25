using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.Settings
{
    public class BackupSettings : SettingsBase
    {
        public new const string SettingName = "Backup";

        #region CTOR

        public BackupSettings(string webServerUri)
            : base(SettingName, "Backup",webServerUri)
        {
        }

        public BackupSettings(XElement xml,string webServerUri)
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