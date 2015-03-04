using System.Collections.Generic;

namespace Dev2.Common.Interfaces.DB
{
    public interface IDbAction
    {
        IList<IDbInput> Inputs { get; set; }
        string Name { get; set; }
  
    }
}