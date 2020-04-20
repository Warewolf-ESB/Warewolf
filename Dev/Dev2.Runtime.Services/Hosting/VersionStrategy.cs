#pragma warning disable
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
using System.Globalization;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Data;

namespace Dev2.Runtime.Hosting
{
    public class VersionStrategy:IVersionStrategy   
    {
        #region Implementation of IVersionStrategy

        public IVersionInfo GetNextVersion(IResource newResource, IResource oldresource, string userName , string reason )
        {
            if (oldresource == null)
            {
                if(newResource.VersionInfo == null)
                {
                    return new VersionInfo(DateTime.Now, reason,userName, "1", newResource.ResourceID, Guid.NewGuid());
                }
            }
            else if (reason == "Rename")
            {
                if (oldresource.VersionInfo != null)
                {
                    return new VersionInfo(DateTime.Now, reason, userName, oldresource.VersionInfo.VersionNumber, oldresource.ResourceID, oldresource.VersionInfo.VersionId);
                }
            }

            else
            {
                if(oldresource.VersionInfo != null)
                {
                    return new VersionInfo(DateTime.Now, reason, userName, (1 + int.Parse(oldresource.VersionInfo.VersionNumber)).ToString(CultureInfo.InvariantCulture), oldresource.ResourceID, oldresource.VersionInfo.VersionId);
                }
            }
            return new VersionInfo(DateTime.Now, reason, userName, "1", newResource.ResourceID, Guid.NewGuid());
        }

        #endregion

        public IVersionInfo GetCurrentVersion(IResource newResource, IVersionInfo oldresource, string userName, string reason)
        {
            if (oldresource == null)
            {
                return new VersionInfo(DateTime.Now, reason, userName, 1.ToString(CultureInfo.InvariantCulture), newResource.ResourceID, Guid.NewGuid());
            }

            return new VersionInfo(DateTime.Now, reason, userName, (1 + int.Parse(oldresource.VersionNumber)).ToString(CultureInfo.InvariantCulture), oldresource.ResourceId, oldresource.VersionId);
        }

        public IVersionInfo GetCurrentVersion(IResource newResource, IResource oldresource, string userName, string reason)
        {
            if (oldresource != null && oldresource.VersionInfo == null)
            {
                return new VersionInfo(DateTime.Now, reason, userName, 1.ToString(CultureInfo.InvariantCulture), oldresource.ResourceID, Guid.NewGuid());
            }

            return oldresource?.VersionInfo;            
        }
    }
}
