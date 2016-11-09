using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Threading;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IServiceTestCommandHandler
    {
        IServiceTestModel CreateTest(IResourceModel resourceModel, int testNumber);
        void StopTest(IContextualResourceModel resourceModel);
        void RunAllTestsCommand(bool isDirty, IEnumerable<IServiceTestModel> tests, IContextualResourceModel resourceModel, IAsyncWorker asyncWorker);
        IServiceTestModel DuplicateTest(IServiceTestModel selectedTests, int testNumber);
        void RunSelectedTest(IServiceTestModel selectedServiceTest, IContextualResourceModel resourceModel, IAsyncWorker asyncWorker);
        void RunSelectedTestInBrowser(string runSelectedTestUrl, IExternalProcessExecutor processExecutor);
        void RunAllTestsInBrowser(bool isDirty, string runAllTestUrl, IExternalProcessExecutor processExecutor);
        ObservableCollection<IServiceTestOutput> CreateServiceTestOutputFromResult(IEnumerable<IServiceTestOutput> stepStepOutputs, IServiceTestStep testStep);
    }
}