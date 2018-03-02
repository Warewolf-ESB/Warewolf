using System.IO;

namespace Dev2.Data.PathOperations
{
    public abstract class PerformStreamIOOperation
    {                        
        public abstract Stream ExecuteOperationWithAuth();
        public abstract Stream ExecuteOperation();
    }
}
