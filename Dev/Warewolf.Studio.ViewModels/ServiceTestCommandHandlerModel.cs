using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dev2;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
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

        private DataListModel DataList { get; set; }


        public IServiceTestModel CreateTest(IResourceModel resourceModel, int testNumber)
        {
            var testModel = new ServiceTestModel(resourceModel.ID)
            {
                TestName = "Test " + (testNumber == 0 ? 1 : testNumber),
                TestPending = true,
                Enabled = true,
                NewTest = true,
                NoErrorExpected = true,
                ErrorExpected = false,
                ErrorContainsText = string.Empty,
                Inputs = new ObservableCollection<IServiceTestInput>(),
                Outputs = new ObservableCollection<IServiceTestOutput>(),
            };
            if (!string.IsNullOrEmpty(resourceModel.DataList))
            {
                DataList = new DataListModel();
                DataList.Create(resourceModel.DataList, resourceModel.DataList);
                var inputList = _dataListConversionUtils.GetInputs(DataList);
                var outputList = _dataListConversionUtils.GetOutputs(DataList);
                testModel.Inputs = inputList.Select(sca =>
                    {
                        var serviceTestInput = new ServiceTestInput(sca.DisplayValue, "");
                        serviceTestInput.AddNewAction = () => testModel.AddRow(serviceTestInput, DataList);
                        return (IServiceTestInput)serviceTestInput;
                    }).ToObservableCollection();

                testModel.Outputs = outputList.Select(sca =>
                {
                    var serviceTestOutput = new ServiceTestOutput(sca.DisplayValue, "", "", "");
                    serviceTestOutput.AddNewAction = () => testModel.AddRow(serviceTestOutput, DataList);
                    return (IServiceTestOutput)serviceTestOutput;
                }).ToObservableCollection();
            }
            testModel.Item = testModel;
            return testModel;
        }


        public void StopTest(IContextualResourceModel resourceModel)
        {
            resourceModel.Environment.ResourceRepository.StopExecution(resourceModel);
        }

        public void RunAllTestsCommand(bool isDirty, IEnumerable<IServiceTestModel> tests, IContextualResourceModel resourceModel, IAsyncWorker asyncWorker)
        {
            if (isDirty)
            {
                ShowRunAllUnsavedError();
                return;
            }
            foreach (var serviceTestModel in tests)
            {
                RunSelectedTest(serviceTestModel, resourceModel, asyncWorker);
            }
        }

        private static void ShowRunAllUnsavedError()
        {
            var popupController = CustomContainer.Get<IPopupController>();
            popupController?.Show(Resources.Languages.Core.ServiceTestRunAllUnsavedTestsMessage,
                Resources.Languages.Core.ServiceTestRunAllUnsavedTestsHeader, MessageBoxButton.OK, MessageBoxImage.Error, null,
                false, true, false, false);
        }


        public IServiceTestModel DuplicateTest(IServiceTestModel selectedTest, int testNumber)
        {
            var nameForDisplay = selectedTest.NameForDisplay.Replace(" *", "") + " " + (testNumber == 0 ? 1 : testNumber);

            var clone = (ServiceTestModel)selectedTest.Clone();
            clone.TestName = selectedTest.TestName + " " + (testNumber == 0 ? 1 : testNumber);
            clone.Enabled = true;
            clone.IsTestSelected = true;
            clone.TestPending = true;
            clone.NewTest = true;
            clone.NameForDisplay = nameForDisplay + " *";

            return clone;
        }

        public void RunSelectedTest(IServiceTestModel selectedServiceTest, IContextualResourceModel resourceModel, IAsyncWorker asyncWorker)
        {
            if (selectedServiceTest == null || resourceModel == null || asyncWorker == null || selectedServiceTest.IsNewTest)
            {
                return;
            }
            selectedServiceTest.IsTestRunning = true;
            asyncWorker.Start(() => resourceModel.Environment.ResourceRepository.ExecuteTest(resourceModel, selectedServiceTest.TestName), res =>
            {
                if (res?.Result != null)
                {
                    selectedServiceTest.TestFailing = res.Result.RunTestResult == RunResult.TestFailed;
                    selectedServiceTest.TestPassed = res.Result.RunTestResult == RunResult.TestPassed;
                    selectedServiceTest.TestInvalid = res.Result.RunTestResult == RunResult.TestInvalid;
                    selectedServiceTest.TestPending = res.Result.RunTestResult != RunResult.TestFailed &&
                                                      res.Result.RunTestResult != RunResult.TestPassed &&
                                                      res.Result.RunTestResult != RunResult.TestInvalid &&
                                                      res.Result.RunTestResult != RunResult.TestResourceDeleted &&
                                                      res.Result.RunTestResult != RunResult.TestResourcePathUpdated;


                    selectedServiceTest.Outputs = res.Outputs?.Select(output =>
                    {
                        var serviceTestOutput = new ServiceTestOutput(output.Variable, output.Value, output.From, output.To) as IServiceTestOutput;
                        serviceTestOutput.AssertOp = output.AssertOp;
                        serviceTestOutput.Result = output.Result;
                        return serviceTestOutput;
                    }).ToObservableCollection();
                    if (selectedServiceTest.TestSteps != null)
                    {
                        foreach (var resTestStep in res.TestSteps)
                        {
                            foreach (var serviceTestStep in selectedServiceTest.TestSteps.Where(testStep => testStep.UniqueId == resTestStep.UniqueId))
                            {
                                serviceTestStep.Result = resTestStep.Result;

                               
                                    foreach (var testStep in res.TestSteps.Where(testStep => testStep.UniqueId == serviceTestStep.UniqueId))
                                    {
                                        serviceTestStep.Result = testStep.Result;

                                        serviceTestStep.StepOutputs = CreateServiceTestOutputFromResult(resTestStep.StepOutputs, serviceTestStep as ServiceTestStep);
                                    }
                                
                                foreach (var serviceTestOutput in serviceTestStep.StepOutputs)
                                {
                                    serviceTestOutput.Result = resTestStep.StepOutputs[0].Result;
                                }
                            }
                        }
                    }

                    if (res.Result.RunTestResult == RunResult.TestResourceDeleted)
                    {
                        var popupController = CustomContainer.Get<IPopupController>();
                        popupController?.Show(Resources.Languages.Core.ServiceTestResourceDeletedMessage, Resources.Languages.Core.ServiceTestResourceDeletedHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false);
                        var shellViewModel = CustomContainer.Get<IShellViewModel>();
                        shellViewModel.CloseResourceTestView(resourceModel.ID, resourceModel.ServerID, resourceModel.Environment.ID);
                    }
                    if (selectedServiceTest.Enabled)
                    {
                        selectedServiceTest.DebugForTest = res.Result.DebugForTest;
                    }
                    selectedServiceTest.LastRunDate = DateTime.Now;
                    selectedServiceTest.LastRunDateVisibility = true;
                }
                else
                {
                    selectedServiceTest.TestPassed = false;
                    selectedServiceTest.TestFailing = false;
                    selectedServiceTest.TestInvalid = true;
                }
                selectedServiceTest.IsTestRunning = false;

            });
        }

        private ObservableCollection<IServiceTestOutput> CreateServiceTestOutputFromResult(ObservableCollection<IServiceTestOutput> stepStepOutputs, ServiceTestStep testStep)
        {
            var stepOutputs = new ObservableCollection<IServiceTestOutput>();
            foreach (var serviceTestOutput in stepStepOutputs)
            {
                var testOutput = new ServiceTestOutput(serviceTestOutput.Variable, serviceTestOutput.Value, serviceTestOutput.From, serviceTestOutput.To)
                {
                    AddStepOutputRow = testStep.AddNewOutput,
                    AssertOp = serviceTestOutput.AssertOp,
                    HasOptionsForValue = serviceTestOutput.HasOptionsForValue,
                    OptionsForValue = serviceTestOutput.OptionsForValue,
                    Result = serviceTestOutput.Result
                };

                stepOutputs.Add(testOutput);
            }
            return stepOutputs;
        }

        public void RunSelectedTestInBrowser(string runSelectedTestUrl, IExternalProcessExecutor processExecutor)
        {
            processExecutor?.OpenInBrowser(new Uri(runSelectedTestUrl));
        }

        public void RunAllTestsInBrowser(bool isDirty, string runAllUrl, IExternalProcessExecutor processExecutor)
        {
            if (isDirty)
            {
                ShowRunAllUnsavedError();
                return;
            }
            processExecutor?.OpenInBrowser(new Uri(runAllUrl));
        }
    }
}