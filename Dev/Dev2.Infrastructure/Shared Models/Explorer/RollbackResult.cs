using System.Collections.Generic;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Versioning;
namespace Dev2.Explorer
{
    public class RollbackResult : IRollbackResult
    {
        public IList<IExplorerItem> VersionHistory { get; set; }
        public string DisplayName { get; set; }
    }
}