/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Warewolf.Enums;
using Warewolf.Licensing;

namespace Dev2.Runtime.Subscription
{
    public interface ISubscriptionProvider
    {
        string SubscriptionKey { get; }
        string SubscriptionSiteName { get; }
        string CustomerId { get; }
        string PlanId { get; }
        string SubscriptionId { get; }
        bool IsLicensed { get; }
        bool StopExecutions { get; }

        SubscriptionStatus Status { get; }

        void SaveSubscriptionData(ISubscriptionData subscriptionData);

        ISubscriptionData GetSubscriptionData();

        ISubscriptionData DefaultSubscription();
    }
}