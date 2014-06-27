using Dev2.Activities.PathOperations;
using Dev2.PathOperations;

namespace Dev2.Tests.Activities.Mocks
{
    public class MockAbstractMultipleFilesActivity : DsfAbstractMultipleFilesActivity
    {
        public MockAbstractMultipleFilesActivity(string displayName)
            : base(displayName)
        {
            ExecuteBrokerCalled = false;
            MoveRemainingIteratorsCalled = false;
        }

        #region Overrides of DsfAbstractMultipleFilesActivity
        public bool ExecuteBrokerCalled { get; set; }
        public bool MoveRemainingIteratorsCalled { get; set; }

        protected override string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint)
        {
            ExecuteBrokerCalled = true;
            return "";
        }

        protected override void MoveRemainingIterators()
        {
            MoveRemainingIteratorsCalled = true;
        }

        #endregion
    }
}