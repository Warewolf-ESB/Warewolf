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
        public bool SubscriptionExists(ISubscriptionData subscriptionData)
        {
            var subscriptionList = ChargeBee.Models.Customer.List()
                .FirstName().Is(subscriptionData.CustomerFirstName)
                .LastName().Is(subscriptionData.CustomerLastName)
                .Email().Is(subscriptionData.CustomerEmail)
                .Request();
            foreach(var item in subscriptionList.List)
            {
                var customer = item.Customer;
                var machineName = customer.GetValue<string>("cf_machinename", false);

                if(machineName == null)
                {
                    return false;
                }

                if(subscriptionData.MachineName == machineName)
                {
                    return true;
                }
            }

            return false;
        }

        public ISubscriptionData RetrievePlan(string subscriptionId)
        {
            var result = ChargeBee.Models.Subscription.Retrieve(subscriptionId).Request();
            return GenerateLicenseData(result);
        }

        public ISubscriptionData CreatePlan(ISubscriptionData subscriptionData)
        {
            var result = ChargeBee.Models.Subscription.Create()
                .PlanId(subscriptionData.PlanId)
                .Param("customer[cf_machinename]", subscriptionData.MachineName)
                .AutoCollection(AutoCollectionEnum.Off)
                .CustomerFirstName(subscriptionData.CustomerFirstName)
                .CustomerLastName(subscriptionData.CustomerLastName)
                .CustomerEmail(subscriptionData.CustomerEmail)
                .PlanQuantity(subscriptionData.NoOfCores)
                .Request();
            return GenerateLicenseData(result);
        }

        private static ISubscriptionData GenerateLicenseData(EntityResult result)
        {
            var subscription = result.Subscription;
            var customer = result.Customer;
            var machineName = customer.GetValue<string>("cf_machinename", false);

            var licenseData = new SubscriptionData
            {
                CustomerId = subscription.CustomerId,
                SubscriptionId = subscription.Id,
                PlanId = subscription.PlanId,
                NoOfCores = subscription.PlanQuantity,
                Status = (SubscriptionStatus)subscription.Status,
                CustomerFirstName = customer.FirstName,
                CustomerLastName = customer.LastName,
                CustomerEmail = customer.Email,
                MachineName = machineName,
                TrialEnd = subscription.TrialEnd
            };
            return licenseData;
        }
    }
}