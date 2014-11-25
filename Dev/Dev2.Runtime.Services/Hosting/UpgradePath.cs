
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
    public class UpgradePath:IUpgradePath
    {
        public UpgradePath(Version upgradesFrom, Version upgradesTo, IResourceUpgrade upgrade)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "upgradesFrom", upgradesFrom }, { "upgradesTo", upgradesTo }, { "upgrade", upgrade } });
            Upgrade = upgrade;
            UpgradesTo = upgradesTo;
            UpgradesFrom = upgradesFrom;
        }

        #region Implementation of IUpgradePath

        public Version UpgradesFrom { get; private set; }
        public Version UpgradesTo { get; private set; }

        public bool CanUpgrade(XElement sourceVersion)
        {
            var version = GetVersionFromXML(sourceVersion);
            return version.CompareTo(UpgradesFrom) < 0 || (version.CompareTo(new Version()) ==0 && UpgradesFrom.CompareTo(version)==0) ;
        }
        private Version GetVersionFromXML(XElement resource)
        {
            if (resource.Attribute("ServerVersion") == null)
                return new Version();
            return Version.Parse(resource.Attribute("ServerVersion").Value);
        }
        public IResourceUpgrade Upgrade { get; private set; }

        #endregion
    }
}
