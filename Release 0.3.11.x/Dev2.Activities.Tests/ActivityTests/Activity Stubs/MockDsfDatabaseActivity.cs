using System;
using Dev2.DataList.Contract;
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

        public Guid MockExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors)
        {
            return base.ExecutionImpl(esbChannel, dataObject, inputs, outputs, out tmpErrors);
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

    }
}