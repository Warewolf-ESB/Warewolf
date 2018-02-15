using System.IO;

namespace Dev2.Data.PathOperations
{
    public abstract class PerformStreamIOOperation : ValidateAuthorization
    {                        
        public abstract Stream ExecuteOperationWithAuth();
        public abstract Stream ExecuteOperation();
    }
}
