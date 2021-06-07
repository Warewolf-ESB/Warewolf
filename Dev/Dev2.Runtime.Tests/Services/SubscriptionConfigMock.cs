/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Specialized;
using Dev2.Runtime.Subscription;

namespace Dev2.Tests.Runtime.Services
{
    public class SubscriptionConfigMock : SubscriptionConfig
    {
        public NameValueCollection SaveConfigSettings { get; private set; }
        public int SaveConfigHitCount { get; private set; }
        public NameValueCollection UpdateConfigSettings { get; private set; }
        public int UpdateConfigHitCount { get; private set; }

        public SubscriptionConfigMock(NameValueCollection settings)
            : base(settings)
        {
        }

        protected override void SaveConfig(NameValueCollection subscriptionSettings)
        {
            SaveConfigSettings = subscriptionSettings;
            SaveConfigHitCount++;
        }

        protected override void UpdateConfig(NameValueCollection subscriptionSettings)
        {
            UpdateConfigSettings = subscriptionSettings;
            UpdateConfigHitCount++;
        }
    }
}