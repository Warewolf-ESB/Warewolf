using System;
using System.Xml.Linq;

namespace Dev2.Common.Interfaces.Data
{
    public interface IUpgradePath
    {
        Version UpgradesFrom{get;}
        Version UpgradesTo { get; }
        bool CanUpgrade (XElement sourceVersion);
        IResourceUpgrade Upgrade { get; }
    }
}
