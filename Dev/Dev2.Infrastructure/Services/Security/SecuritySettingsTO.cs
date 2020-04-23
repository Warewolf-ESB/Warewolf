/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Warewolf.Data;

namespace Dev2.Services.Security
{
    public class SecuritySettingsTO

    {
        public SecuritySettingsTO()
        {
            WindowsGroupPermissions = new List<WindowsGroupPermission>();
            OverrideResource = new NamedGuid();
        }

        public SecuritySettingsTO(IEnumerable<WindowsGroupPermission> permissions, NamedGuid overrideResource)
            : this()
        {
            if (permissions != null)
            {
                WindowsGroupPermissions.AddRange(permissions);
            }
            OverrideResource = overrideResource;
        }

        public SecuritySettingsTO(IEnumerable<WindowsGroupPermission> permissions)
            : this()
        {
            if (permissions != null)
            {
                WindowsGroupPermissions.AddRange(permissions);
            }
        }

        public NamedGuid OverrideResource { get; set; }
        public List<WindowsGroupPermission> WindowsGroupPermissions { get; private set; }
        public TimeSpan CacheTimeout { get; set; }
    }
}