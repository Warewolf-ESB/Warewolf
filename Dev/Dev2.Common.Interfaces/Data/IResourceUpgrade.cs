using System;
using System.Xml.Linq;

namespace Dev2.Common.Interfaces.Data
{
    public interface IResourceUpgrade
    {

        Func<XElement, XElement> UpgradeFunc { get;}
    }
}