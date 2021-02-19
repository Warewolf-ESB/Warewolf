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
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Comparer;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Value_Objects;
using Warewolf.Auditing;
using Warewolf.Core;
using Warewolf.Driver.Persistence;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using WarewolfParserInterop;

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
                    Response = ErrorResource.ManualResumptionSuspensionIdBlank;
                    throw new Exception(ErrorResource.ManualResumptionSuspensionIdBlank);
                }

                if (!_persistenceEnabled)
                {
                    Response = ErrorResource.PersistenceSettingsNoConfigured;
                    throw new Exception(ErrorResource.PersistenceSettingsNoConfigured);
                }

                var overrideVariables = "";
                if (OverrideInputVariables)
                {
                    var suspendedEnv = _scheduler.GetSuspendedEnvironment(suspensionId);

                    if (string.IsNullOrEmpty(suspendedEnv))
                    {
                        throw new Exception(ErrorResource.ManualResumptionSuspensionEnvBlank);
                    }

                    if (suspendedEnv.StartsWith("Failed:"))
                    {
                        throw new Exception(suspendedEnv);
                    }

                    var innerActivity = InnerActivity();
                    overrideVariables = ExecuteOverrideDataFunc(innerActivity, suspendedEnv, _dataObject);
                }

                Response = _scheduler.ResumeJob(_dataObject, suspensionId, OverrideInputVariables, overrideVariables);
                _stateNotifier?.LogActivityExecuteState(this);

                if (_dataObject.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams("SuspensionID: " + suspensionId, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                    debugItemStaticDataParams = new DebugItemStaticDataParams("Override Variables: " + OverrideInputVariables, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                    debugItemStaticDataParams = new DebugItemStaticDataParams("Result: " + Response, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                }

                return new List<string> {Response};
            }
            catch (Exception ex)
            {
                Response = ex.Message;
                _stateNotifier?.LogExecuteException(ex, this);
                Dev2Logger.Error(nameof(ManualResumptionActivity), ex, GlobalConstants.WarewolfError);
                throw new Exception(ex.GetAllMessages());
            }
        }

        public override enFindMissingType GetFindMissingType() => enFindMissingType.ManualResumption;

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

        private ForEachInnerActivityTO InnerActivity()
        {
            if (!(OverrideDataFunc.Handler is IDev2ActivityIOMapping ioMapping))
            {
                throw new Exception(ErrorResource.InnerActivityWithNoContentError);
            }

            //TODO: Refactor/Rename ForEachInnerActivityTO to not be specific to "ForEach"
            var innerActivity = new ForEachInnerActivityTO(ioMapping);
            return innerActivity;
        }

        public string ExecuteOverrideDataFunc(ForEachInnerActivityTO innerActivity, string suspendedEnv, IDSFDataObject dataObject)
        {
            IDSFDataObject newDataObject = new DsfDataObject(string.Empty, Guid.Empty);

            var serviceInputs = new List<IServiceInput>();
            InputsFromJson.FromJson(suspendedEnv, serviceInputs);

            var origInnerInputMapping = innerActivity.OrigInnerInputMapping;
            var inputs = TranslateInputMappingToInputs(origInnerInputMapping);

            foreach (var serviceInput in inputs)
            {
                string inputName;
                string inputValue;
                IServiceInput storedInput = null;

                var isVariable = ExecutionEnvironment.IsValidVariableExpression(serviceInput.Value, out string errorMessage, 0);
                if (isVariable)
                {
                    inputName = dataObject.Environment.EvalToExpression(serviceInput.Value, _update);
                    inputValue = ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(inputName, _update, false, true));
                }
                else
                {
                    inputName = "[[" + serviceInput.Name + "]]";
                    inputValue = serviceInput.Value;
                }

                if (ExecutionEnvironment.IsScalar(serviceInput.Value))
                {
                    storedInput = serviceInputs.FirstOrDefault(o => o.Name == serviceInput.Name);
                }
                else if (ExecutionEnvironment.IsRecordsetIdentifier(serviceInput.Value))
                {
                    storedInput = serviceInputs.FirstOrDefault(o =>
                    {
                        var recName = o.Name;
                        if (o.Name.Contains("."))
                        {
                            recName = o.Name.Remove(0, o.Name.IndexOf(".", StringComparison.Ordinal) + 1);
                        }

                        return recName == serviceInput.Name;
                    });
                }
                else
                {
                    if (serviceInput.Name.StartsWith("@"))
                    {
                        storedInput = serviceInputs.FirstOrDefault(o => o.Name == serviceInput.Name);
                    }
                }

                if (storedInput != null && inputValue is null)
                {
                    inputValue = storedInput.Value;
                }

                newDataObject.Environment.AssignWithFrame(new AssignValue(inputName, inputValue), _update);
            }

            return newDataObject.Environment.ToJson();
        }

        private static IEnumerable<IServiceInput> TranslateInputMappingToInputs(string inputMapping)
        {
            var inputDefs = DataListFactory.CreateInputParser().Parse(inputMapping);
            return inputDefs.Select(inputDef => new ServiceInput(inputDef.Name, inputDef.RawValue)
            {
                EmptyIsNull = inputDef.EmptyToNull,
                RequiredField = inputDef.IsRequired
            }).Cast<IServiceInput>().ToList();
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
                hashCode = (hashCode * 397) ^ OverrideInputVariables.GetHashCode();
                hashCode = (hashCode * 397) ^ (Response != null ? Response.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OverrideDataFunc != null ? OverrideDataFunc.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}