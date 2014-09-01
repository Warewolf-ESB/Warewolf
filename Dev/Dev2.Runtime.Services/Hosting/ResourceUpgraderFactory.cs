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
            List<IUpgradePath> upgrades = new List<IUpgradePath> { new UpgradePath(new Version(), new Version(0, 4, 16, 26347), new BaseResourceUpgrader()) };
           return upgrades;
        }
    }
}
