using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface ITestCommandHandler
    {
        ITestModel CreateTestAction();
        void StopTestAction();
        void RunAllTestsInBrowserAction();
        void RunAllTestsCommandAction();
        void RunSelectedTestInBrowserAction();
        void RunSelectedTestAction();
        void DuplicateTestAction();
        void DeleteTestAction();
        void SaveAction();
    }
}