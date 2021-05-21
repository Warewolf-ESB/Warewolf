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
using Dev2.Common;
using Warewolf.Enums;

namespace Warewolf.Licensing
{
    public class WarewolfLicense
    {
        private readonly IWarewolfLicense _warewolfLicense;

        public WarewolfLicense()
            : this(new LicenceApiConfig(), new WarewolfLicenseWrapper())
        {
        }

        public WarewolfLicense(ILicenceApiConfig config, IWarewolfLicense api)
        {
            ApiConfig.Configure(config.SiteName, config.ApiKey);
            _warewolfLicense = api;
        }

        //TODO: These might not be used. Just created the shell in the meantime. This will be decided when we do the registerUI
        public ILicenseData CreatePlan(ILicenseData licenseData)
        {
            var result = _warewolfLicense.CreatePlan(licenseData);
            result.IsLicensed = true;
            if (result.Status != SubscriptionStatus.Active && result.Status != SubscriptionStatus.InTrial)
            {
                result.IsLicensed = false;
            }

            GlobalConstants.LicenseCustomerId = result.CustomerId;
            GlobalConstants.LicensePlanId = result.PlanId;
            GlobalConstants.IsLicensed = result.IsLicensed;
            return result;
        }

        //TODO: These might not be used. Just created the shell in the meantime. This will be decided when we do the UpgradeUI
        public ILicenseData UpgradePlan(ILicenseData licenseData)
        {
            var result = _warewolfLicense.UpgradePlan(licenseData);
            result.IsLicensed = true;
            if (result.Status != SubscriptionStatus.Active && result.Status != SubscriptionStatus.InTrial)
            {
                result.IsLicensed = false;
            }

            GlobalConstants.LicensePlanId = result.PlanId;
            GlobalConstants.IsLicensed = result.IsLicensed;
            return result;
        }

        public ILicenseData Retrieve(ILicenseData licenseData)
        {
            try
            {
                if (licenseData.CustomerId == null || licenseData.CustomerId =="Unknown")
                {
                    licenseData.CustomerId = "Unknown";
                    licenseData.PlanId = "UnRegistered";
                    licenseData.IsLicensed = false;
                    GlobalConstants.LicensePlanId = licenseData.PlanId;
                    GlobalConstants.IsLicensed = licenseData.IsLicensed;
                    GlobalConstants.LicenseCustomerId = licenseData.CustomerId;
                    return licenseData;
                }
                var result = _warewolfLicense.Retrieve(licenseData);
                result.IsLicensed = true;

                if (result.Status != SubscriptionStatus.Active && result.Status != SubscriptionStatus.InTrial)
                {
                    result.IsLicensed = false;
                }

                GlobalConstants.LicensePlanId = result.PlanId;
                GlobalConstants.IsLicensed = result.IsLicensed;
                return result;
            }
            catch (Exception)
            {
                licenseData.IsLicensed = false;
                GlobalConstants.IsLicensed = licenseData.IsLicensed;
                return licenseData;
            }
        }
    }
}