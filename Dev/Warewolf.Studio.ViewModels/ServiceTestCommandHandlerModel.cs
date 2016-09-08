using System.Linq;
using System.Windows;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Data;
using Dev2.Studio.Core.Interfaces;

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

        public IServiceTestModel CreateTest(IResourceModel resourceModel)
        {
            var testModel = new ServiceTestModel
            {
                TestName = "Test 1",
                IsDirty = true,
                TestPending =  true,
                Enabled = true
            };
            if (!string.IsNullOrEmpty(resourceModel.DataList))
            {

                DataList.Create(resourceModel.DataList,resourceModel.DataList);
                var inputList = _dataListConversionUtils.GetInputs(DataList);
                var outputList = _dataListConversionUtils.GetOutputs(DataList);
                testModel.Inputs = inputList
                                    .Select(sca =>
                    {
                        var serviceTestInput = new ServiceTestInput(sca.DisplayValue, sca.Value);
                        serviceTestInput.AddNewAction = () => testModel.AddRow(serviceTestInput, DataList);
                        return serviceTestInput as IServiceTestInput;
                    }).ToList();

                testModel.Outputs =  outputList
                                    .Select(sca => new ServiceTestOutput(sca.DisplayValue, sca.Value) as IServiceTestOutput).ToList();
            }
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

        public void RunSelectedTest()
        {
        }

        public void DuplicateTest()
        {
        }

        public void DeleteTest(IServiceTestModel selectedServiceTest)
        {
            var popupController = CustomContainer.Get<Dev2.Common.Interfaces.Studio.Controller.IPopupController>();
            if (popupController.ShowDeleteConfirmation(selectedServiceTest.NameForDisplay.Replace("*", "").TrimEnd(' ')) == MessageBoxResult.Yes)
            {
                //Delete the test
            }
        }
    }
}