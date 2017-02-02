
using System.Collections.Generic;

namespace Dev2.Data
{
    public interface IComplexObject : IScalar
    {
        bool IsArray { get; set; }
        Dictionary<int, List<IComplexObject>> Children { get; set; }

        IComplexObject Parent { get; set; }
    }

    public class ComplexObject: Scalar, IComplexObject
    {

        public bool IsArray { get; set; }
        public Dictionary<int, List<IComplexObject>> Children { get; set; }       
        
        public IComplexObject Parent { get; set; } 
    }
}