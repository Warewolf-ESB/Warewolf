using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface IComputerNameProvider
    {
        void SetNames(IList<ComputerName> computerNames);
    }
}