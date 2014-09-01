using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Runtime.Hosting
{
    public class ResourceUpgrade:IResourceUpgrade
    {
        public ResourceUpgrade( Func<XElement, XElement> upgradeFunc)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "upgradeFunc", upgradeFunc } });
            UpgradeFunc = upgradeFunc;
  
        }

        public Func<XElement, XElement> UpgradeFunc { get; private set; }

        #region Implementation of IResourceUpgrade
       




        #endregion
    }
}
