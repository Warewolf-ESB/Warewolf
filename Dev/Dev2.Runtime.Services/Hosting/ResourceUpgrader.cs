using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Runtime.Hosting
{
    public class ResourceUpgrader:IResourceUpgrader
    {
        public ResourceUpgrader(List<IUpgradePath> availableUpgrades)
        {
            VerifyArgument.IsNotNull("availableUpgrades",availableUpgrades);
            AvailableUpgrades = availableUpgrades;

        }

        #region Implementation of IResourceUpgrader

        public XElement UpgradeResource(XElement sourceVersion, Version destinationVersion, Action<XElement> onUpgrade)
        {
            var available = AvailableUpgrades.Where(a => a.CanUpgrade(sourceVersion)).OrderBy(a=>a.UpgradesFrom).Select(a=>a.Upgrade.UpgradeFunc).ToList();
            if (available.Any())
            {
                var outputLang = available.Aggregate((a, b) => (x => (b(a(x)))));
                
                var output =  outputLang(sourceVersion);
                output.SetAttributeValue("ServerVersion",Assembly.GetExecutingAssembly().GetName().Version);
                onUpgrade(output);
                return output;
            }
          
            return sourceVersion;
        }

        public List<IUpgradePath> AvailableUpgrades { get; private set; }

        #endregion
    }
}
