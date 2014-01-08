using Dev2.Activities.PathOperations;
using Dev2.Data.PathOperations.Interfaces;
using Dev2.PathOperations;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfPathCopy : DsfAbstractMultipleFilesActivity, IPathOverwrite, IPathInput, IPathOutput,
                               IDestinationUsernamePassword
    {
        public DsfPathCopy()
            : base("Copy")
        {
        }

        protected override string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint, Dev2CRUDOperationTO opTO)
        {
            return broker.Copy(scrEndPoint, dstEndPoint, opTO);
        }
    }
}
