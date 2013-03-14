using System;
using System.ComponentModel;
using System.Data;
using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.Settings
{
    public abstract class SettingsBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Impl

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Fields

        private string _settingName;
        private string _displayName;

        #endregion

        #region Properties

        public string SettingName 
        {
            get
            {
                return _settingName;
            }
            private set
            {
                _settingName = value;
                OnPropertyChanged("SettingName");
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
                OnPropertyChanged("DisplayName");
            }
        }

        #endregion

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