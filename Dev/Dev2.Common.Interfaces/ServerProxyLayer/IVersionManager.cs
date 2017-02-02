﻿using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Versioning;

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