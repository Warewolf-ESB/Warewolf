#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
    public sealed class Configuration : PropertyChangedBase
    {
        static ConcurrentDictionary<Guid, TextExpressionCompilerResults> resultscache = new ConcurrentDictionary<Guid, TextExpressionCompilerResults>();

        #region Fields

        LoggingSettings _logging;
        SecuritySettings _security;
        BackupSettings _backup;

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

        public static ConcurrentDictionary<Guid, TextExpressionCompilerResults> Resultscache { get => resultscache; set => resultscache = value; }

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

        void SettingChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HasError" || e.PropertyName == "Error")
            {
                NotifyOfPropertyChange(() => HasError);
                return;
            }

            if (e.PropertyName == "HasChanges")
            {
                NotifyOfPropertyChange(() => HasChanges);
            }
        }

        #endregion

        #region Static Methods

        #endregion
    }
}
