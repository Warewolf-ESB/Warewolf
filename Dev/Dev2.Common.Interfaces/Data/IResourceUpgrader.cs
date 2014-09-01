using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Dev2.Common.Interfaces.Data
{
    public interface IResourceUpgrader
    {
        XElement UpgradeResource(XElement sourceVersion, Version destinationVersion,Action<XElement> OnUpgrade);
        List<IUpgradePath> AvailableUpgrades { get; }
  
    }
}
