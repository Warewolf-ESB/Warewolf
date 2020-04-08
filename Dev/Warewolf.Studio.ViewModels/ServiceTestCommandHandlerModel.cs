#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
            testModel.Item = testModel.Clone().As<ServiceTestModel>();
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
            var model = selectedServiceTest.As<ServiceTestModel>();
            if (model == null || resourceModel == null || asyncWorker == null ||
                model.IsNewTest)
            {
                return;
            }
            model.IsTestLoading = true;
            model.IsTestRunning = true;

            asyncWorker.Start(() => BackgroundAction(model, resourceModel), res => UiAction(model, resourceModel, res));
        }

        private static IServiceTestModelTO BackgroundAction(IServiceTestModel selectedServiceTest, IContextualResourceModel resourceModel)
        {
            return resourceModel.Environment.ResourceRepository.ExecuteTest(resourceModel, selectedServiceTest.TestName);
        }

        private void UiAction(IServiceTestModel selectedServiceTest, IContextualResourceModel resourceModel, IServiceTestModelTO res)
        {
            if (res == null)
            {
                ShowResourceDeleted(selectedServiceTest, resourceModel);
                return;
            }
            if (res?.Result != null)
            {
                if (res.Result.RunTestResult == RunResult.TestResourceDeleted)
                {
                    ShowResourceDeleted(selectedServiceTest, resourceModel);
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
                    RunTestStepForeachTestStep(selectedServiceTest, res);
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
        }

        private static void ShowResourceDeleted(IServiceTestModel selectedServiceTest, IContextualResourceModel resourceModel)
        {
            selectedServiceTest.IsTestRunning = false;
            ShowPopupController();
            CloseResourceTestView(resourceModel);
        }

        private void RunTestStepForeachTestStep(IServiceTestModel selectedServiceTest, IServiceTestModelTO res)
        {
            foreach (var resTestStep in res.TestSteps)
            {
                RunTestStep(selectedServiceTest, resTestStep);
            }
        }

        private static void CloseResourceTestView(IContextualResourceModel resourceModel)
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            shellViewModel.CloseResourceTestView(resourceModel.ID, resourceModel.ServerID, resourceModel.Environment.EnvironmentID);
        }

        private static void ShowPopupController()
        {
            var popupController = CustomContainer.Get<IPopupController>();
            popupController?.Show(Resources.Languages.Core.ServiceTestResourceDeletedMessage, Resources.Languages.Core.ServiceTestResourceDeletedHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false);
        }

        private void RunTestStep(IServiceTestModel selectedServiceTest, IServiceTestStep resTestStep)
        {
            var serviceTestSteps = selectedServiceTest.TestSteps.Where(testStep => testStep.ActivityID == resTestStep.ActivityID).ToList();
            foreach (var serviceTestStep in serviceTestSteps)
            {
                var resServiceTestStep = serviceTestStep.As<ServiceTestStep>();
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
                var childServiceTestSteps = serviceTestStepChildren.Where(testStep => testStep.ActivityID == child.ActivityID).ToList();
                foreach (var childServiceTestStep in childServiceTestSteps)
                {
                    childServiceTestStep.Result = child.Result;

                    childServiceTestStep.StepOutputs = CreateServiceTestOutputFromResult(child.StepOutputs, childServiceTestStep.As<ServiceTestStep>());
                    var children1 = child.Children;
                    if (children1.Count > 0)
                    {
                        SetChildrenTestResult(children1, childServiceTestStep.Children);
                    }
                }
            }
        }

        static ObservableCollection<IServiceTestOutput> CreateServiceTestOutputFromResult(ObservableCollection<IServiceTestOutput> stepStepOutputs, ServiceTestStep testStep)
        {
            var stepOutputs = new ObservableCollection<IServiceTestOutput>();
            foreach (var serviceTestOutput in stepStepOutputs)
            {
                var testOutput = new ServiceTestOutput();

                if (serviceTestOutput != null)
                {
                    testOutput = GetTestOutput(testStep, serviceTestOutput);
                }

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

        private static ServiceTestOutput GetTestOutput(ServiceTestStep testStep, IServiceTestOutput serviceTestOutput)
        {
            var variable = serviceTestOutput.Variable ?? "";
            var value = serviceTestOutput.Value ?? "";
            var to = serviceTestOutput.To ?? "";
            var from = serviceTestOutput.From ?? "";

            var testOutput = new ServiceTestOutput(variable, value, from, to)
            {
                AddStepOutputRow = testStep.AddNewOutput,
                AssertOp = serviceTestOutput.AssertOp ?? "=",
                HasOptionsForValue = serviceTestOutput.HasOptionsForValue,
                OptionsForValue = serviceTestOutput.OptionsForValue ?? new List<string>(),
                Result = serviceTestOutput.Result ?? new TestRunResult { RunTestResult = RunResult.TestPending }
            };
            return testOutput;
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

        public void RunAllTestCoverageInBrowser(bool isDirty, string runAllTestUrl, IExternalProcessExecutor processExecutor)
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
