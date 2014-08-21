using System.Collections.Generic;
using Dev2.Common.Interfaces.Explorer;

namespace Dev2.Common.Interfaces.Versioning
{
    public interface IRollbackResult
    {
        IList<IExplorerItem> VersionHistory { get; set; }
        string DisplayName { get; set; }
    }
}