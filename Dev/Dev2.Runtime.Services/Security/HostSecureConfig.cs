#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Services.Security;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.Security
{
    public class HostSecureConfig : ISecureConfig
    {
        const string SectionName = "secureSettings";
        private const string FileName = "Warewolf Server.exe.secureconfig";

        public HostSecureConfig()
        {
            try
            {
                EnsureSecureConfigFileExists();
                var settings = (NameValueCollection) ConfigurationManager.GetSection(SectionName);
                Initialize(settings, true);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
            }
        }

        public HostSecureConfig(NameValueCollection settings)
            : this(settings, true)
        {
        }

        public HostSecureConfig(NameValueCollection settings, bool shouldProtectConfig)
        {
            Initialize(settings, shouldProtectConfig);
        }

        public Guid ServerID { get; private set; }
        public string ConfigKey { get; private set; }
        public string ConfigSitename { get; private set; }
        public string CustomerId { get; private set; }
        public RSACryptoServiceProvider ServerKey { get; private set; }

        public RSACryptoServiceProvider SystemKey { get; private set; }

        protected void Initialize(NameValueCollection settings, bool shouldProtectConfig)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            SystemKey = CreateKey(settings["SystemKey"]);

            if (Guid.TryParse(settings["ServerID"], out Guid serverID) && serverID != Guid.Empty)
            {
                ServerID = serverID;
                ServerKey = CreateKey(settings["ServerKey"]);
                ManageLicenseConfig(settings, shouldProtectConfig);
            }
            else
            {
                // BUG 8395: settings is a ReadOnlyNameValueCollection so create a new collection instead 
                var newSettings = new NameValueCollection();
                ServerID = Guid.NewGuid();
                ServerKey = new RSACryptoServiceProvider();

                ConfigKey = HostSecurityProvider.InternalConfigKey;
                ConfigSitename = HostSecurityProvider.InternalConfigSitename;
                CustomerId = "";

                newSettings["ConfigKey"] = ConfigKey;
                newSettings["ConfigSitename"] = ConfigSitename;
                newSettings["CustomerId"] = CustomerId;

                ConfigSitename = DecryptKey(ConfigSitename);
                ConfigKey = DecryptKey(ConfigKey);
                CustomerId = DecryptKey(CustomerId);

                newSettings["ServerID"] = ServerID.ToString();
                newSettings["ServerKey"] = Convert.ToBase64String(ServerKey.ExportCspBlob(true));
                newSettings["SystemKey"] = settings["SystemKey"];

                SaveConfig(newSettings);

                if (shouldProtectConfig)
                {
                    ProtectConfig();
                }
            }
        }

        private void ManageLicenseConfig(NameValueCollection settings, bool shouldProtectConfig)
        {
            var updateSettingsFile = false;
            //TODO: the reason we create a new NameValueCollection is because the one coming in is readonly. The only
            //way to add the new keys is to do it this way. CustomerId might be removed from here.
            var newSettings = new NameValueCollection();

            if (settings["ConfigKey"] == null)
            {
                newSettings.Add("ConfigKey", HostSecurityProvider.InternalConfigKey);
                updateSettingsFile = true;
            }
            else
            {
                newSettings["ConfigKey"] = settings["ConfigKey"];
            }
            if (settings["ConfigSitename"] == null)
            {
                newSettings.Add("ConfigSitename", HostSecurityProvider.InternalConfigSitename);
                updateSettingsFile = true;
            }
            else
            {
                newSettings["ConfigSitename"] = settings["ConfigSitename"];
            }

            if (settings["CustomerId"] == null)
            {
                newSettings.Add("CustomerId", "");
                updateSettingsFile = true;
            }
            else
            {
                newSettings["CustomerId"] = settings["CustomerId"];
            }
            ConfigKey = newSettings["ConfigKey"];
            ConfigSitename = newSettings["ConfigSitename"];
            CustomerId = newSettings["CustomerId"];

            ConfigSitename = DecryptKey(ConfigSitename);
            ConfigKey = DecryptKey(ConfigKey);
            CustomerId = DecryptKey(CustomerId);

            if (updateSettingsFile)
            {
                newSettings["ServerID"] = ServerID.ToString();
                newSettings["ServerKey"] = Convert.ToBase64String(ServerKey.ExportCspBlob(true));
                newSettings["SystemKey"] = settings["SystemKey"];
                SaveConfig(newSettings);
                if (shouldProtectConfig)
                {
                    ProtectConfig();
                }
            }
        }

        void EnsureSecureConfigFileExists()
        {
            ConfigurationManager.RefreshSection(SectionName);
            if (!File.Exists(FileName))
            {
                Dev2Logger.Info(string.Format(ErrorResource.FileNotFound, FileName), GlobalConstants.WarewolfInfo);
                var newSettings = new NameValueCollection
                {

                    ["ServerID"] = "",
                    ["ServerKey"] = "",
                    ["SystemKey"] = HostSecurityProvider.InternalPublicKey,
                    ["CustomerId"] = "",
                    ["ConfigKey"] = HostSecurityProvider.InternalConfigKey,
                    ["ConfigSitename"] = HostSecurityProvider.InternalConfigSitename
                };
                SaveConfig(newSettings);
            }
        }

        protected virtual void SaveConfig(NameValueCollection secureSettings)
        {
            var config = new XElement(SectionName);
            foreach (string key in secureSettings.Keys)
            {
                config.Add(new XElement("add",
                    new XAttribute("key", key),
                    new XAttribute("value", secureSettings[key])
                ));
            }

            var configDoc = new XDocument(new XDeclaration("1.0", "utf-8", ""), config);
            configDoc.Save(FileName, SaveOptions.None);
        }

        protected virtual void ProtectConfig()
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var section = config.GetSection(SectionName);
                if (section != null && !section.SectionInformation.IsProtected && !section.ElementInformation.IsLocked)
                {
                    section.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");
                    section.SectionInformation.ForceSave = true;
                    config.Save(ConfigurationSaveMode.Full);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public static string DecryptKey(string base64String)
        {
            return SecurityEncryption.Decrypt(base64String);
        }

        public static RSACryptoServiceProvider CreateKey(string base64String)
        {
            var keyBlob = Convert.FromBase64String(base64String);
            var key = new RSACryptoServiceProvider();
            key.ImportCspBlob(keyBlob);
            return key;
        }

        public static NameValueCollection CreateSettings(string serverID, string serverKey, string systemKey, string customerId, string configSitename, string configKey) => new NameValueCollection
        {
            {
                "ServerID", serverID
            },
            {
                "ServerKey", serverKey
            },
            {
                "SystemKey", systemKey
            },
            {
                "CustomerId", customerId
            },
            {
                "ConfigKey", configKey
            },
            {
                "ConfigSitename", configSitename
            }
        };
    }
}