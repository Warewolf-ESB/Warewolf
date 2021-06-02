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
using ChargeBee.Models.Enums;
using Warewolf.Enums;

namespace Warewolf.Licensing
{
    public class Subscription : ISubscription
    {
        public ISubscriptionData Retrieve(string subscriptionId)
        {
            var result = ChargeBee.Models.Subscription.Retrieve(subscriptionId).Request();
            return GenerateLicenseData(result);
        }

        //TODO: No coverage yet due to it probably being deleted later
        public ISubscriptionData CreatePlan(ISubscriptionData subscriptionData)
        {
            var result = ChargeBee.Models.Subscription.Create()
                .PlanId(subscriptionData.PlanId)
                .AutoCollection(AutoCollectionEnum.Off)
                .CustomerFirstName(subscriptionData.CustomerFirstName)
                .CustomerLastName(subscriptionData.CustomerLastName)
                .CustomerEmail(subscriptionData.CustomerEmail)
                .Request();
            return GenerateLicenseData(result);
        }

        //TODO: No coverage yet due to it probably being deleted later
        public ISubscriptionData UpgradePlan(ISubscriptionData subscriptionData)
        {
            var result = ChargeBee.Models.Subscription.Update(subscriptionData.CustomerId)
                .PlanId(subscriptionData.PlanId)
                .CardNumber(subscriptionData.CardNumber.ToString())
                .CardCvv(subscriptionData.CardCvv.ToString())
                .CardExpiryYear(subscriptionData.CardExpiryYear)
                .CardExpiryMonth(subscriptionData.CardExpiryMonth)
                .EndOfTerm(subscriptionData.EndOfTerm)
                .Request();
            return GenerateLicenseData(result);
        }

        private static ISubscriptionData GenerateLicenseData(EntityResult result)
        {
            var subscription = result.Subscription;
            var customer = result.Customer;

            var licenseData = new SubscriptionData
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