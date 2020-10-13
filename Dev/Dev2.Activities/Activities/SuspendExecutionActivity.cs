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
using System.Diagnostics.CodeAnalysis;
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
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Auditing;
using Warewolf.Core;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("ControlFlow-SuspendExecution", "Suspend Execution", ToolType.Native, "8999E58B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Control Flow", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Flow_SuspendExecution")]
    public class SuspendExecutionActivity : DsfActivityAbstract<string>, IEquatable<SuspendExecutionActivity>
    {
        private IDSFDataObject _dataObject;
        private IStateNotifier _stateNotifier = null;
        public SuspendExecutionActivity()
        {
            DisplayName = "Suspend Execution";
            DataFunc = new ActivityFunc<string, bool>
            {
                DisplayName = "Data Action",
                Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}")

            };
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

        /// <summary>
        /// The property that holds the result string the user enters into the "Result" box
        /// </summary>
        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }
        public ActivityFunc<string, bool> DataFunc { get; set; }

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        [ExcludeFromCodeCoverage]
        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }
        [ExcludeFromCodeCoverage]
        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }
        [ExcludeFromCodeCoverage]
        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return new List<DsfForEachItem>();
        }
        [ExcludeFromCodeCoverage]
        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return new List<DsfForEachItem>();
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _dataObject = dataObject;
            _errorsTo = new ErrorResultTO();
            try
            {
                _stateNotifier?.LogActivityExecuteState(this);

                var suspensionId = _dataObject.ServiceName + "-SuspendExecution-" + Guid.NewGuid();

                if (_dataObject.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams("Execution Suspended: " + suspensionId, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                    debugItemStaticDataParams = new DebugItemStaticDataParams("Allow Manual Resumption: " + AllowManualResumption, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                }

                Result = suspensionId;
                Dev2Logger.Debug($"{_dataObject.ServiceName} execution suspended: Trigger {suspensionId} scheduled: {suspensionId}", GlobalConstants.WarewolfDebug);
                _dataObject.StopExecution = true;
            }
            catch (Exception ex)
            {
                _stateNotifier?.LogExecuteException(ex, this);
                Dev2Logger.Error(nameof(SuspendExecutionActivity), ex, GlobalConstants.WarewolfError);
                throw new Exception(ex.GetAllMessages());
            }
        }

        public override List<string> GetOutputs() => new List<string> { Result };

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
            equals &= Equals(Result, other.Result);
            equals &= activityFuncComparer.Equals(DataFunc, other.DataFunc);

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

            return Equals((SuspendExecutionActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)SuspendOption;
                hashCode = (hashCode * 397) ^ (PersistValue != null ? PersistValue.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AllowManualResumption != null ? AllowManualResumption.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DataFunc != null ? DataFunc.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}