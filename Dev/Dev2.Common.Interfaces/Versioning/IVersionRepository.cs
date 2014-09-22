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