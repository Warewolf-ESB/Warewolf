
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Runtime.Security
{
    /// <summary>
    /// The Secure Config
    /// </summary>
    public class HostSecureConfig : ISecureConfig
    {
        const string SectionName = "secureSettings";

        public const string FileName = "Warewolf Server.exe.secureconfig";

        #region Ctor

        public HostSecureConfig()
        {
            try
            {
                EnsureSecureConfigFileExists();
                var settings = (NameValueCollection)ConfigurationManager.GetSection(SectionName);
                Initialize(settings, true);
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error(e);
            }
        }

        public HostSecureConfig(NameValueCollection settings, bool shouldProtectConfig = true)
        {
            Initialize(settings, shouldProtectConfig);
        }

        #endregion

        #region Properties

        public Guid ServerID { get; private set; }

        public RSACryptoServiceProvider ServerKey { get; private set; }

        public RSACryptoServiceProvider SystemKey { get; private set; }

        #endregion

        #region Initialize

        /// <summary>
        /// Initializes config with the given values and optionally protects it.
        /// <remarks>
        /// If <paramref name="shouldProtectConfig"/> is <code>true</code>, then the config file must exist on disk.
        /// </remarks>
        /// </summary>
        /// <param name="settings">The settings to be loaded.</param>
        /// <param name="shouldProtectConfig"><code>true</code> if the configuration should be protected; <code>false</code> otherwise.</param>
        /// <exception cref="System.ArgumentNullException">settings</exception>
        protected void Initialize(NameValueCollection settings, bool shouldProtectConfig)
        {
            if(settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            SystemKey = CreateKey(settings["SystemKey"]);
            Guid serverID;
            if(Guid.TryParse(settings["ServerID"], out serverID) && serverID != Guid.Empty)
            {
                ServerID = serverID;
                ServerKey = CreateKey(settings["ServerKey"]);
            }
            else
            {
                // BUG 8395: settings is a ReadOnlyNameValueCollection so create a new collection instead 
                var newSettings = new NameValueCollection();

                #region New server installation so initialize config with new server id and key

                ServerID = Guid.NewGuid();
                ServerKey = new RSACryptoServiceProvider();

                newSettings["ServerID"] = ServerID.ToString();
                newSettings["ServerKey"] = Convert.ToBase64String(ServerKey.ExportCspBlob(true));
                newSettings["SystemKey"] = settings["SystemKey"];

                #endregion

                SaveConfig(newSettings);

                if(shouldProtectConfig)
                {
                    ProtectConfig();
                }
            }
        }

        #endregion

        #region EnsureSecureConfigFileExists

        private void EnsureSecureConfigFileExists()
        {
            ConfigurationManager.RefreshSection(SectionName);
            // We need to check both the live and development paths ;)
            if(!File.Exists(FileName))
            {
                Dev2Logger.Log.Info("File not found: " + FileName);
                var newSettings = new NameValueCollection();
                newSettings["ServerID"] = "";
                newSettings["ServerKey"] = "";
                newSettings["SystemKey"] = HostSecurityProvider.InternalPublicKey;
                SaveConfig(newSettings);
            }
        }

        #endregion EnsureSecureConfigFileExists

        #region SaveConfig

        /// <summary>
        /// Saves the given secure settings into XML configuration file called <see cref="FileName"/>.
        /// </summary>
        /// <param name="secureSettings">The settings to be saved.</param>
        protected virtual void SaveConfig(NameValueCollection secureSettings)
        {
            var config = new XElement(SectionName);
            foreach(string key in secureSettings.Keys)
            {
                config.Add(new XElement("add",
                                        new XAttribute("key", key),
                                        new XAttribute("value", secureSettings[key])
                               ));
            }

            var configDoc = new XDocument(new XDeclaration("1.0", "utf-8", ""), config);
            configDoc.Save(FileName, SaveOptions.None);
        }

        #endregion

        #region ProtectConfig

        /// <summary>
        /// Protects the configuration using the <see cref="RsaProtectedConfigurationProvider"/>.
        /// </summary>
        protected virtual void ProtectConfig()
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var section = config.GetSection(SectionName);
                if(section != null)
                {
                    if(!section.SectionInformation.IsProtected)
                    {
                        if(!section.ElementInformation.IsLocked)
                        {
                            section.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");
                            section.SectionInformation.ForceSave = true;
                            config.Save(ConfigurationSaveMode.Full);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error(e);
                throw;
            }
        }

        #endregion

        #region CreateKey

        /// <summary>
        /// Creates the <see cref="RSACryptoServiceProvider"/> from the given base64 encoded key.
        /// </summary>
        /// <param name="base64String">The base64 encoded key.</param>
        /// <returns>A  <see cref="RSACryptoServiceProvider"/>.</returns>
        public static RSACryptoServiceProvider CreateKey(string base64String)
        {
            var keyBlob = Convert.FromBase64String(base64String);
            var key = new RSACryptoServiceProvider();
            key.ImportCspBlob(keyBlob);
            return key;
        }


        #endregion

        #region CreateSettings

        /// <summary>
        /// Creates the a <see cref="NameValueCollection"/> configuration settings.
        /// </summary>
        /// <param name="serverID">The server ID.</param>
        /// <param name="serverKey">The server key.</param>
        /// <param name="systemKey">The system key.</param>
        /// <returns>a <see cref="NameValueCollection"/> configuration.</returns>
        public static NameValueCollection CreateSettings(string serverID, string serverKey, string systemKey)
        {
            return new NameValueCollection
            {
                {
                    "ServerID", serverID
                },
                {
                    "ServerKey", serverKey
                },
                {
                    "SystemKey", systemKey
                }
            };
        }

        #endregion

    }
}
