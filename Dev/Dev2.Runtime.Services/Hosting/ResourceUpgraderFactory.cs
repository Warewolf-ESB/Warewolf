/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
            List<IUpgradePath> upgrades = new List<IUpgradePath>
            {
                new UpgradePath(
                    upgradesFrom: new Version(), 
                    upgradesTo: new Version(0, 4, 17, 27001), 
                    upgrade: new BaseResourceUpgrader()
                    ),
                    new UpgradePath(
                    upgradesFrom: new Version(0,4,2,3), 
                    upgradesTo: new Version(0, 5, 22, 27001), 
                    upgrade: new EncryptionResourceUpgrader()
                    )
            };
            return upgrades;
        }
    }
}
