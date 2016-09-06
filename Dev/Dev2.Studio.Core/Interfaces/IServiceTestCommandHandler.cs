using Dev2.Common.Interfaces;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IServiceTestCommandHandler
    {
        IServiceTestModel CreateTest(IResourceModel resourceModel);
        void StopTest();
        void RunAllTestsInBrowser();
        void RunAllTestsCommand();
        void RunSelectedTestInBrowser();
        void RunSelectedTest();
        void DuplicateTest();
        void DeleteTest();
    }
}