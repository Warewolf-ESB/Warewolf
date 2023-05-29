/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
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
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Auditing;
using Warewolf.Core;
using Warewolf.Driver.Persistence;
using Warewolf.Resource.Errors;
using Warewolf.Resource.Messages;
using Warewolf.Security.Encryption;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("ControlFlow-SuspendExecution", "Suspend Execution", ToolType.Native, "8999E58B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Control Flow", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Flow_SuspendExecution")]
    public class SuspendExecutionActivity : DsfBaseActivity, IEquatable<SuspendExecutionActivity>, IStateNotifierRequired
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

        [FindMissing] 
        public string Response { get; set; }

        public ActivityFunc<string, bool> SaveDataFunc { get; set; }

        public override IEnumerable<IDev2Activity> GetChildrenNodes()
        {
            var act = SaveDataFunc.Handler as IDev2ActivityIOMapping;
            if (act == null)
            {
                return new List<IDev2Activity>();
            }
            var nextNodes = new List<IDev2Activity> { act };
            return nextNodes;
        }

        protected override void OnExecute(NativeActivityContext context)
        {

        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            throw new Exception("this should not be reached");
        }

        private IExecutionEnvironment _originalExecutionEnvironment;
        public override IDev2Activity Execute(IDSFDataObject data, int update)
        {
            _previousParentId = data.ParentInstanceID;
            _debugInputs?.Clear();
            _debugOutputs?.Clear();
            _dataObject = data;
            _update = update;
            _originalExecutionEnvironment = data.Environment.Snapshot();

            _suspensionId = "";
            var allErrors = new ErrorResultTO();
            try
            {
                _dataObject.ForEachNestingLevel++;
                if (!_persistenceEnabled)
                {
                    throw new Exception(ErrorResource.PersistenceSettingsNoConfigured);
                }

                if (NextNodes is null)
                {
                    throw new Exception(ErrorResource.NextNodeRequiredForSuspendExecution);
                }

                var persistScheduleValue = PersistSchedulePersistValue();

                if (string.IsNullOrEmpty(persistScheduleValue))
                {
                    throw new Exception(string.Format(ErrorResource.SuspendOptionValueNotSet, GetSuspendValidationMessageType(SuspendOption)));
                }

                var currentEnvironment = _originalExecutionEnvironment.ToJson();
                var currentuserprincipal = _dataObject.ExecutingUser.Identity.Name;
                var versionNumber = _dataObject.VersionNumber.ToString();
                var resourceId = _dataObject.ResourceID;
                if (EncryptData)
                {
                    currentEnvironment = DpapiWrapper.Encrypt(currentEnvironment);
                    currentuserprincipal = DpapiWrapper.Encrypt(currentuserprincipal);
                }

                var firstActivity = NextNodes.First();
                var activityId = Guid.Parse(firstActivity?.UniqueID ??
                                            throw new Exception(GlobalConstants.NextNodeIDNotFound));
                var values = new Dictionary<string, StringBuilder>
                {
                    {"resourceID", new StringBuilder(resourceId.ToString())},
                    {"environment", new StringBuilder(currentEnvironment)},
                    {"startActivityId", new StringBuilder(activityId.ToString())},
                    {nameof(versionNumber), new StringBuilder(versionNumber)},
                    {nameof(currentuserprincipal), new StringBuilder(currentuserprincipal)}
                };

                if (_dataObject.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams("Allow Manual Resumption: " + AllowManualResumption, "", true);
                    AddDebugInputItem(debugItemStaticDataParams);
                }

                DispatchDebug(_dataObject, StateType.Before, _update);
                _suspensionId = _scheduler.CreateAndScheduleJob(SuspendOption, persistScheduleValue, values);

                _dataObject.ParentInstanceID = UniqueID;
                _dataObject.IsDebugNested = true;
                DispatchDebug(_dataObject, StateType.After, _update);

                Response = _suspensionId;
                _dataObject.Environment.Assign(Result, Response, 0);
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

            }
            catch (Hangfire.BackgroundJobClientException)
            {
                LogException(new Exception(ErrorResource.BackgroundJobClientCreateFailed), allErrors);
            }
            catch (Exception ex)
            {
                _stateNotifier?.LogExecuteException(new SerializableException(ex), this);
                LogException(ex, allErrors);
            }
            finally
            {
                var serviceTestStep = HandleServiceTestExecution(_dataObject);
                _dataObject.ParentInstanceID = _previousParentId;
                _dataObject.ForEachNestingLevel--;
                _dataObject.IsDebugNested = false;
                HandleDebug(_dataObject, serviceTestStep);
                HandleErrors(_dataObject, allErrors);
            }
            return  null; //fire once the rest should be done on resumption service
        }

        private static void LogException(Exception ex, ErrorResultTO allErrors)
        {
            Dev2Logger.Error(nameof(SuspendExecutionActivity), ex, GlobalConstants.WarewolfError);
            allErrors.AddError(ex.Message); 
        }

        public static string GetSuspendValidationMessageType(enSuspendOption suspendOption)
        {
            switch (suspendOption)
            {
                case enSuspendOption.SuspendUntil:
                    return "Date";
                case enSuspendOption.SuspendForSeconds:
                    return "Seconds";
                case enSuspendOption.SuspendForMinutes:
                    return "Minutes";
                case enSuspendOption.SuspendForHours:
                    return "Hours";
                case enSuspendOption.SuspendForDays:
                    return "Days";
                case enSuspendOption.SuspendForMonths:
                    return "Months";
                default:
                    return "";
            }
        }

        public override enFindMissingType GetFindMissingType() => enFindMissingType.SuspendExecution;

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
                if (allErrors.HasErrors())
                {
                    foreach (var fetchError in allErrors.FetchErrors())
                    {
                        dataObject.Environment.AddError(fetchError);
                    }
                    dataObject.ParentInstanceID = _previousParentId;
                    DisplayAndWriteError(dataObject,DisplayName, allErrors);
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

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if (updates != null && updates.Count == 1)
            {
                Response = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
            if (itemUpdate != null)
            {
                Result = itemUpdate.Item2;
            }
        }

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(Response);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(Result);

        public override List<string> GetOutputs() => new List<string> { Result };

        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            throw new NotImplementedException();
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
                new StateVariable
                {
                    Name = nameof(Result),
                    Value = Result,
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
            equals &= Equals(Result, other.Result);
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
                hashCode = (hashCode * 397) ^ AllowManualResumption.GetHashCode();
                hashCode = (hashCode * 397) ^ EncryptData.GetHashCode();
                hashCode = (hashCode * 397) ^ (Response != null ? Response.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SaveDataFunc != null ? SaveDataFunc.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}