/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Gates;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Data.TO;
using Dev2.Interfaces;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("ControlFlow-Gate", nameof(Gate), ToolType.Native, "8999E58B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Control Flow", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Flow_Gate")]
    public class GateActivity : DsfActivityAbstract<string>, IEquatable<GateActivity>, IDisposable
    {
        private string _gateFailureOption;
        private string _retryStrategy;
        IGateActivityWorker _worker;
        public GateActivity()
        {
            Construct(new GateActivityWorker(this));
        }
        public GateActivity(IGateActivityWorker worker)
        {
            Construct(worker);
        }

        private void Construct(IGateActivityWorker worker)
        {
            _worker = worker;
            DisplayName = nameof(Gate);
        }
        public string GateFailure
        {
            get => _gateFailureOption;
            set
            {
                _gateFailureOption = value;
            }
        }

        public string GateRetryStrategy
        {
            get => _retryStrategy;
            set
            {
                _retryStrategy = value;
            }
        }
        public override IList<DsfForEachItem> GetForEachInputs()
        {
            throw new NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            throw new NotImplementedException();
        }

        public override List<string> GetOutputs()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<StateVariable> GetState()
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            var allErrors = new ErrorResultTO();
            InitializeDebug(dataObject);


            try
            {
                _worker.AddValidationErrors(allErrors);
                if (!allErrors.HasErrors())
                {
                    if (dataObject.IsDebugMode())
                    {
                        ExecuteToolAddDebugItems(dataObject, update);
                    }
                    _worker.ExecuteGate(dataObject, update);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(nameof(Gate), e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            finally
            {
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError(nameof(GateActivity), allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        public static void ExecuteToolAddDebugItems(IDSFDataObject dataObject, int update)
        {



        }

        public bool Equals(GateActivity other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) && string.Equals(GateFailure, other.GateFailure) && string.Equals(GateRetryStrategy, other.GateRetryStrategy);
        }

        public override bool Equals(object obj)
        {
            if (obj is GateActivity act)
            {
                return Equals(act);
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397);

                return hashCode;
            }
        }

        public void Dispose()
        {
            if (_worker != null)
            {
                _worker.Dispose();
                _worker = null;
            }
        }
    }

    public interface IGateActivityWorker : IDisposable
    {
        IGate Gate { get; set; }
        void ExecuteGate(IDSFDataObject dataObject, int update);
        void AddValidationErrors(ErrorResultTO allErrors);
    }
    internal class GateActivityWorker : IGateActivityWorker
    {
        private GateActivity _activity;
        private IGate _gate;
        private readonly IGateFactory _gateFactory;

        [ExcludeFromCodeCoverage]
        public GateActivityWorker(GateActivity activity)
           : this(activity, new Gate(), new GateFactory())
        {
        }
        public GateActivityWorker(GateActivity activity, IGate gate)
           : this(activity, gate, new GateFactory())
        {
        }

        public GateActivityWorker(GateActivity activity, IGate gate, IGateFactory gateFactory)
        {
            _activity = activity;
            _gate = gate;
            _gateFactory = gateFactory;
        }

        public IGate Gate
        {
            get => _gate;
            set => _gate = value;
        }

        public void ExecuteGate(IDSFDataObject dataObject, int update)
        {
            var env = dataObject.Environment;
            Gate = _gateFactory.New(env);
           
            //TODO: This needs to be implemented
            //IStateNotifier stateNotifier = null;
            //stateNotifier = LogManager.CreateStateNotifier(dataObject);
            //dataObject.StateNotifier = stateNotifier;
            //stateNotifier?.LogPreExecuteState(_activity);

        }

        public void AddValidationErrors(ErrorResultTO allErrors)
        {
        }
        public void Dispose()
        {
            _activity = null;
        }
    }
}
