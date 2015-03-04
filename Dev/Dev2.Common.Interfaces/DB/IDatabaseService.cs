using System.Collections.Generic;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Dev2.Common.Interfaces.DB
{
    public interface IDatabaseService
    {
        string Name { get; set; }
        IDbSource Source { get; set; }
        IDbAction Action { get; set; }
        IList<IDbInput> Inputs { get; set; }
        IList<IDbOutputMapping> OutputMappings { get; set; }
    }
}