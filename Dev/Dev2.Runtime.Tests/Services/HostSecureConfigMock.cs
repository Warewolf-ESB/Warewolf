/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Specialized;
using Dev2.Runtime.Security;

namespace Dev2.Tests.Runtime.Services
{
    public class HostSecureConfigMock : HostSecureConfig
    {
        public NameValueCollection SaveConfigSettings { get; private set; }
        public int SaveConfigHitCount { get; private set; }
        public int ProtectConfigHitCount { get; private set; }

        public HostSecureConfigMock(NameValueCollection settings)
            : base(settings, true)
        {
        }

        protected override void SaveConfig(NameValueCollection secureSettings)
        {
            SaveConfigSettings = secureSettings;
            SaveConfigHitCount++;
        }

        protected override void ProtectConfig()
        {
            ProtectConfigHitCount++;
        }
    }
}
