using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Threading;
using Dev2.Data;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network;

namespace Warewolf.Studio.ViewModels
{
    public sealed class ServiceTestCommandHandlerModel : IServiceTestCommandHandler
    {
        private readonly DataListConversionUtils _dataListConversionUtils;

        public ServiceTestCommandHandlerModel()
        {
            DataList = new DataListModel();
            _dataListConversionUtils = new DataListConversionUtils();
        }

        public DataListModel DataList { get; set; }

        public IServiceTestModel CreateTest(IResourceModel resourceModel, int testNumber)
        {
            var testModel = new ServiceTestModel(resourceModel.ID)
            {
                TestName = "Test " + (testNumber == 0 ? 1 : testNumber),
                TestPending = true,
                Enabled = true,
                NewTest = true,
                Inputs = new List<IServiceTestInput>(),
                Outputs = new List<IServiceTestOutput>()
            };
            if (!string.IsNullOrEmpty(resourceModel.DataList))
            {
                DataList = new DataListModel();
                DataList.Create(resourceModel.DataList, resourceModel.DataList);
                var inputList = _dataListConversionUtils.GetInputs(DataList);
                var outputList = _dataListConversionUtils.GetOutputs(DataList);
                testModel.Inputs = inputList
                                    .Select(sca =>
                    {
                        var serviceTestInput = new ServiceTestInput(sca.DisplayValue, sca.Value);
                        serviceTestInput.AddNewAction = () => testModel.AddRow(serviceTestInput, DataList);
                        return (IServiceTestInput)serviceTestInput;
                    }).ToList();

                testModel.Outputs = outputList
                                    .Select(sca => new ServiceTestOutput(sca.DisplayValue, sca.Value) as IServiceTestOutput).ToList();
            }
            testModel.Item = testModel;
            return testModel;
        }


        public void StopTest()
        {
        }

        public void RunAllTestsInBrowser(bool isDirty)
        {
            if (isDirty)
            {
                ShowRunAllUnsavedError();
                //return;
            }
            //Run all tests
        }

        public void RunAllTestsCommand(bool isDirty)
        {
            if (isDirty)
            {
                ShowRunAllUnsavedError();
                //return;
            }
            //Run all tests
        }

        private static void ShowRunAllUnsavedError()
        {
            var popupController = CustomContainer.Get<Dev2.Common.Interfaces.Studio.Controller.IPopupController>();
            popupController?.Show(Resources.Languages.Core.ServiceTestRunAllUnsavedTestsMessage,
                Resources.Languages.Core.ServiceTestRunAllUnsavedTestsHeader, MessageBoxButton.OK, MessageBoxImage.Error, null,
                false, true, false, false);
        }

        public void RunSelectedTestInBrowser()
        {
        }
        

        public IServiceTestModel DuplicateTest(IServiceTestModel selectedTest)
        {
            var testClone = new ServiceTestModel(selectedTest.ParentId)
            {
                TestName = selectedTest.TestName,
                Inputs = selectedTest.Inputs,
                Outputs = selectedTest.Outputs,
                AuthenticationType = selectedTest.AuthenticationType,
                Enabled = true,
                ErrorExpected = selectedTest.ErrorExpected,
                NoErrorExpected = selectedTest.NoErrorExpected,
                IsTestSelected = true,
                Password = selectedTest.Password,
                TestPending = true,
                NewTest = true,
                UserName = selectedTest.UserName,
            };
            return testClone;
        }

        public void RunSelectedTest(IServiceTestModel selectedServiceTest, IContextualResourceModel resourceModel, IAsyncWorker asyncWorker)
        {
            asyncWorker.Start(() => WebServer.ExecuteTest(resourceModel, selectedServiceTest.TestName), res =>
            {
                if (res.Result == RunResult.TestFailed)
                {
                    selectedServiceTest.TestFailing = true;
                    selectedServiceTest.TestPassed = false;
                }
                else if(res.Result == RunResult.TestPassed)
                {
                    selectedServiceTest.TestFailing = false;
                    selectedServiceTest.TestPassed = true;
                }
            });
        }
    }
}