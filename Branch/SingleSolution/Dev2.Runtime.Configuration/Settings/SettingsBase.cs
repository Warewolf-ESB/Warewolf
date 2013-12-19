using System;
using System.Data;
using System.Xml.Linq;
using Caliburn.Micro;

namespace Dev2.Runtime.Configuration.Settings
{
    public abstract class SettingsBase : PropertyChangedBase
    {

        #region Fields

        private string _error = string.Empty;
        private bool _hasChanges;
        private bool _isInitializing;

        #endregion

        #region Properties

        public bool HasChanges
        {
            get
            {
                return _hasChanges;
            }
            protected set
            {
                if (_hasChanges == value)
                {
                    return;
                }

                _hasChanges = value;
                NotifyOfPropertyChange(() => HasChanges);
            }
        }

        public bool IsInitializing
        {
            get
            {
                return _isInitializing;
            }
            set
            {
                if (_isInitializing == value)
                {
                    return;
                }

                _isInitializing = value;
                NotifyOfPropertyChange(() => IsInitializing);
            }
        }

        public string Error
        {
            get
            {
                return _error;
            }
            set
            {
                if (_error == value)
                {
                    return;
                }

                _error = value;
                NotifyOfPropertyChange(() => Error);
                NotifyOfPropertyChange(() => HasError);
            }
        }

        public bool HasError
        {
            get { return !string.IsNullOrWhiteSpace(Error); }
        }
        
        public string SettingName { get; private set; }

        public string DisplayName { get; private set; }

        public string WebServerUri { get; private set; }

        public override void NotifyOfPropertyChange(string propertyName)
        {
            if (IsInitializing)
            {
                return;
            }

            if (propertyName != "IsInitializing" && propertyName != "HasChanges")
            {
                HasChanges = true;
            }

            base.NotifyOfPropertyChange(propertyName);
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