using Dev2.Common.Interfaces.DB;
using System.Collections.Generic;

namespace Dev2.Comparer
{
    public class ServiceOutputMappingComparer : IEqualityComparer<IServiceOutputMapping>
    {
        public bool Equals(IServiceOutputMapping x, IServiceOutputMapping y)
        {
            if (x == null && y == null) return true;
            return x != null && y != null && x.Equals(y);
        }
        
        public int GetHashCode(IServiceOutputMapping obj)
        {
            return obj.GetHashCode();
        }
    }
}