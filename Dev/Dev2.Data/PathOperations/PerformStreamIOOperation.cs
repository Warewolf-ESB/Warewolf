using System.IO;

namespace Dev2.Data.PathOperations
{
    public abstract class PerformStreamIOOperation : ImpersonationOperation
    {
        protected PerformStreamIOOperation(ImpersonationDelegate impersonationDelegate) : base(impersonationDelegate)
        {
        }

        public abstract Stream ExecuteOperationWithAuth();
        public abstract Stream ExecuteOperation();
    }
}
