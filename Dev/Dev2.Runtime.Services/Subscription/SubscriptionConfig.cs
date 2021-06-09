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
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Services.Security;
using Warewolf.Licensing;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.Subscription
{
    public class SubscriptionConfig : ISubscriptionConfig
    {
        const string SectionName = "subscriptionSettings";
        private const string FileName = "Warewolf License.secureconfig";

        public SubscriptionConfig()
        {
            try
            {
                EnsureSubscriptionConfigFileExists();
                var settings = (NameValueCollection)ConfigurationManager.GetSection(SectionName);
                Initialize(settings);
            }
            catch(Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
            }
        }

        public SubscriptionConfig(NameValueCollection settings)
        {
            Initialize(settings);
        }

        public string SubscriptionKey { get; private set; }
        public string SubscriptionSiteName { get; private set; }
        public string CustomerId { get; private set; }
        public string PlanId { get; private set; }
        public string SubscriptionId { get; private set; }
        public string Status { get; private set; }

        protected void Initialize(NameValueCollection settings)
        {
            if(settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if(settings["SubscriptionSiteName"] != "" || settings["SubscriptionKey"] != "")
            {
                SubscriptionKey = DecryptKey(settings["SubscriptionKey"]);
                SubscriptionSiteName = DecryptKey(settings["SubscriptionSiteName"]);
                CustomerId = DecryptKey(settings["CustomerId"]);
                PlanId = DecryptKey(settings["PlanId"]);
                SubscriptionId = DecryptKey(settings["SubscriptionId"]);
                Status = DecryptKey(settings["Status"]);
            }
            else
            {
                //Broken Installation
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
                newSettings["Status"] = SubscriptionProvider.SubscriptionDefaultStatus;
                newSettings["PlanId"] = SubscriptionProvider.SubscriptionDefaultPlanId;
                newSettings["SubscriptionKey"] = subscriptionKey;
                newSettings["SubscriptionSiteName"] = subscriptionSiteName;
                SaveConfig(newSettings);
            }
        }

        void EnsureSubscriptionConfigFileExists()
        {
            ConfigurationManager.RefreshSection(SectionName);
            if(!File.Exists(FileName))
            {
                Dev2Logger.Info(string.Format(ErrorResource.FileNotFound, FileName), GlobalConstants.WarewolfInfo);

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
                newSettings["Status"] = SubscriptionProvider.SubscriptionDefaultStatus;
                newSettings["PlanId"] = SubscriptionProvider.SubscriptionDefaultPlanId;
                newSettings["SubscriptionKey"] = subscriptionKey;
                newSettings["SubscriptionSiteName"] = subscriptionSiteName;
                SaveConfig(newSettings);
            }
        }
        public void UpdateSubscriptionSettings(ISubscriptionData subscriptionData)
        {
            try
            {
                var newSettings = new NameValueCollection();
                newSettings["CustomerId"] = SecurityEncryption.Encrypt(subscriptionData.CustomerId);
                newSettings["SubscriptionId"] = SecurityEncryption.Encrypt(subscriptionData.SubscriptionId);
                newSettings["Status"] = SecurityEncryption.Encrypt(subscriptionData.Status.ToString());
                newSettings["PlanId"] = SecurityEncryption.Encrypt(subscriptionData.PlanId);
                newSettings["SubscriptionKey"] = SecurityEncryption.Encrypt(subscriptionData.SubscriptionKey);
                newSettings["SubscriptionSiteName"] = SecurityEncryption.Encrypt(subscriptionData.SubscriptionSiteName);
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
            UpdateConfig(subscriptionSettings);
        }

        private static void UpdateConfig(NameValueCollection subscriptionSettings)
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
                configDoc.Save(FileName, SaveOptions.None);
            }
            catch(Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
            }
        }

        public static string DecryptKey(string base64String)
        {
            return SecurityEncryption.Decrypt(base64String).TrimEnd('\0');
        }

        public static NameValueCollection CreateSettings(string customerId, string planId, string subscriptionId, string status, string subscriptionSiteName, string subscriptionKey) => new NameValueCollection
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
                "Status", status
            },
            {
                "SubscriptionKey", subscriptionKey
            },
            {
                "SubscriptionSiteName", subscriptionSiteName
            }
        };
    }
}