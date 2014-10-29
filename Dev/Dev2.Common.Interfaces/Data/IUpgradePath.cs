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
using System.Xml.Linq;

namespace Dev2.Common.Interfaces.Data
{
    public interface IUpgradePath
    {
        Version UpgradesFrom { get; }
        Version UpgradesTo { get; }
        IResourceUpgrade Upgrade { get; }
        bool CanUpgrade(XElement sourceVersion);
    }
}