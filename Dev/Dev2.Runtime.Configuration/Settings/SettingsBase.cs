using System;
using System.Data;
using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.Settings
{
    public abstract class SettingsBase
    {
        public string SettingName { get; private set; }

        public string DisplayName { get; private set; }

        #region CTOR

        protected SettingsBase(string settingName, string displayName)
        {
            if(string.IsNullOrEmpty(settingName))
            {
                throw new ArgumentNullException("settingName");
            }
            if(string.IsNullOrEmpty(displayName))
            {
                throw new ArgumentNullException("displayName");
            }
            SettingName = settingName;
            DisplayName = displayName;
        }

        protected SettingsBase(XElement xml)
        {
            if(xml == null)
            {
                throw new ArgumentNullException("xml");
            }
            SettingName = xml.Name.LocalName;

            var displayName = xml.AttributeSafe("DisplayName");
            if(string.IsNullOrEmpty(displayName))
            {
                throw new NoNullAllowedException("displayName");
            }
            DisplayName = displayName;
        }

        #endregion

        #region ToXml

        public virtual XElement ToXml()
        {
            var result = new XElement(SettingName,
                new XAttribute("DisplayName", DisplayName)
                );
            return result;
        }

        #endregion

    }
}