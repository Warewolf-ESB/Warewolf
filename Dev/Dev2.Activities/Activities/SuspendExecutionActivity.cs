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
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Comparer;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Util;
using Warewolf.Auditing;
using Warewolf.Core;
using Warewolf.Driver.Persistence;

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

        public SuspendExecutionActivity()
            : this(Config.Persistence)
        {

        }

        public SuspendExecutionActivity(PersistenceSettings config)
        {
            DisplayName = "Suspend Execution";
            SaveDataFunc = new ActivityFunc<string, bool>
            {
                DisplayName = "Data Action",
                Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}"),
            };
            _persistenceEnabled = config.Enable;
        }

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
            _dataObject = dataObject;
            _update = update;
            base.ExecuteTool(_dataObject, update);
        }

        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            _errorsTo = new ErrorResultTO();
            _suspensionId = "";
            try
            {

                if (!_persistenceEnabled)
                {
                    throw new Exception(GlobalConstants.PersistenceSettingsNoConfigured);
                }

                if (NextNodes is null)
                {
                    throw new Exception(GlobalConstants.NextNodeRequiredForSuspendExecution);
                }

                var activityId = Guid.Parse(NextNodes.First()?.UniqueID ?? throw new Exception(GlobalConstants.NextNodeIDNotFound));
                var values = new Dictionary<string, StringBuilder>
                {
                    {"resourceID", new StringBuilder(_dataObject.ResourceID.ToString())},
                    {"environment", new StringBuilder(_dataObject.Environment.ToJson())},
                    {"startActivityId", new StringBuilder(activityId.ToString())},
                    {"versionNumber", new StringBuilder(_dataObject.VersionNumber.ToString())}
                };
                var persistScheduleValue = PersistSchedulePersistValue();
                var scheduler = new SuspendExecution();
                _suspensionId = scheduler.CreateAndScheduleJob(SuspendOption, persistScheduleValue, values);

                _stateNotifier?.LogActivityExecuteState(this);

                if (_dataObject.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams("Execution Suspended: " + _suspensionId, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                    debugItemStaticDataParams = new DebugItemStaticDataParams("Allow Manual Resumption: " + AllowManualResumption, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                }

                Response = _suspensionId;
                _dataObject.Environment.Assign(Result, _suspensionId, 0);
                _dataObject.Environment.CommitAssign();
                Dev2Logger.Debug($"{_dataObject.ServiceName} execution suspended: SuspensionId {_suspensionId} scheduled", GlobalConstants.WarewolfDebug);
                ExecuteSaveDataFunc();
                _dataObject.StopExecution = true;
                return new List<string> {_suspensionId};
            }
            catch (Exception ex)
            {
                _stateNotifier?.LogExecuteException(ex, this);
                Dev2Logger.Error(nameof(SuspendExecutionActivity), ex, GlobalConstants.WarewolfError);
                _dataObject.StopExecution = true;
                throw new Exception(ex.GetAllMessages());
            }
        }



        private void ExecuteSaveDataFunc()
        {
            if (SaveDataFunc.Handler is IDev2Activity act && AllowManualResumption)
            {
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
                hashCode = (hashCode * 397) ^ (Response != null ? Response.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SaveDataFunc != null ? SaveDataFunc.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}