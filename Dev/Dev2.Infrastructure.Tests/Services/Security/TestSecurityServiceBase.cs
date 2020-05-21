/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Dev2.Services.Security;
using Warewolf;

namespace Dev2.Infrastructure.Tests.Services.Security
{
    public class TestSecurityServiceBase : SecurityServiceBase
    {
        protected override void OnDisposed()
        {
        }

        public List<WindowsGroupPermission> ReadPermissionsResults { get; set; }
        public SecuritySettingsTO  ReadSecuritySettingsResults { get; set; }

        protected override List<WindowsGroupPermission> ReadPermissions()
        {
            return ReadPermissionsResults;
        }
        protected override SecuritySettingsTO ReadSecuritySettings()
        {
            return ReadSecuritySettingsResults;
        }
        protected override void WritePermissions(List<WindowsGroupPermission> permissions, INamedGuid overrideResource,string secretKey)
        {
        }

        protected override void LogStart([CallerMemberName] string methodName = null)
        {
        }

        protected override void LogEnd([CallerMemberName] string methodName = null)
        {
        }
    }
}