
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Dev2.Services.Security;

namespace Dev2.Infrastructure.Tests.Services.Security
{
    public class TestSecurityServiceBase : SecurityServiceBase
    {
        protected override void OnDisposed()
        {
        }

        public List<WindowsGroupPermission> ReadPermissionsResult { get; set; }

        protected override List<WindowsGroupPermission> ReadPermissions()
        {
            return ReadPermissionsResult;
        }

        protected override void WritePermissions(List<WindowsGroupPermission> permissions)
        {
        }

        protected override void LogStart([CallerMemberName]string methodName = null)
        {
        }

        protected override void LogEnd([CallerMemberName]string methodName = null)
        {
        }
    }
}
