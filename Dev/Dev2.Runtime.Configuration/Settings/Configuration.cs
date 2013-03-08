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
        public LoggingSettings Logging { get; private set; }
        public SecuritySettings Security { get; private set; }
        public BackupSettings Backup { get; private set; }

        public Configuration()
        {
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
            Logging = new LoggingSettings(xml.Element("Logging"));
            Security = new SecuritySettings(xml.Element("Security"));
            Backup = new BackupSettings(xml.Element("Backup"));
        }

        public XElement ToXml()
        {
            var result = new XElement("Settings",
                Logging.ToXml(),
                Security.ToXml(),
                Backup.ToXml()
                );
            return result;
        }
    }
}