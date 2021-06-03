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
using ChargeBee.Api;
using Warewolf.Enums;

namespace Warewolf.Licensing
{
   public class WarewolfLicense : IWarewolfLicense
    {
        private readonly ISubscription _subscription;

        public WarewolfLicense()
            : this(new Subscription())
        {
        }

        public WarewolfLicense(ISubscription subscription)
        {
            _subscription = subscription;
        }

        public ISubscriptionData CreatePlan(ISubscriptionData subscriptionData)
        {
            if (!string.IsNullOrEmpty(subscriptionData.SubscriptionSiteName) || !string.IsNullOrEmpty(subscriptionData.SubscriptionKey))
            {
                ApiConfig.Configure(subscriptionData.SubscriptionSiteName, subscriptionData.SubscriptionKey);
            }
            var result = _subscription.CreatePlan(subscriptionData);
            result.IsLicensed = true;
            if(result.Status != SubscriptionStatus.Active && result.Status != SubscriptionStatus.InTrial)
            {
                result.IsLicensed = false;
            }
            return result;
        }

        public ISubscriptionData RetrievePlan(ISubscriptionData subscriptionData)
        {
            try
            {
                if (!string.IsNullOrEmpty(subscriptionData.SubscriptionSiteName) || !string.IsNullOrEmpty(subscriptionData.SubscriptionKey))
                {
                    ApiConfig.Configure(subscriptionData.SubscriptionSiteName, subscriptionData.SubscriptionKey);
                }
                var subscriptionId = subscriptionData.SubscriptionId;
                if(string.IsNullOrEmpty(subscriptionId))
                {
                    return DefaultLicenseData();
                }

                if(subscriptionId == "Unknown")
                {
                    return DefaultLicenseData();
                }

                var result = _subscription.RetrievePlan(subscriptionId);
                result.IsLicensed = true;

                if(result.Status != SubscriptionStatus.Active && result.Status != SubscriptionStatus.InTrial)
                {
                    result.IsLicensed = false;
                }
                return result;
            }
            catch(Exception)
            {
                return DefaultLicenseData();
            }
        }

        private static ISubscriptionData DefaultLicenseData()
        {
            var licenseData = new SubscriptionData
            {
                CustomerId = "Unknown",
                SubscriptionId = "None",
                PlanId = "NotActive",
                Status = SubscriptionStatus.NotActive,
                IsLicensed = false
            };
            return licenseData;
        }
    }
}