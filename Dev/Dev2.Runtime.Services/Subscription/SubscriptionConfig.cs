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
using System.IO;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Services.Security;
using Warewolf.Licensing;
using Warewolf.Resource.Errors;
using Warewolf.Enums;
#if NETFRAMEWORK
using static Dev2.Runtime.Hosting.ServerExplorerRepository;
#endif

namespace Dev2.Runtime.Subscription
{
    public class SubscriptionConfig : ISubscriptionConfig
    {
        const string SectionName = "subscriptionSettings";
        internal const string FileName = "Warewolf License.secureconfig";

        /// <summary>
        /// Absolute path to this instance's <c>Warewolf License.secureconfig</c> file. Computed
        /// once at construction from an explicit/injected base path (see
        /// <see cref="ForBasePath"/>), defaulting to
        /// <see cref="AppContext.BaseDirectory"/> — the deployment directory of the running
        /// assembly — for both hosts (Dev2.Server and the Lightweight isolated-worker Function
        /// App). Deliberately never resolved against <see cref="Environment.CurrentDirectory"/>:
        /// that is process-wide, mutable, and not guaranteed to equal the deployment directory
        /// under the Azure Functions isolated-worker host, which is what silently broke license
        /// resolution there (see docs/SubscriptionConfig-IsolatedWorker-Resolution-Spec.md).
        /// </summary>
        private readonly string _configFilePath;

        /// <summary>
        /// Production constructor. Resolves the license file against
        /// <see cref="AppContext.BaseDirectory"/> — correct for both hosts without requiring a
        /// DI container or any host-specific startup wiring.
        /// </summary>
        public SubscriptionConfig() : this(AppContext.BaseDirectory, fromFile: true)
        {
        }

        /// <summary>
        /// Test-only constructor: bypasses file resolution entirely and initializes directly
        /// from the supplied settings. Still resolves <see cref="_configFilePath"/> against
        /// <see cref="AppContext.BaseDirectory"/> for consistency, in case a caller other than
        /// a mocked subclass ever reaches <see cref="SaveConfig"/> from this path.
        /// </summary>
        public SubscriptionConfig(NameValueCollection settings)
        {
            _configFilePath = Path.Combine(AppContext.BaseDirectory, FileName);
            Initialize(settings);
        }

        /// <summary>
        /// Backing constructor shared by the parameterless constructor and
        /// <see cref="ForBasePath"/>. Not public itself — a public <c>SubscriptionConfig(string)</c>
        /// overload would be ambiguous with <see cref="SubscriptionConfig(NameValueCollection)"/>
        /// for a <c>null</c> argument (both are single-reference-type overloads), which existing
        /// callers rely on resolving to the <c>NameValueCollection</c> overload (see
        /// <c>SubscriptionConfigTests.SubscriptionConfig_WithoutConfig_Expected_ThrowsArgumentNullException</c>).
        /// </summary>
        private SubscriptionConfig(string basePath, bool fromFile)
        {
            _configFilePath = Path.Combine(
                string.IsNullOrWhiteSpace(basePath) ? AppContext.BaseDirectory : basePath,
                FileName);

            try
            {
                var settings = EnsureSubscriptionConfigFileExists();
                Initialize(settings);
            }
            catch(Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
            }
        }

        /// <summary>
        /// Constructs against an explicit base directory instead of
        /// <see cref="AppContext.BaseDirectory"/>. Exists so callers (and tests) can inject the
        /// deployment directory explicitly rather than relying on ambient process state — the
        /// fix for the CWD-relative resolution bug this class used to have (see
        /// docs/SubscriptionConfig-IsolatedWorker-Resolution-Spec.md). A static factory method,
        /// not a constructor overload, to avoid the overload-resolution ambiguity described on
        /// the private constructor above.
        /// </summary>
        /// <param name="basePath">
        /// Directory the license file lives in. Falls back to <see cref="AppContext.BaseDirectory"/>
        /// when null/whitespace.
        /// </param>
        public static SubscriptionConfig ForBasePath(string basePath) => new SubscriptionConfig(basePath, fromFile: true);

        public string SubscriptionKey { get; private set; }
        public string SubscriptionSiteName { get; private set; }
        public string CustomerId { get; private set; }
        public string PlanId { get; private set; }
        public string SubscriptionId { get; private set; }
        public string MarketplaceResourceId { get; private set; }
        public string Status { get; private set; }

        public bool StopExecutions { get; private set; }

