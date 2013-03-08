using System;
using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.Settings
{
    // ------------------------------------------------------------------------------
    // - Add new SettingsBase derived class in this namespace
    // - Then add new property for class here and initialize it in constructors
    // - Then add property to ToXml() 
    // ------------------------------------------------------------------------------
    public sealed class Configuration
    {
        public Version Version { get; set; }

        public LoggingSettings Logging { get; private set; }
        public SecuritySettings Security { get; private set; }
        public BackupSettings Backup { get; private set; }

        public Configuration()
        {
            Version = new Version(1, 0);
            Logging = new LoggingSettings();
            Security = new SecuritySettings();
            Backup = new BackupSettings();
        }

        public Configuration(XElement xml)
        {
            if(xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            Version = new Version(xml.AttributeSafe("Version"));

            Logging = new LoggingSettings(xml.Element(LoggingSettings.SettingName));
            Security = new SecuritySettings(xml.Element(SecuritySettings.SettingName));
            Backup = new BackupSettings(xml.Element(BackupSettings.SettingName));
        }

        public XElement ToXml()
        {
            var result = new XElement("Settings",
                new XAttribute("Version", Version.ToString(2)),
                Logging.ToXml(),
                Security.ToXml(),
                Backup.ToXml()
                );
            return result;
        }
    }
}