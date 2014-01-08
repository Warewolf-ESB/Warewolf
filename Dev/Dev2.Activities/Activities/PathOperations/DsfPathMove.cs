using Dev2.Activities.PathOperations;
using Dev2.Data.PathOperations.Interfaces;
using Dev2.PathOperations;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfPathMove : DsfAbstractMultipleFilesActivity, IPathInput, IPathOutput, IPathOverwrite,
                               IDestinationUsernamePassword
    {
        public DsfPathMove()
            : base("Move")
        {
        }

        protected override string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint, Dev2CRUDOperationTO opTO)
        {
            return broker.Move(scrEndPoint, dstEndPoint, opTO);
        }
    }
}
