using Dev2.Common.Interfaces;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IServiceTestCommandHandler
    {
        IServiceTestModel CreateTest(IResourceModel resourceModel, int testNumber);
        void StopTest();
        void RunAllTestsInBrowser(bool isDirty);
        void RunAllTestsCommand(bool isDirty);
        void RunSelectedTestInBrowser();
        void RunSelectedTest();
        IServiceTestModel DuplicateTest(IServiceTestModel selectedTests);
        void DeleteTest(IServiceTestModel selectedServiceTest);
    }
}