using System;
using System.ComponentModel;
using System.Data;
using System.Xml.Linq;
using Caliburn.Micro;

namespace Dev2.Runtime.Configuration.Settings
{
    public abstract class SettingsBase : PropertyChangedBase
    {

        #region Fields

        private string _settingName;
        private string _displayName;
        string _webServerUri;

        #endregion

        #region Properties

        public bool IsInitializing { get; set; }

        public string SettingName 
        {
            get
            {
                return _settingName;
            }
            private set
            {
                _settingName = value;
                NotifyOfPropertyChange(() => SettingName);
            }
        }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            private set
            {
                _displayName = value;
                NotifyOfPropertyChange(() => DisplayName);
            }
        }
        
        public string WebServerUri
        {
            get
            {
                return _webServerUri;
            }
            private set
            {
                _webServerUri = value;
                NotifyOfPropertyChange(() => WebServerUri);
            }
        }

        #endregion

        #region CTOR

        protected SettingsBase(string settingName, string displayName,string webServerUri)
        {
            if(string.IsNullOrEmpty(settingName))
            {
                throw new ArgumentNullException("settingName");
            }
            if(string.IsNullOrEmpty(displayName))
            {
                throw new ArgumentNullException("displayName");
            }
            if (string.IsNullOrEmpty(webServerUri))
            {
                throw new ArgumentNullException("webServerUri");
            }
            SettingName = settingName;
            DisplayName = displayName;
            WebServerUri = webServerUri;
        }

        protected SettingsBase(XElement xml,string webServerUri)
        {
            if(xml == null)
            {
                throw new ArgumentNullException("xml");
            }
            var displayName = xml.AttributeSafe("DisplayName");
            if(string.IsNullOrEmpty(displayName))
            {
                throw new NoNullAllowedException("displayName");
            }
            if (string.IsNullOrEmpty(webServerUri))
            {
                throw new NoNullAllowedException("webServerUri");
            }
            SettingName = xml.Name.LocalName;
            DisplayName = displayName;
            WebServerUri = webServerUri;
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