using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.Settings
{
    public class BackupSettings : SettingsBase
    {
        #region CTOR

        public BackupSettings() : base("Backup")
        {
        }

        public BackupSettings(XElement xml)
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