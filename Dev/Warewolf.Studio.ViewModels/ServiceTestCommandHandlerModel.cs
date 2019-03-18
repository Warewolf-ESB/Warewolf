#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using Dev2.Studio.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public sealed class ServiceTestCommandHandlerModel : IServiceTestCommandHandler
    {
        readonly DataListConversionUtils _dataListConversionUtils;

        public ServiceTestCommandHandlerModel()
        {
            DataList = new DataListModel();
            _dataListConversionUtils = new DataListConversionUtils();
        }

        DataListModel DataList { get; set; }

        public IServiceTestModel CreateTest(IResourceModel resourceModel, int testNumber) => CreateTest(resourceModel, testNumber, false);

        public IServiceTestModel CreateTest(IResourceModel resourceModel, int testNumber, bool isFromDebug)
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
                if (!isFromDebug)
                {
                    testModel.Outputs = outputList.Select(sca =>
                    {
                        var serviceTestOutput = new ServiceTestOutput(sca.DisplayValue, "", "", "");
                        serviceTestOutput.AddNewAction = () => testModel.AddRow(serviceTestOutput, DataList);
                        return (IServiceTestOutput)serviceTestOutput;
                    }).ToObservableCollection();
                }
            }
            testModel.Item = (ServiceTestModel)testModel.Clone();
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

        static void ShowRunAllUnsavedError()
        {
            var popupController = CustomContainer.Get<IPopupController>();
            popupController?.Show(Resources.Languages.Core.ServiceTestRunAllUnsavedTestsMessage,
                Resources.Languages.Core.ServiceTestRunAllUnsavedTestsHeader, MessageBoxButton.OK, MessageBoxImage.Error, null,
                false, true, false, false, false, false);
        }

        public IServiceTestModel DuplicateTest(IServiceTestModel selectedTests, int testNumber)
        {
            var clone = selectedTests.Clone();
            clone.TestName = selectedTests.TestName + " " + (testNumber == 0 ? 1 : testNumber);
            clone.OldTestName = clone.TestName;
            clone.Enabled = true;
            clone.IsTestSelected = true;
            clone.TestPending = true;
            clone.NewTest = true;

            return clone;
        }

        public void RunSelectedTest(IServiceTestModel selectedServiceTest, IContextualResourceModel resourceModel, IAsyncWorker asyncWorker)
        {
            selectedServiceTest = selectedServiceTest as ServiceTestModel;
            if (selectedServiceTest == null || resourceModel == null || asyncWorker == null ||
                selectedServiceTest.IsNewTest)
            {
                return;
            }
            selectedServiceTest.IsTestLoading = true;
            selectedServiceTest.IsTestRunning = true;
            asyncWorker.Start(() => resourceModel.Environment.ResourceRepository.ExecuteTest(resourceModel, selectedServiceTest.TestName), res =>
                {
                    if (res?.Result != null)
                    {
                        if (res.Result.RunTestResult == RunResult.TestResourceDeleted)
                        {
                            selectedServiceTest.IsTestRunning = false;
                            var popupController = CustomContainer.Get<IPopupController>();
                            popupController?.Show(Resources.Languages.Core.ServiceTestResourceDeletedMessage, Resources.Languages.Core.ServiceTestResourceDeletedHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false);
                            var shellViewModel = CustomContainer.Get<IShellViewModel>();
                            shellViewModel.CloseResourceTestView(resourceModel.ID, resourceModel.ServerID, resourceModel.Environment.EnvironmentID);
                            return;
                        }

                        UpdateTestStatus(selectedServiceTest, res);

                        selectedServiceTest.Outputs = res.Outputs?.Select(output =>
                        {
                            var serviceTestOutput = new ServiceTestOutput(output.Variable, output.Value, output.From, output.To) as IServiceTestOutput;
                            serviceTestOutput.AssertOp = output.AssertOp;
                            serviceTestOutput.Result = output.Result;
                            return serviceTestOutput;
                        }).ToObservableCollection();

                        if (selectedServiceTest.TestSteps != null && res.TestSteps != null)
                        {
                            foreach (var resTestStep in res.TestSteps)
                            {
                                RunTestStep(selectedServiceTest, resTestStep);
                            }
                        }

                        selectedServiceTest.DebugForTest = res.Result.DebugForTest;
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
                    selectedServiceTest.IsTestLoading = false;
                });
        }

        private void RunTestStep(IServiceTestModel selectedServiceTest, IServiceTestStep resTestStep)
        {
            var serviceTestSteps = selectedServiceTest.TestSteps.Where(testStep => testStep.UniqueId == resTestStep.UniqueId).ToList();
            foreach (var serviceTestStep in serviceTestSteps)
            {
                var resServiceTestStep = serviceTestStep as ServiceTestStep;
                if (resServiceTestStep == null)
                {
                    continue;
                }

                UpdateTestStepResult(resServiceTestStep, resTestStep);

                var serviceTestOutputs = resTestStep.StepOutputs;
                if (serviceTestOutputs.Count > 0)
                {
                    resServiceTestStep.StepOutputs = CreateServiceTestOutputFromResult(resTestStep.StepOutputs, resServiceTestStep);
                }
                var children = resTestStep.Children;
                if (children.Count > 0)
                {
                    SetChildrenTestResult(children, resServiceTestStep.Children);
                }
            }
        }

        static void UpdateTestStepResult(ServiceTestStep resServiceTestStep, IServiceTestStep resTestStep)
        {
            resServiceTestStep.Result = resTestStep.Result;

            if (resServiceTestStep.MockSelected)
            {
                resServiceTestStep.TestPending = false;
                resServiceTestStep.TestPassed = false;
                resServiceTestStep.TestFailing = false;
                resServiceTestStep.TestInvalid = false;
            }
        }

        static void UpdateTestStatus(IServiceTestModel selectedServiceTest, IServiceTestModelTO res)
        {
            selectedServiceTest.TestFailing = res.Result.RunTestResult == RunResult.TestFailed;
            selectedServiceTest.TestPassed = res.Result.RunTestResult == RunResult.TestPassed;
            selectedServiceTest.TestInvalid = res.Result.RunTestResult == RunResult.TestInvalid ||
                                              res.Result.RunTestResult == RunResult.TestResourceDeleted;

            var testPending = res.Result.RunTestResult != RunResult.TestFailed;
            testPending &= res.Result.RunTestResult != RunResult.TestPassed;
            testPending &= res.Result.RunTestResult != RunResult.TestInvalid;
            testPending &= res.Result.RunTestResult != RunResult.TestResourceDeleted;
            testPending &= res.Result.RunTestResult != RunResult.TestResourcePathUpdated;

            selectedServiceTest.TestPending = testPending;
        }

        void SetChildrenTestResult(ObservableCollection<IServiceTestStep> resTestStepchildren, ObservableCollection<IServiceTestStep> serviceTestStepChildren)
        {
            foreach (var child in resTestStepchildren)
            {
                var childServiceTestSteps = serviceTestStepChildren.Where(testStep => testStep.UniqueId == child.UniqueId).ToList();
                foreach (var childServiceTestStep in childServiceTestSteps)
                {
                    childServiceTestStep.Result = child.Result;

                    childServiceTestStep.StepOutputs = CreateServiceTestOutputFromResult(child.StepOutputs, childServiceTestStep as ServiceTestStep);
                    var children1 = child.Children;
                    if (children1.Count > 0)
                    {
                        SetChildrenTestResult(children1, childServiceTestStep.Children);
                    }
                }
            }
        }

        ObservableCollection<IServiceTestOutput> CreateServiceTestOutputFromResult(ObservableCollection<IServiceTestOutput> stepStepOutputs, ServiceTestStep testStep)
        {
            var stepOutputs = new ObservableCollection<IServiceTestOutput>();
            foreach (var serviceTestOutput in stepStepOutputs)
            {
                var variable = serviceTestOutput?.Variable ?? "";
                var value = serviceTestOutput?.Value ?? "";
                var to = serviceTestOutput?.To ?? "";
                var from = serviceTestOutput?.From ?? "";

                var testOutput = new ServiceTestOutput(variable, value, from, to)
                {
                    AddStepOutputRow = testStep.AddNewOutput,
                    AssertOp = serviceTestOutput?.AssertOp ?? "=",
                    HasOptionsForValue = serviceTestOutput?.HasOptionsForValue ?? false,
                    OptionsForValue = serviceTestOutput?.OptionsForValue ?? new List<string>(),
                    Result = serviceTestOutput?.Result ?? new TestRunResult { RunTestResult = RunResult.TestPending }
                };

                if (testStep.MockSelected)
                {
                    if (!string.IsNullOrEmpty(testOutput.Variable))
                    {
                        testOutput.TestPassed = true;
                    }
                    else
                    {
                        testOutput.TestPending = false;
                        testOutput.TestPassed = false;
                        testOutput.TestFailing = false;
                        testOutput.TestInvalid = true;
                    }
                }

                stepOutputs.Add(testOutput);
            }
            return stepOutputs;
        }

        public void RunSelectedTestInBrowser(string runSelectedTestUrl, IExternalProcessExecutor processExecutor)
        {
            processExecutor?.OpenInBrowser(new Uri(runSelectedTestUrl));
        }

        public void RunAllTestsInBrowser(bool isDirty, string runAllTestUrl, IExternalProcessExecutor processExecutor)
        {
            if (isDirty)
            {
                ShowRunAllUnsavedError();
                return;
            }
            processExecutor?.OpenInBrowser(new Uri(runAllTestUrl));
        }
    }
}