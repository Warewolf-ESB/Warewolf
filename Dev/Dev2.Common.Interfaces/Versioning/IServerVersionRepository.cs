/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces.Versioning
{
    public interface IServerVersionRepository : IVersionRepository
    {
        void StoreVersion(IResource resource, string userName, string reason, Guid workSpaceId);
        void MoveVersions(Guid resourceId, string newPath);
    }
}