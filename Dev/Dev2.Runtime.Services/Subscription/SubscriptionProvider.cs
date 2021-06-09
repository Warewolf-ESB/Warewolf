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
using Warewolf.Enums;
using Warewolf.Licensing;

namespace Dev2.Runtime.Subscription
{
    public class SubscriptionProvider : ISubscriptionProvider
    {
        public static readonly string SubscriptionTestKey = "wCYcjqzbAiHIneFFib+LCrn73SSkOlRzm4QxP+mkeHsH7e3surKN5liDsrv39JFR";
        public static readonly string SubscriptionTestSiteName = "L8NilnImZ18r8VCMD88AdQ==";
        public static readonly string SubscriptionLiveKey = "ml420y+ZHMiv8CoQJxF1XMsYXXxCcDgNkvFkZSJHQB+m3EdlYIeUAP8oEIl9Z29b";
        public static readonly string SubscriptionLiveSiteName = "tWPn5xcpWET9NX3yt+uPHQ==";
        public static readonly string SubscriptionDefaultPlanId = "qj2HmQwVsUt12btj/iXadA==";
        public static readonly string SubscriptionDefaultStatus = "aT/AoVWEMyf6OPvaYp47Gw==";
        public string SubscriptionKey { get; }
        public string SubscriptionSiteName { get; }
        public string CustomerId { get; }
        public string PlanId { get; }
        public string SubscriptionId { get; }
        public SubscriptionStatus Status { get; }
        public bool IsLicensed { get; }
        static volatile ISubscriptionProvider _theInstance;
        static readonly object SyncRoot = new object();
        private static ISubscriptionConfig _config;
        public static ISubscriptionProvider Instance
        {
            get
            {
                if(_theInstance == null)
                {
                    lock(SyncRoot)
                    {
                        if(_theInstance == null)
                        {
                            _config = new SubscriptionConfig();
                            _theInstance = new SubscriptionProvider(_config);
                        }
                    }
                }

                return _theInstance;
            }
        }

        private static ISubscriptionProvider RefreshInstance
        {
            get
            {
                _config = new SubscriptionConfig();
                _theInstance = new SubscriptionProvider(_config);
                return _theInstance;
            }
        }

        protected SubscriptionProvider(ISubscriptionConfig config)
        {
            if(config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _config = config;
            SubscriptionKey = config.SubscriptionKey;
            SubscriptionSiteName = config.SubscriptionSiteName;
            CustomerId = config.CustomerId;
            PlanId = config.PlanId;
            SubscriptionId = config.SubscriptionId;
            Enum.TryParse(config.Status, out SubscriptionStatus status);
            Status = status;
            IsLicensed = Status == SubscriptionStatus.Active || Status == SubscriptionStatus.InTrial;
        }

        public void SaveSubscriptionData(ISubscriptionData subscriptionData)
        {
            var newSubscriptionData = SetNewSubscriptionData(subscriptionData);
            _config.UpdateSubscriptionSettings(newSubscriptionData);
            _theInstance = RefreshInstance;
        }

        private ISubscriptionData SetNewSubscriptionData(ISubscriptionData subscriptionData)
        {
            var newSubscriptionData = new SubscriptionData
            {
                CustomerId = subscriptionData.CustomerId,
                SubscriptionId = subscriptionData.SubscriptionId,
                PlanId = subscriptionData.PlanId,
                Status = subscriptionData.Status,
                SubscriptionSiteName = SubscriptionSiteName,
                SubscriptionKey = SubscriptionKey
            };
            return newSubscriptionData;
        }

        public ISubscriptionData GetSubscriptionData()
        {
            return new SubscriptionData
            {
                CustomerId = CustomerId,
                SubscriptionId = SubscriptionId,
                PlanId = PlanId,
                Status = Status,
                SubscriptionSiteName = SubscriptionSiteName,
                SubscriptionKey = SubscriptionKey,
                IsLicensed = IsLicensed,
            };
        }
    }
}