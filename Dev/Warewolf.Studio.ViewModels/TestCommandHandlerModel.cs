using System.Collections.ObjectModel;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Commands;

namespace Warewolf.Studio.ViewModels
{
    public class TestCommandHandlerModel : ITestCommandHandler
    {
        

        public ITestModel CreateTestAction()
        {
            var testModel = new TestModel
            {
                TestName = "Test 1"
            };
            return testModel;
       }

        

        public void StopTestAction()
        {
        }

      

        public void RunAllTestsInBrowserAction()
        {
        }

        

        public void RunAllTestsCommandAction()
        {
        }
        

        public void RunSelectedTestInBrowserAction()
        {
        }

        

        public void RunSelectedTestAction()
        {
        }

        

        public void DuplicateTestAction()
        {
        }

       

        public void DeleteTestAction()
        {
        }

        

        public void SaveAction()
        {
        }
        

    }
}