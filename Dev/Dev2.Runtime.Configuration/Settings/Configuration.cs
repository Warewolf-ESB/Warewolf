using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.ViewModels;
using Dev2.Runtime.Configuration.Views;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.Settings
{
    // ------------------------------------------------------------------------------
    // - Add new SettingsBase derived class in this namespace
    // - Then add new property for class here and initialize it in constructors
    // - Then add property to ToXml() 
    // ------------------------------------------------------------------------------
    public sealed class Configuration : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Impl

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Class Members

        private bool _hasChanges;

        #endregion

        #region Fields

        public Version Version { get; set; }

        public bool HasChanges
        {
            get
            {
                return _hasChanges;
            }
            set
            {
                _hasChanges = value;
                OnPropertyChanged("HasChanges");
            }
        }

        #endregion

        #region Constructors

        public Configuration()
        {
            Init(null);
        }

        public Configuration(XElement xml)
        {
            if(xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            Init(xml);
        }

        #endregion

        #region Properties

        [SettingsObject(typeof(LoggingView), typeof(LoggingViewModel))]
        public LoggingSettings Logging { get; private set; }

        public SecuritySettings Security { get; private set; }

        public BackupSettings Backup { get; private set; }

        #endregion

        #region Methods

        public XElement ToXml()
        {
            var result = new XElement("Settings",
                new XAttribute("Version", Version.ToString()),
                Logging.ToXml(),
                Security.ToXml(),
                Backup.ToXml()
                );
            return result;
        }

        public void IncrementVersion()
        {
            Version = Version == null ? new Version(1, 0) : new Version(Version.Major, Version.Minor + 1);
        }

        #endregion

        #region Private Methods

        private void Init(XElement xml)
        {
            if (xml == null)
            {
                Version = new Version(1, 0);
                Logging = new LoggingSettings();
                Security = new SecuritySettings();
                Backup = new BackupSettings();
            }
            else
            {
                Version = new Version(xml.AttributeSafe("Version"));
                Logging = new LoggingSettings(xml.Element(LoggingSettings.SettingName));
                Security = new SecuritySettings(xml.Element(SecuritySettings.SettingName));
                Backup = new BackupSettings(xml.Element(BackupSettings.SettingName));                
            }

            Logging.PropertyChanged += SettingChanged;
            Security.PropertyChanged += SettingChanged;
            Backup.PropertyChanged += SettingChanged;
        }

        #endregion

        #region Event Handlers

        private void SettingChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (!HasChanges)
            {
                HasChanges = true;
            }
        }

        #endregion

        #region Static Methods

        public static UserControl EntryPoint(XElement configurationXML, Action<XElement> saveCallback, Action cancelCallback, Action settingChangedCallback)
        {
            MainView settingsView = new MainView();
            MainViewModel mainViewModel = new MainViewModel(configurationXML, saveCallback, cancelCallback, settingChangedCallback);
            settingsView.DataContext = mainViewModel;

            if (mainViewModel.SettingsObjects != null && mainViewModel.SettingsObjects.Count > 0)
            {
                mainViewModel.SelectedSettingsObjects = mainViewModel.SettingsObjects[0];
            }

            return settingsView;
        }

        #endregion
    }
}