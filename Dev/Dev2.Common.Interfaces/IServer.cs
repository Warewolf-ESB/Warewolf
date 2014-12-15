using System.Collections.Generic;
using Dev2.Common.Interfaces.Explorer;

namespace Dev2.Common.Interfaces
{
    public interface IServer
    {
        bool Connect();
        IList<IExplorerItem> Load();
    }
}