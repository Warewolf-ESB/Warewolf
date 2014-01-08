using Dev2.Activities.PathOperations;
using Dev2.Data.PathOperations.Interfaces;
using Dev2.PathOperations;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfPathRename : DsfAbstractMultipleFilesActivity, IPathInput, IPathOutput, IPathOverwrite, IDestinationUsernamePassword
    {
        public DsfPathRename()
            : base("Rename")
        {
        }

        protected override string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint, Dev2CRUDOperationTO opTO)
        {
            var result = broker.Rename(scrEndPoint, dstEndPoint, opTO);
            return result.Replace("Move", "Rename");
        }
    }
}
