using System;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Runtime.ResourceUpgrades
{
    public class BaseResourceUpgrader:IResourceUpgrade
    {
        #region Implementation of IResourceUpgrade
        public BaseResourceUpgrader()
        {
            UpgradeFunc = Upgrade;
        }

        XElement Upgrade(XElement arg)
        {

            return XElement.Parse(arg.ToString().Replace("clr-namespace:Dev2.Providers.Errors;assembly=Dev2.Infrastructure", "clr-namespace:Dev2.Common.Interfaces.Infrastructure.Providers.Errors;assembly=Dev2.Common.Interfaces").Replace("clr-namespace:Dev2.Interfaces;assembly=Dev2.Core", "clr-namespace:Dev2.Common.Interfaces.Core.Convertors.Case;assembly=Dev2.Common.Interfaces"));
        }

        public Func<XElement, XElement> UpgradeFunc { get; private set; }

        #endregion
    }
}
