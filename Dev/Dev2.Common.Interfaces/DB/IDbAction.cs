using System.Collections.Generic;

namespace Dev2.Common.Interfaces.DB
{
    public interface IDbAction
    {
        IList<IServiceInput> Inputs { get; set; }
        string Name { get; set; }
        IDictionary<string, List<string>> Test();
    }
}