        protected void Initialize(NameValueCollection settings)
        {
            if(settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if(settings["SubscriptionSiteName"] != "" || settings["SubscriptionKey"] != "")
            {
                bool isPlainText = false;
                SubscriptionKey = DecryptKey(settings["SubscriptionKey"]);
                isPlainText |= settings["SubscriptionKey"] != string.Empty && SubscriptionKey == settings["SubscriptionKey"];
                SubscriptionSiteName = DecryptKey(settings["SubscriptionSiteName"]);
                isPlainText |= settings["SubscriptionSiteName"] != string.Empty && SubscriptionSiteName == settings["SubscriptionSiteName"];
                CustomerId = DecryptKey(settings["CustomerId"]);
                isPlainText |= settings["CustomerId"] != string.Empty && CustomerId == settings["CustomerId"];
                PlanId = DecryptKey(settings["PlanId"]);
                isPlainText |= settings["PlanId"] != string.Empty && PlanId == settings["PlanId"];
                SubscriptionId = DecryptKey(settings["SubscriptionId"]);
                isPlainText |= settings["SubscriptionId"] != string.Empty && SubscriptionId == settings["SubscriptionId"];
                MarketplaceResourceId = DecryptKey(settings["MarketplaceResourceId"]);
                isPlainText |= settings["MarketplaceResourceId"] != string.Empty && MarketplaceResourceId == settings["MarketplaceResourceId"];
                Status = DecryptKey(settings["Status"]);
                isPlainText |= settings["Status"] != string.Empty && Status == settings["Status"];
                StopExecutions = bool.Parse(DecryptKey(settings["StopExecutions"]));
                isPlainText |= settings["StopExecutions"] != string.Empty && bool.TryParse(settings["StopExecutions"], out bool settingParsedAsPlainText);
                if (isPlainText)
                {
                    Enum.TryParse(Status, out SubscriptionStatus status);
                    UpdateSubscriptionSettings(new SubscriptionData()
                    {
                        SubscriptionKey = SubscriptionKey,
                        SubscriptionSiteName = SubscriptionSiteName,
                        CustomerId = CustomerId,
                        PlanId = PlanId,
                        SubscriptionId = SubscriptionId,
                        MarketplaceResourceId = MarketplaceResourceId,
                        Status = status,
                        StopExecutions = StopExecutions
                    });
                }
            }
            else
            {
                //Broken Installation
                Dev2Logger.Warn(
                    $"Subscription settings at '{_configFilePath}' were empty/missing key values. " +
                    "Writing default (unlicensed) settings over this file.",
                    GlobalConstants.WarewolfWarn);
                SaveConfig(BuildDefaultSettings());
            }
        }

        /// <summary>
        /// Reads this instance's <see cref="_configFilePath"/> directly, bypassing
        /// <c>ConfigurationManager</c>/<c>configSource</c> entirely — that indirection is what
        /// silently broke under the isolated-worker host (see docs/
        /// SubscriptionConfig-IsolatedWorker-Resolution-Spec.md, root cause). When the file does
        /// not yet exist at the resolved absolute path, a fresh default (unlicensed) file is
        /// written there and returned, exactly as before, but now targeting the *same* absolute
        /// path this method just checked — so a "not found" read can never overwrite a
        /// *different*, real file elsewhere, which is the failure mode this fix closes.
        /// </summary>
        NameValueCollection EnsureSubscriptionConfigFileExists()
        {
            if (!File.Exists(_configFilePath))
            {
                Dev2Logger.Info(string.Format(ErrorResource.FileNotFound, _configFilePath), GlobalConstants.WarewolfInfo);

                var newSettings = BuildDefaultSettings();
                SaveConfig(newSettings);
                return newSettings;
            }

            return ReadConfigFile(_configFilePath);
        }

        /// <summary>
        /// Parses the <c>&lt;subscriptionSettings&gt;&lt;add key="..." value="..." /&gt;...</c>
        /// XML shape <see cref="UpdateConfig"/> writes, directly from disk at an absolute path —
        /// the read-side counterpart to <see cref="UpdateConfig"/>, replacing the
        /// <c>ConfigurationManager.GetSection</c>/<c>configSource</c> redirect this class used to
        /// depend on.
        /// </summary>
        static NameValueCollection ReadConfigFile(string configFilePath)
        {
            var settings = new NameValueCollection();
            var root = XDocument.Load(configFilePath).Root;
            if (root != null)
            {
                foreach (var add in root.Elements("add"))
                {
                    var key = (string)add.Attribute("key");
                    if (!string.IsNullOrEmpty(key))
                    {
                        settings[key] = (string)add.Attribute("value") ?? string.Empty;
                    }
                }
            }

            return settings;
        }

        /// <summary>
        /// The "broken/fresh installation" default settings shared by both
        /// <see cref="Initialize"/>'s empty-values branch and <see cref="EnsureSubscriptionConfigFileExists"/>'s
        /// missing-file branch — factored out so the two call sites can never drift apart.
        /// </summary>
        static NameValueCollection BuildDefaultSettings()
        {
            // ReSharper disable once RedundantAssignment
            var subscriptionKey = SubscriptionProvider.SubscriptionLiveKey;
            // ReSharper disable once RedundantAssignment
            var subscriptionSiteName = SubscriptionProvider.SubscriptionLiveSiteName;
#if DEBUG
            subscriptionKey = SubscriptionProvider.SubscriptionTestKey;
            subscriptionSiteName = SubscriptionProvider.SubscriptionTestSiteName;
#endif
            var newSettings = new NameValueCollection();
            newSettings["CustomerId"] = "";
            newSettings["SubscriptionId"] = "";
            newSettings["MarketplaceResourceId"] = "";
            newSettings["Status"] = SubscriptionProvider.SubscriptionDefaultStatus;
            newSettings["PlanId"] = SubscriptionProvider.SubscriptionDefaultPlanId;
            newSettings["SubscriptionKey"] = subscriptionKey;
            newSettings["SubscriptionSiteName"] = subscriptionSiteName;
            newSettings["StopExecutions"] = SubscriptionProvider.StopExecutionsDefault;
            return newSettings;
        }

        public void UpdateSubscriptionSettings(ISubscriptionData subscriptionData)
        {
            try
            {
                var newSettings = new NameValueCollection();
                newSettings["CustomerId"] = SecurityEncryption.Encrypt(subscriptionData.CustomerId);
                newSettings["SubscriptionId"] = SecurityEncryption.Encrypt(subscriptionData.SubscriptionId);
                newSettings["MarketplaceResourceId"] = SecurityEncryption.Encrypt(subscriptionData.MarketplaceResourceId ?? string.Empty);
                newSettings["Status"] = SecurityEncryption.Encrypt(subscriptionData.Status.ToString());
                newSettings["PlanId"] = SecurityEncryption.Encrypt(subscriptionData.PlanId);
                newSettings["SubscriptionKey"] = SecurityEncryption.Encrypt(subscriptionData.SubscriptionKey);
                newSettings["SubscriptionSiteName"] = SecurityEncryption.Encrypt(subscriptionData.SubscriptionSiteName);
                newSettings["StopExecutions"] = SecurityEncryption.Encrypt(subscriptionData.StopExecutions.ToString());
                SaveConfig(newSettings);
            }
            catch(Exception e)
            {
                Dev2Logger.Error("Failed to update Subscription Settings", GlobalConstants.WarewolfError);
                throw;
            }
        }

        protected virtual void SaveConfig(NameValueCollection subscriptionSettings)
        {
            UpdateConfig(subscriptionSettings, _configFilePath);
        }

        /// <summary>
        /// Writes to <paramref name="configFilePath"/> — an absolute path resolved once at
        /// construction (see <see cref="_configFilePath"/>) — instead of the bare relative
        /// <see cref="FileName"/> this used to resolve against
        /// <see cref="Environment.CurrentDirectory"/>. That CWD-relative write is what let a
        /// "file not found" read (itself CWD-relative) silently overwrite a real, correctly
        /// staged license file living elsewhere on disk under the isolated-worker host.
        /// </summary>
        private static void UpdateConfig(NameValueCollection subscriptionSettings, string configFilePath)
        {
            try
            {
                var config = new XElement(SectionName);
                foreach(string key in subscriptionSettings.Keys)
                {
                    config.Add(
                        new XElement(
                            "add",
                            new XAttribute("key", key),
                            new XAttribute("value", subscriptionSettings[key])
                        ));
                }

                var configDoc = new XDocument(new XDeclaration("1.0", "utf-8", ""), config);
                configDoc.Save(configFilePath, SaveOptions.None);
            }
            catch(Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
            }
        }

        public static string DecryptKey(string base64String)
        {
            return SecurityEncryption.TryDecrypt(base64String).TrimEnd('\0');
        }

        public static NameValueCollection CreateSettings(
            string customerId,
            string planId,
            string subscriptionId,
            string status,
            string subscriptionSiteName,
            string subscriptionKey,
            string stopExecutions,
            string marketplaceResourceId = "") => new NameValueCollection
        {
            {
                "CustomerId", customerId
            },
            {
                "PlanId", planId
            },
            {
                "SubscriptionId", subscriptionId
            },
            {
                "MarketplaceResourceId", marketplaceResourceId
            },
            {
                "Status", status
            },
            {
                "SubscriptionKey", subscriptionKey
            },
            {
                "SubscriptionSiteName", subscriptionSiteName
            },
            {
                "StopExecutions", stopExecutions
            }
        };
    }
}