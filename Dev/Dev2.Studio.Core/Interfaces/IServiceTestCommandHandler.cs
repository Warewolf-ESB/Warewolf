using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Threading;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IServiceTestCommandHandler
    {
        IServiceTestModel CreateTest(IResourceModel resourceModel, int testNumber);
        void StopTest();
        void RunAllTestsInBrowser(bool isDirty);
        void RunAllTestsCommand(bool isDirty);
        void RunSelectedTestInBrowser();
        IServiceTestModel DuplicateTest(IServiceTestModel selectedTests, int testNumber);
        void RunSelectedTest(IServiceTestModel selectedServiceTest, IContextualResourceModel resourceModel, IAsyncWorker asyncWorker);
    }
}