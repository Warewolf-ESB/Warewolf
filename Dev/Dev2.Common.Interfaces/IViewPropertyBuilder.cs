using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Common.Interfaces
{
    public interface IViewPropertyBuilder
    {
        List<KeyValuePair<string, string>> BuildProperties(IDbActionToolRegion<IDbAction> actionToolRegion, ISourceToolRegion<IDbSource> sourceToolRegion, string type);
    }
}