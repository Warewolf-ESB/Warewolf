using System;
using Dev2.DataList.Contract;
using Dev2.Runtime.Helpers;
using Dev2.Services.Execution;

namespace Dev2.Activities
{
    public class MockDsfDatabaseActivity : DsfDatabaseActivity
    {
        public MockDsfDatabaseActivity()
        {
        }

        public MockDsfDatabaseActivity(IServiceExecution exection)
        {
            ServiceExecution = exection;
        }

        public Guid MockExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, out ErrorResultTO tmpErrors)
        {
            return base.ExecutionImpl(esbChannel, dataObject, out tmpErrors);
        }

        public void MockBeforeExecutionStart(IDSFDataObject dsfDataObject)
        {
            var tmpErrors = new ErrorResultTO();
            BeforeExecutionStart(dsfDataObject, tmpErrors);
        }

        public void MockAfterExecutionCompleted()
        {
            var tmpErrors = new ErrorResultTO();
            AfterExecutionCompleted(tmpErrors);
        }

        public void MockCleanDataList(RuntimeHelpers runtimeHelpers, IDSFDataObject dataObject, Guid workspaceID, IDataListCompiler compiler)
        {
            var tmpErrors = new ErrorResultTO();
            base.CleanDataList(runtimeHelpers, dataObject, workspaceID, compiler, tmpErrors);
        }

        public int CleanDataListHitCount { get; private set; }
        protected override void CleanDataList(RuntimeHelpers runtimeHelpers, IDSFDataObject dataObject, Guid workspaceID, IDataListCompiler compiler, ErrorResultTO tmpErrors)
        {
            CleanDataListHitCount++;
        }
    }
}