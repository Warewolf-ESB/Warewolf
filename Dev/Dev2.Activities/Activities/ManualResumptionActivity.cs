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
using System.Text;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Comparer;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Util;
using Warewolf.Auditing;
using Warewolf.Core;
using Warewolf.Driver.Persistence;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("ControlFlow-ManualResumption", "Manual Resumption", ToolType.Native, "8999E58B-38A3-43BB-A98F-6090C5C9EA1F", "Dev2.Activities", "1.0.0.0", "Legacy", "Control Flow", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Flow_ManualResumption")]
    public class ManualResumptionActivity : DsfBaseActivity, IEquatable<ManualResumptionActivity>, IStateNotifierRequired
    {
        private IDSFDataObject _dataObject;
        private int _update;
        private IStateNotifier _stateNotifier = null;
        private readonly bool _persistenceEnabled;
        private readonly IPersistenceExecution _scheduler;

        public ManualResumptionActivity()
            : this(Config.Persistence, new PersistenceExecution())
        {
        }

        public ManualResumptionActivity(PersistenceSettings config, IPersistenceExecution resumeExecution)
        {
            DisplayName = "Manual Resumption";
            OverrideDataFunc = new ActivityFunc<string, bool>
            {
                DisplayName = "Data Action",
                Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}"),
            };
            _persistenceEnabled = config.Enable;
            _scheduler = resumeExecution;
        }

        /// <summary>
        /// The property that holds the result string the user enters into the "SuspensionId" box
        /// </summary>
        [FindMissing]
        public string SuspensionId { get; set; }

        [FindMissing] public string Response { get; set; }

        /// <summary>
        /// The property that holds the result bool the user selects from the "OverrideInputVariables" box
        /// </summary>
        [FindMissing]
        public bool OverrideInputVariables { get; set; }

        public ActivityFunc<string, bool> OverrideDataFunc { get; set; }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name = nameof(Response),
                    Value = Response,
                    Type = StateVariable.StateType.Output
                },
                new StateVariable
                {
                    Name = nameof(SuspensionId),
                    Value = SuspensionId,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(OverrideInputVariables),
                    Value = OverrideInputVariables.ToString(),
                    Type = StateVariable.StateType.Input
                }
            };
        }

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
            try
            {
                var suspensionId = EvalSuspensionId();
                if (string.IsNullOrWhiteSpace(suspensionId))
                {
                    Response = "failed";
                    throw new Exception(GlobalConstants.ManualResumptionSuspensionIdBlank);
                }

                if (!_persistenceEnabled)
                {
                    Response = "failed";
                    throw new Exception(GlobalConstants.PersistenceSettingsNoConfigured);
                }

                var overrideVariables = ExecuteOverrideDataFunc();
                Response = _scheduler.ResumeJob(suspensionId, OverrideInputVariables, overrideVariables);
                _stateNotifier?.LogActivityExecuteState(this);
                if (_dataObject.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams("SuspensionID: " + suspensionId, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                    debugItemStaticDataParams = new DebugItemStaticDataParams("Override Variables: " + OverrideInputVariables, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                    debugItemStaticDataParams = new DebugItemStaticDataParams("ResumptionID: " + Response, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                }

                return new List<string> {Response};
            }
            catch (Exception ex)
            {
                Response = "failed";
                _stateNotifier?.LogExecuteException(ex, this);
                Dev2Logger.Error(nameof(SuspendExecutionActivity), ex, GlobalConstants.WarewolfError);
                throw new Exception(ex.GetAllMessages());
            }
        }

        private string EvalSuspensionId()
        {
            var debugEvalResult = new DebugEvalResult(SuspensionId, nameof(SuspensionId), _dataObject.Environment, _update);
            AddDebugInputItem(debugEvalResult);
            var suspensionId = string.Empty;

            var debugItemResults = debugEvalResult.GetDebugItemResult();
            if (debugItemResults.Count > 0)
            {
                suspensionId = debugItemResults[0].Value;
            }

            return suspensionId;
        }

        private Dictionary<string, StringBuilder> ExecuteOverrideDataFunc()
        {
            if (OverrideDataFunc.Handler is IDev2Activity act && OverrideInputVariables)
            {
                //TODO: Return new variables
                return new Dictionary<string, StringBuilder>();
            }

            return new Dictionary<string, StringBuilder>();
        }

        public void SetStateNotifier(IStateNotifier stateNotifier)
        {
            if (_stateNotifier is null)
            {
                _stateNotifier = stateNotifier;
            }
        }

        public bool Equals(ManualResumptionActivity other)
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
            equals &= Equals(SuspensionId, other.SuspensionId);
            equals &= Equals(OverrideInputVariables, other.OverrideInputVariables);
            equals &= Equals(Response, other.Response);
            equals &= activityFuncComparer.Equals(OverrideDataFunc, other.OverrideDataFunc);
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

            return Equals((ManualResumptionActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (SuspensionId != null ? SuspensionId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OverrideInputVariables != null ? OverrideInputVariables.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Response != null ? Response.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OverrideDataFunc != null ? OverrideDataFunc.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}