using System;
using System.Globalization;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.Hosting
{
    public class VersionStrategy:IVersionStrategy   
    {
        #region Implementation of IVersionStrategy

        public IVersionInfo GetNextVersion(IResource newResource, IResource oldResource, string userName , string reason )
        {
            if (oldResource == null)
            {
                if(newResource.VersionInfo == null)
                {
                    return new VersionInfo(DateTime.Now, reason,userName, "1", newResource.ResourceID, Guid.NewGuid());
                }
            }
            else if (reason == "Rename")
            {
                if (oldResource.VersionInfo != null)
                    return new VersionInfo(DateTime.Now, reason, userName, oldResource.VersionInfo.VersionNumber, oldResource.ResourceID, oldResource.VersionInfo.VersionId);

            }

            else
            {
                if(oldResource.VersionInfo != null)
                return new VersionInfo(DateTime.Now, reason, userName, (1 + int.Parse(oldResource.VersionInfo.VersionNumber)).ToString(CultureInfo.InvariantCulture), oldResource.ResourceID, oldResource.VersionInfo.VersionId);
         
            }
            return new VersionInfo(DateTime.Now, reason, userName, "1", newResource.ResourceID, Guid.NewGuid());
        }

        #endregion


        public IVersionInfo GetCurrentVersion(IResource newResource, IResource oldResource, string userName, string reason)
        {
            if (oldResource !=null && oldResource.VersionInfo == null)
                return new VersionInfo(DateTime.Now, reason, userName, (1).ToString(CultureInfo.InvariantCulture), oldResource.ResourceID, Guid.NewGuid());


// ReSharper disable PossibleNullReferenceException
            return  oldResource.VersionInfo;
// ReSharper restore PossibleNullReferenceException
        }

        public IVersionInfo GetCurrentVersion(IResource newResource, IVersionInfo oldresource, string userName, string reason)
        {
            if (oldresource == null)
                return new VersionInfo(DateTime.Now, reason, userName, (1).ToString(CultureInfo.InvariantCulture), newResource.ResourceID, Guid.NewGuid());


            // ReSharper disable PossibleNullReferenceException
            return new VersionInfo(DateTime.Now, reason, userName, (1 + int.Parse(oldresource.VersionNumber)).ToString(CultureInfo.InvariantCulture), oldresource.ResourceId, oldresource.VersionId);
        }
    }
}