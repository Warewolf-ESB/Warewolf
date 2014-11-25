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
using System.Text;
using Dev2.Common.Interfaces.Explorer;

namespace Dev2.Common.Interfaces.Versioning
{
    public interface IVersionRepository
    {
        IList<IExplorerItem> GetVersions(Guid resourceId);
        StringBuilder GetVersion(IVersionInfo version);
        IExplorerItem GetLatestVersionNumber(Guid resourceId);
        IRollbackResult RollbackTo(Guid resourceId, string versionNumber);
        IList<IExplorerItem> DeleteVersion(Guid resourceId, string versionNumber);
    }

    public interface IVersionManager
    {
        void MoveVersions(Guid resourceId, string newPath);
    }
}