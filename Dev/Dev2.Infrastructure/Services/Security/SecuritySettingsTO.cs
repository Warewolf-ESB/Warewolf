
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure;

namespace Dev2.Services.Security
{
    // ReSharper disable InconsistentNaming
    public class SecuritySettingsTO
        // ReSharper restore InconsistentNaming
    {
        public SecuritySettingsTO()
        {
            WindowsGroupPermissions = new List<IWindowsGroupPermission>();
        }

        public SecuritySettingsTO(IEnumerable<IWindowsGroupPermission> permissions)
            : this()
        {
            if(permissions != null)
            {
                WindowsGroupPermissions.AddRange(permissions);
            }
        }

        public List<IWindowsGroupPermission> WindowsGroupPermissions { get; private set; }
        public TimeSpan CacheTimeout { get; set; }
    }
}
