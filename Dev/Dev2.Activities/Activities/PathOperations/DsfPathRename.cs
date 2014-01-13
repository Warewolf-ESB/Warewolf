using Dev2.Activities.PathOperations;
using Dev2.Data.PathOperations.Interfaces;
using Dev2.PathOperations;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfPathRename : DsfAbstractMultipleFilesActivity, IPathInput, IPathOutput, IPathOverwrite, IDestinationUsernamePassword
    {
        public DsfPathRename()
            : base("Rename")
        {
        }

        protected override string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint)
        {
            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(Overwrite);
            var result = broker.Rename(scrEndPoint, dstEndPoint, opTO);
            return result.Replace("Move", "Rename");
        }
    }
}
