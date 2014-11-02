/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ResourceUpgrades;

namespace Dev2.Runtime.Hosting
{
    public static class ResourceUpgraderFactory
    {
        public static IResourceUpgrader GetUpgrader()
        {
            return new ResourceUpgrader(CreateUpgradePath());
        }

        private static List<IUpgradePath> CreateUpgradePath()
        {
            var upgrades = new List<IUpgradePath>
            {
                new UpgradePath(new Version(), new Version(0, 4, 17, 27001), new BaseResourceUpgrader())
            };
            return upgrades;
        }
    }
}