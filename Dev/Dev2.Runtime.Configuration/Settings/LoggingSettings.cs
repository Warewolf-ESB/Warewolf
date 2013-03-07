using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.Settings
{
    public class LoggingSettings : ILoggingSettings
    {
        #region CTOR

        public LoggingSettings()
        {
        }

        public LoggingSettings(XElement xml)
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