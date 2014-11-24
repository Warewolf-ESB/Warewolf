
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
