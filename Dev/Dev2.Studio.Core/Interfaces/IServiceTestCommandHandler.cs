using Dev2.Common.Interfaces;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IServiceTestCommandHandler
    {
        IServiceTestModel CreateTest(IResourceModel resourceModel);
        void StopTest();
        void RunAllTestsInBrowser(bool isDirty);
        void RunAllTestsCommand(bool isDirty);
        void RunSelectedTestInBrowser();
        void RunSelectedTest();
        void DuplicateTest();
        void DeleteTest(IServiceTestModel selectedServiceTest);
    }
}