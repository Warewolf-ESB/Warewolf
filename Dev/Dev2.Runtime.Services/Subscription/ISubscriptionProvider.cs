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
        string MarketplaceResourceId { get; }
        bool IsLicensed { get; }
        bool StopExecutions { get; }

        SubscriptionStatus Status { get; }

        void SaveSubscriptionData(ISubscriptionData subscriptionData);

        /// <summary>
        /// Licenses (or re-licenses) this instance from <paramref name="subscriptionData"/>,
        /// including its <c>SubscriptionKey</c> — unlike <see cref="SaveSubscriptionData"/>,
        /// which always keeps this instance's existing <c>SubscriptionKey</c>/<c>SubscriptionSiteName</c>
        /// regardless of what is passed in (that method exists for the Chargebee plan/status
        /// update path, where the key must not change). <c>SubscriptionSiteName</c> is still never
        /// caller-settable through either method: it always stays whatever this instance already
        /// has. Intended for MCP/administrative tooling that needs to license a deployed-but-unlicensed
        /// (or re-key) instance without redeploying or staging a file over Kudu/VFS.
        /// </summary>
        void SetLicense(ISubscriptionData subscriptionData);

        ISubscriptionData GetSubscriptionData();

        ISubscriptionData DefaultSubscription();
    }
}