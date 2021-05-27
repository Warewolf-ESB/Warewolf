/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using ChargeBee.Api;
using ChargeBee.Models;
using ChargeBee.Models.Enums;
using Warewolf.Enums;

namespace Warewolf.Licensing
{
    public class WarewolfLicenseWrapper : IWarewolfLicense
    {
        public ILicenseData Retrieve(ILicenseData licenseData)
        {
            var result = Subscription.Retrieve(licenseData.SubscriptionId).Request();
            return GenerateLicenseData(result);
        }

        //TODO: No coverage yet due to it probably being deleted later
        public ILicenseData CreatePlan(ILicenseData licenseData)
        {
            var result = Subscription.Create()
                .PlanId(licenseData.PlanId)
                .AutoCollection(AutoCollectionEnum.Off)
                .CustomerFirstName(licenseData.CustomerFirstName)
                .CustomerLastName(licenseData.CustomerLastName)
                .CustomerEmail(licenseData.CustomerEmail)
                .Request();
            return GenerateLicenseData(result);
        }

        //TODO: No coverage yet due to it probably being deleted later
        public ILicenseData UpgradePlan(ILicenseData licenseData)
        {
            var result = Subscription.Update(licenseData.CustomerId)
                .PlanId(licenseData.PlanId)
                .CardNumber(licenseData.CardNumber.ToString())
                .CardCvv(licenseData.CardCvv.ToString())
                .CardExpiryYear(licenseData.CardExpiryYear)
                .CardExpiryMonth(licenseData.CardExpiryMonth)
                .EndOfTerm(licenseData.EndOfTerm)
                .Request();
            return GenerateLicenseData(result);
        }

        private static ILicenseData GenerateLicenseData(EntityResult result)
        {
            var subscription = result.Subscription;
            var customer = result.Customer;

            var licenseData = new LicenseData
            {
                CustomerId = subscription.CustomerId,
                SubscriptionId = subscription.Id,
                PlanId = subscription.PlanId,
                SubscriptionStatus = subscription.Status.ToString(),
                Status = (SubscriptionStatus)subscription.Status,
                CustomerFirstName = customer.FirstName,
                CustomerLastName = customer.LastName,
                CustomerEmail = customer.Email,
                TrialEnd = subscription.TrialEnd
            };
            return licenseData;
        }
    }
}