using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace Dev2.DynamicServices.Security
{
    public class HostSecureConfig : ISecureConfig
    {
        const string SectionName = "secureSettings";

        public const string FileName = "Unlimited.Applications.DynamicServicesHost.exe.secureconfig";

        #region Ctor

        public HostSecureConfig() :
            this((NameValueCollection)ConfigurationManager.GetSection(SectionName))
        {
        }

        public HostSecureConfig(NameValueCollection settings, bool saveNew = true)
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
                #region New server installation so initialize config with new server id and key

                ServerID = Guid.NewGuid();
                ServerKey = new RSACryptoServiceProvider();

                settings["ServerID"] = ServerID.ToString();
                settings["ServerKey"] = Convert.ToBase64String(ServerKey.ExportCspBlob(true));

                #endregion

                if(saveNew)
                {
                    SaveConfig(settings);
                    ProtectConfig();
                }
            }
        }

        #endregion

        #region Properties

        public Guid ServerID { get; private set; }

        public RSACryptoServiceProvider ServerKey { get; private set; }

        public RSACryptoServiceProvider SystemKey { get; private set; }

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

        #region ProtectConfig

        static void ProtectConfig()
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

        #endregion

        #region SaveConfig

        /// <summary>
        /// Saves the given secure settings into XML configuration file called <see cref="FileName"/>.
        /// </summary>
        /// <param name="secureSettings">The settings to be saved.</param>
        public static void SaveConfig(NameValueCollection secureSettings)
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
