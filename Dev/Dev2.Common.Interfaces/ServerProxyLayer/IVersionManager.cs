/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Versioning;
using Warewolf.Data;

namespace Dev2.Common.Interfaces.ServerProxyLayer
{
    /// <summary>
    /// Manages the version of a warewolf resource
    /// </summary>
    public interface IVersionManager
    {
        /// <summary>
        /// Get a list of versions of a resource
        /// </summary>
        /// <param name="resourceId">the resource</param>
        /// <returns>the resource versions. N configured versions are stored on a server</returns>
        IList<IExplorerItem> GetVersions(Guid resourceId);
        /// <summary>
        /// rollback to a specific version 
        /// </summary>
        /// <param name="resourceId">the resource</param>
        /// <param name="versionNumber">the version to rollback to</param>
        IRollbackResult RollbackTo(Guid resourceId, string versionNumber);

        /// <summary>
        /// Delete a version o a resource
        /// </summary>
        /// <param name="resourceId">the resource</param>
        /// <param name="versionNumber">the version to delete</param>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        IList<IExplorerItem> DeleteVersion(Guid resourceId, string versionNumber, string resourcePath);

        StringBuilder GetVersion(IVersionInfo versionInfo, Guid resourceId);
    }
}