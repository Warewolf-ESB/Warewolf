using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.Settings
{
    public class BackupSettings : IBackupSettings
    {
        #region CTOR

        public BackupSettings()
        {
        }

        public BackupSettings(XElement xml)
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