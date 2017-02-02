/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.XamlIntegration;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Xml.Linq;
using Caliburn.Micro;

namespace Dev2.Runtime.Configuration.Settings
{
    // ------------------------------------------------------------------------------
    // - Add new SettingsBase derived class in this name space
    // - Then add new property for class here and initialize it in constructors
    // - Then add property to ToXml() 
    // ------------------------------------------------------------------------------
    public sealed class Configuration : PropertyChangedBase
    {
        public static ConcurrentDictionary<Guid, TextExpressionCompilerResults> Resultscache = new ConcurrentDictionary<Guid, TextExpressionCompilerResults>(); 


        #region Fields

        private LoggingSettings _logging;
        private SecuritySettings _security;
        private BackupSettings _backup;

        #endregion

        #region Constructors

        public Configuration(string webServerUri)
        {
            WebServerUri = webServerUri;
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

        public Version Version { get; set; }

        public string WebServerUri { get; set; }

        public bool HasChanges => Logging != null && Logging.HasChanges ||
                                  Security != null && Security.HasChanges ||
                                  Backup != null && Backup.HasChanges;

        public bool HasError => Logging != null && Logging.HasError ||
                                Security != null && Security.HasError ||
                                Backup != null && Backup.HasError;

        public LoggingSettings Logging
        {
            get { return _logging; }
            private set
            {
                _logging = value;
                NotifyOfPropertyChange(() => Logging);
            }
        }

        public SecuritySettings Security
        {
            get { return _security; }
            private set
            {
                _security = value;
                NotifyOfPropertyChange(() => Security);
            }
        }

        public BackupSettings Backup
        {
            get { return _backup; }
            private set
            {
                _backup = value;
                NotifyOfPropertyChange(() => Backup);
            }
        }

        #endregion

        #region Methods
        public XElement ToXml()
        {
            var result = new XElement("Settings",
                new XAttribute("WebServerUri", WebServerUri),
                new XAttribute("Version", Version.ToString()),
                Logging.ToXml(),
                Security.ToXml(),
                Backup.ToXml()
                );
            return result;
        }

        #endregion

        #region Private Methods

        public void Init(XElement xml)
        {
            if(xml == null)
            {
                Version = new Version(1, 0);
                Logging = new LoggingSettings(WebServerUri);
                Security = new SecuritySettings(WebServerUri);
                Backup = new BackupSettings(WebServerUri);
            }
            else
            {
                WebServerUri = xml.AttributeSafe("WebServerUri");
                Version = new Version(xml.AttributeSafe("Version"));
                Logging = new LoggingSettings(xml.Element(LoggingSettings.SettingName), WebServerUri);
                Security = new SecuritySettings(xml.Element(SecuritySettings.SettingName), WebServerUri);
                Backup = new BackupSettings(xml.Element(BackupSettings.SettingName), WebServerUri);
            }

            Logging.PropertyChanged += SettingChanged;
            Security.PropertyChanged += SettingChanged;
            Backup.PropertyChanged += SettingChanged;

            NotifyOfPropertyChange(() => HasChanges);
        }

        #endregion

        #region Event Handlers

        private void SettingChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "HasError" || e.PropertyName == "Error")
            {
                NotifyOfPropertyChange(() => HasError);
                return;
            }

            if(e.PropertyName == "HasChanges")
            {
                NotifyOfPropertyChange(() => HasChanges);
            }
        }

        #endregion

        #region Static Methods

        #endregion
    }
}
