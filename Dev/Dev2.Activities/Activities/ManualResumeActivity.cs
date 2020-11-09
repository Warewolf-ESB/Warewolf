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
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Util;
using Warewolf.Auditing;
using Warewolf.Core;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("ControlFlow-ManualResume", "Manual Resume", ToolType.Native, "8999E58B-38A3-43BB-A98F-6090C5C9EA1F", "Dev2.Activities", "1.0.0.0", "Legacy", "Control Flow", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Flow_ManualResume")]
    public class ManualResumeActivity : DsfBaseActivity, IEquatable<ManualResumeActivity>, IStateNotifierRequired
    {
        private IDSFDataObject _dataObject;
        private int _update;
        private IStateNotifier _stateNotifier = null;
        private string _suspensionId = "";

        public ManualResumeActivity()
            : this(Config.Persistence)
        {

        }

        public ManualResumeActivity(PersistenceSettings config)
        {
            DisplayName = "Manual Resume";
        }
        [FindMissing] public string Response { get; set; }

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
                _stateNotifier?.LogActivityExecuteState(this);
                if (_dataObject.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams("Manually Resume: " + _suspensionId, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                }
                Response = "success";
                return new List<string> {_suspensionId};
            }
            catch (Exception ex)
            {
                Response = "failed";
                _stateNotifier?.LogExecuteException(ex, this);
                Dev2Logger.Error(nameof(SuspendExecutionActivity), ex, GlobalConstants.WarewolfError);
                throw new Exception(ex.GetAllMessages());
            }
        }
        public void SetStateNotifier(IStateNotifier stateNotifier)
        {
            if (_stateNotifier is null)
            {
                _stateNotifier = stateNotifier;
            }
        }
          public bool Equals(ManualResumeActivity other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            var equals = base.Equals(other);
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

            return Equals((ManualResumeActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                return hashCode;
            }
        }
    }
}