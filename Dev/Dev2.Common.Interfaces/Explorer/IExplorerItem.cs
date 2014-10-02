
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
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Versioning;

namespace Dev2.Common.Interfaces.Explorer
{
    public interface IExplorerItem
    {
        string DisplayName { get; set; }
        Guid ServerId { get; set; }
        Guid ResourceId { get; set; }
        ResourceType ResourceType { get; set; }
        IList<IExplorerItem> Children { get; set; }
        Permissions Permissions { get; set; }
        IVersionInfo VersionInfo { get; set; }
        string ResourcePath { get; set; }
        IExplorerItem Parent { get; set; }
        string WebserverUri { get; set; }
    }
}
