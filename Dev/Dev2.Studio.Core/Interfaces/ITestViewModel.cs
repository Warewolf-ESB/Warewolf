using System.Collections.ObjectModel;
using System.Windows.Input;
using Dev2.Common.Interfaces;

namespace Dev2.Studio.Core.Interfaces
{
    public interface ITestViewModel
    {
        ITestModel SelectedTest { get; set; }
        ITestCommandHandler TestCommandHandler { get; set; }
        string RunAllTestsUrl { get; set; }
        string TestPassingResult { get; set; }
        ObservableCollection<ITestModel> Tests  { get; set; }

        ICommand SaveCommand { get; set; }
        ICommand DeleteTestCommand { get; set; }
        ICommand DuplicateTestCommand { get; set; }
        ICommand RunAllTestsInBrowserCommand { get; set; }
        ICommand RunAllTestsCommand { get; set; }
        ICommand RunSelectedTestInBrowserCommand { get; set; }
        ICommand RunSelectedTestCommand { get; set; }
        ICommand StopTestCommand { get; set; }
        ICommand CreateTestCommand { get; set; }
    }
}