using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface ITestFrameworkViewModel
    {
        ITestFrameworkModel SelectedTest { get; set; }
        ITestFrameworkCommandHandler TestFrameworkCommandHandler { get; set; }
        string RunAllTestsUrl { get; set; }
        string TestPassingResult { get; set; }
        ObservableCollection<ITestFrameworkModel> TestsList  { get; set; }
    }

    public interface ITestFrameworkCommandHandler
    {
        ICommand RenameCommand { get; set; }
        ICommand SaveCommand { get; set; }
        ICommand EnableTestCommand { get; set; }
        ICommand DisableTestCommand { get; set; }
        ICommand DeleteTestCommand { get; set; }
        ICommand DuplicateTestCommand { get; set; }
        ICommand RunAllTestsInBrowserCommand { get; set; }
        ICommand RunAllTestsCommand { get; set; }
        ICommand RunSelectedTestInBrowserCommand { get; set; }
        ICommand RunSelectedTestCommand { get; set; }
        ICommand StopTestCommand { get; set; }
        ICommand CreateTestCommand { get; set; }
    }

    public interface ITestFrameworkModel
    {
        string TestName { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        List<ITestInput> Inputs { get; set; }
        List<ITestOutPut> OutPuts { get; set; }
        bool Error { get; set; }
        bool IsNewTest { get; set; }
        bool IsTestSelected { get; set; }
    }

    public interface ITestInput
    {
        string Variable { get; set; }
        string Value { get; set; }
        bool EmptyIsNull { get; set; }
    }

    public interface ITestOutPut
    {
        string Variable { get; set; }
        string Value { get; set; }
    }
}