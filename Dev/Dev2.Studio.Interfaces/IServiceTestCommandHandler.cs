#pragma warning disable
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Threading;

namespace Dev2.Studio.Interfaces
{
    public interface IServiceTestCommandHandler
    {
        IServiceTestModel CreateTest(IResourceModel resourceModel, int testNumber);
        IServiceTestModel CreateTest(IResourceModel resourceModel, int testNumber, bool isFromDebug);
        void StopTest(IContextualResourceModel resourceModel);
        void RunAllTestsCommand(bool isDirty, IEnumerable<IServiceTestModel> tests, IContextualResourceModel resourceModel, IAsyncWorker asyncWorker);
        IServiceTestModel DuplicateTest(IServiceTestModel selectedTests, int testNumber);

        void RunSelectedTest(IServiceTestModel selectedServiceTest, IContextualResourceModel resourceModel, IAsyncWorker asyncWorker);

        void RunSelectedTestInBrowser(string runSelectedTestUrl, IExternalProcessExecutor processExecutor);

        void RunAllTestsInBrowser(bool isDirty, string runAllTestUrl, IExternalProcessExecutor processExecutor);
        void RunAllTestCoverageInBrowser(bool isDirty, string runAllCoverageUrl, IExternalProcessExecutor processExecutor);
    }
}