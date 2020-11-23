/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Comparer;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Dev2.Util;
using Warewolf.Auditing;
using Warewolf.Core;
using Warewolf.Driver.Persistence;
using Warewolf.Resource.Errors;
using Warewolf.Resource.Messages;
using Warewolf.Security.Encryption;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("ControlFlow-SuspendExecution", "Suspend Execution", ToolType.Native, "8999E58B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Control Flow", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Flow_SuspendExecution")]
    public class SuspendExecutionActivity : DsfBaseActivity, IEquatable<SuspendExecutionActivity>,
        IStateNotifierRequired
    {
        private IDSFDataObject _dataObject;
        private IStateNotifier _stateNotifier = null;
        private int _update;
        private string _suspensionId = "";
        private readonly bool _persistenceEnabled;
        private readonly IPersistenceExecution _scheduler;

        public SuspendExecutionActivity()
            : this(Config.Persistence, new PersistenceExecution())
        {
        }

        public SuspendExecutionActivity(PersistenceSettings config, IPersistenceExecution suspendExecution)
        {
            DisplayName = "Suspend Execution";
            SaveDataFunc = new ActivityFunc<string, bool>
            {
                DisplayName = "Data Action",
                Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}"),
            };
            _persistenceEnabled = config.Enable;
            _scheduler = suspendExecution;
        }

        string _childUniqueId;
        Guid _originalUniqueID;
        private string _previousParentId;

        public enSuspendOption SuspendOption { get; set; }

        /// <summary>
        /// The property that holds the result string the user enters into the "PersistValue" box
        /// </summary>
        [FindMissing]
        public string PersistValue { get; set; }

        /// <summary>
        /// The property that holds the result bool the user selects from the "AllowManualResumption" box
        /// </summary>
        [FindMissing]
        public bool AllowManualResumption { get; set; }

        /// <summary>
        /// The property that holds the result bool the user selects from the "EncryptData" box
        /// </summary>
        [FindMissing]
        public bool EncryptData { get; set; }

        [FindMissing] public string Response { get; set; }

        public ActivityFunc<string, bool> SaveDataFunc { get; set; }

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            _dataObject = dataObject;
            ExecuteTool(_dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _previousParentId = dataObject.ParentInstanceID;
            _dataObject = dataObject;
            _update = update;
            base.ExecuteTool(_dataObject, update);
        }

        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            _errorsTo = new ErrorResultTO();
            _suspensionId = "";
            var allErrors = new ErrorResultTO();
            var dataObject = _dataObject;
            try
            {
                dataObject.ForEachNestingLevel++;
                if (!_persistenceEnabled)
                {
                    throw new Exception(ErrorResource.PersistenceSettingsNoConfigured);
                }

                if (NextNodes is null)
                {
                    throw new Exception(ErrorResource.NextNodeRequiredForSuspendExecution);
                }

                var activityId = Guid.Parse(NextNodes.First()?.UniqueID ??
                                            throw new Exception(GlobalConstants.NextNodeIDNotFound));
                var currentEnvironment = _dataObject.Environment.ToJson();
                if (EncryptData)
                {
                    currentEnvironment = DpapiWrapper.Encrypt(currentEnvironment);
                }

                var values = new Dictionary<string, StringBuilder>
                {
                    {"resourceID", new StringBuilder(_dataObject.ResourceID.ToString())},
                    {"environment", new StringBuilder(currentEnvironment)},
                    {"startActivityId", new StringBuilder(activityId.ToString())},
                    {"versionNumber", new StringBuilder(_dataObject.VersionNumber.ToString())}
                };
                var persistScheduleValue = PersistSchedulePersistValue();
                if (_dataObject.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams("Allow Manual Resumption: " + AllowManualResumption, "", true);
                    AddDebugInputItem(debugItemStaticDataParams);
                }

                DispatchDebug(dataObject, StateType.Before, _update);
                _suspensionId = _scheduler.CreateAndScheduleJob(SuspendOption, persistScheduleValue, values);

                dataObject.ParentInstanceID = UniqueID;
                dataObject.IsDebugNested = true;
                DispatchDebug(dataObject, StateType.After, _update);

                Response = _suspensionId;
                _dataObject.Environment.Assign(Result, _suspensionId, 0);
                _dataObject.Environment.CommitAssign();
                _stateNotifier?.LogActivityExecuteState(this);
                Dev2Logger.Debug($"{_dataObject.ServiceName} execution suspended: SuspensionId {_suspensionId} scheduled", GlobalConstants.WarewolfDebug);
                if (AllowManualResumption)
                {
                    ExecuteSaveDataFunc();
                }

                if (_dataObject.IsServiceTestExecution && _originalUniqueID == Guid.Empty)
                {
                    _originalUniqueID = Guid.Parse(UniqueID);
                }

                _dataObject.StopExecution = true;
                return new List<string> {_suspensionId};
            }
            catch (Exception ex)
            {
                _stateNotifier?.LogExecuteException(ex, this);
                Dev2Logger.Error(nameof(SuspendExecutionActivity), ex, GlobalConstants.WarewolfError);
                _dataObject.StopExecution = true;
                allErrors.AddError(ex.GetAllMessages());
                throw;
            }
            finally
            {
                var serviceTestStep = HandleServiceTestExecution(dataObject);
                dataObject.ParentInstanceID = _previousParentId;
                dataObject.ForEachNestingLevel--;
                dataObject.IsDebugNested = false;
                HandleDebug(dataObject, serviceTestStep);
                HandleErrors(dataObject, allErrors);
            }
        }

        private void HandleDebug(IDSFDataObject dataObject, IServiceTestStep serviceTestStep)
        {
            if (dataObject.IsDebugMode())
            {
                if (dataObject.IsServiceTestExecution && serviceTestStep != null)
                {
                    var debugItems = TestDebugMessageRepo.Instance.GetDebugItems(dataObject.ResourceID, dataObject.TestName);
                    debugItems = debugItems.Where(state => state.WorkSurfaceMappingId == serviceTestStep.ActivityID).ToList();
                    var debugStates = debugItems.LastOrDefault();

                    var debugItemStaticDataParams = new DebugItemServiceTestStaticDataParams(serviceTestStep.Result.Message, serviceTestStep.Result.RunTestResult == RunResult.TestFailed);
                    var itemToAdd = new DebugItem();
                    itemToAdd.AddRange(debugItemStaticDataParams.GetDebugItemResult());
                    debugStates?.AssertResultList?.Add(itemToAdd);
                }
                DispatchDebugState(dataObject, StateType.Duration, 0);
            }
        }

        private void HandleErrors(IDSFDataObject dataObject, IErrorResultTO allErrors)
        {
            if (allErrors.HasErrors())
            {
                dataObject.ParentInstanceID = _previousParentId;
                dataObject.ForEachNestingLevel--;
                dataObject.IsDebugNested = false;
                // Handle Errors
                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError(nameof(SuspendExecutionActivity), allErrors);
                    foreach (var fetchError in allErrors.FetchErrors())
                    {
                        dataObject.Environment.AddError(fetchError);
                    }

                    dataObject.ParentInstanceID = _previousParentId;
                }
            }
        }

        private void DispatchDebug(IDSFDataObject dataObject, StateType stateType, int update)
        {
            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, stateType, update);
            }
        }

        private void ExecuteSaveDataFunc()
        {
            if (SaveDataFunc.Handler is IDev2Activity act)
            {
                _childUniqueId = act.UniqueID;
                act.Execute(_dataObject, 0);
                Dev2Logger.Debug("Save SuspensionId: Execute - " + act.GetDisplayName(), _dataObject.ExecutionID.ToString());
            }
        }

        private string PersistSchedulePersistValue()
        {
            var debugEvalResult = new DebugEvalResult(PersistValue, "Persist Schedule Value", _dataObject.Environment, _update);
            AddDebugInputItem(debugEvalResult);
            var persistValue = string.Empty;
            var debugItemResults = debugEvalResult.GetDebugItemResult();
            if (debugItemResults.Count > 0)
            {
                persistValue = debugItemResults[0].Value;
            }

            return persistValue;
        }

        private IServiceTestStep HandleServiceTestExecution(IDSFDataObject dataObject)
        {
            var serviceTestStep = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.ActivityID == _originalUniqueID);
            if (dataObject.IsServiceTestExecution)
            {
                var serviceTestSteps = serviceTestStep?.Children;
                UpdateDebugStateWithAssertions(dataObject, serviceTestSteps?.ToList());
                if (serviceTestStep != null)
                {
                    var testRunResult = new TestRunResult();
                    GetFinalTestRunResult(serviceTestStep, testRunResult);
                    serviceTestStep.Result = testRunResult;
                }
            }

            return serviceTestStep;
        }

        void UpdateDebugStateWithAssertions(IDSFDataObject dataObject, List<IServiceTestStep> serviceTestTestSteps)
        {
            ServiceTestHelper.UpdateDebugStateWithAssertions(dataObject, serviceTestTestSteps, _childUniqueId);
        }

        private static void GetFinalTestRunResult(IServiceTestStep serviceTestStep, TestRunResult testRunResult)
        {
            var nonPassingSteps = serviceTestStep.Children?.Where(step => step.Type != StepType.Mock && step.Result?.RunTestResult != RunResult.TestPassed).ToList();
            if (nonPassingSteps != null && nonPassingSteps.Count == 0)
            {
                testRunResult.Message = Messages.Test_PassedResult;
                testRunResult.RunTestResult = RunResult.TestPassed;
            }
            else
            {
                if (nonPassingSteps != null)
                {
                    var failMessage = string.Join(Environment.NewLine, nonPassingSteps.Select(step => step.Result.Message));
                    testRunResult.Message = failMessage;
                }
                testRunResult.RunTestResult = RunResult.TestFailed;
            }
        }

        public void SetStateNotifier(IStateNotifier stateNotifier)
        {
            if (_stateNotifier is null)
            {
                _stateNotifier = stateNotifier;
            }
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name = nameof(SuspendOption),
                    Value = SuspendOption.ToString(),
                    Type = StateVariable.StateType.Input,
                },
                new StateVariable
                {
                    Name = nameof(PersistValue),
                    Value = PersistValue,
                    Type = StateVariable.StateType.Input,
                },
                new StateVariable
                {
                    Name = nameof(AllowManualResumption),
                    Value = AllowManualResumption.ToString(),
                    Type = StateVariable.StateType.Input,
                },
                new StateVariable
                {
                    Name = nameof(EncryptData),
                    Value = EncryptData.ToString(),
                    Type = StateVariable.StateType.Input,
                },
                new StateVariable
                {
                    Name = nameof(Response),
                    Value = Response,
                    Type = StateVariable.StateType.Output
                },
            };
        }

        public bool Equals(SuspendExecutionActivity other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var activityFuncComparer = new ActivityFuncComparer();
            var equals = base.Equals(other);
            equals &= Equals(SuspendOption, other.SuspendOption);
            equals &= string.Equals(PersistValue, other.PersistValue);
            equals &= Equals(AllowManualResumption, other.AllowManualResumption);
            equals &= Equals(EncryptData, other.EncryptData);
            equals &= Equals(Response, other.Response);
            equals &= activityFuncComparer.Equals(SaveDataFunc, other.SaveDataFunc);

            return equals;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((SuspendExecutionActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) SuspendOption;
                hashCode = (hashCode * 397) ^ (PersistValue != null ? PersistValue.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AllowManualResumption != null ? AllowManualResumption.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (EncryptData != null ? EncryptData.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Response != null ? Response.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SaveDataFunc != null ? SaveDataFunc.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}