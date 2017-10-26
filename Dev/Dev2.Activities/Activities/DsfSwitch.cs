using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage.Interfaces;
using Dev2.Comparer;
using Dev2.Common;
using System.Activities.Statements;

namespace Dev2.Activities
{
    public class DsfSwitch : DsfActivityAbstract<string>,IEquatable<DsfSwitch>, IAdapterActivity
    {
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable S2357 // Fields should be private
        public DsfFlowSwitchActivity Inner;
#pragma warning restore S2357 // Fields should be private
#pragma warning restore IDE1006 // Naming Styles

        public DsfSwitch(DsfFlowSwitchActivity inner)
      : base("Switch")
        {
            Inner = inner;
            UniqueID = inner.UniqueID;
        }

        public DsfSwitch() { }       

        public override List<string> GetOutputs()
        {
            return new List<string>();
        }

        public override FlowNode GetFlowNode()
        {
            var swt = new FlowSwitch<string>();
            return swt; 
        }

        public IFlowNodeActivity GetInnerNode()
        {
            return Inner;
        }

        public Dictionary<string, IDev2Activity> Switches { get; set; }
        public IEnumerable<IDev2Activity> Default { get; set; }

        public override string GetDisplayName()
        {
            return Switch;
        }

        public override IConflictTreeNode BuildNode()
        {
            var node = new ConflictTreeNode(this, new System.Windows.Point());
            foreach (var activity in Switches)
            {
                if (activity.Value is IDev2Activity act)
                {
                    node.AddChild(act.BuildNode(),activity.Key);
                }
            }
            if (Default != null)
            {
                node.AddChild(Default.FirstOrDefault().BuildNode(),"Default");
            }
            return node;
        }
        public override Dictionary<string,IDev2Activity> GetChildrenNodes()
        {
            var nextNodes = new Dictionary<string, IDev2Activity>();            
            foreach(var swt in Switches)
            {
                var currentAct = swt.Value;
                nextNodes.Add(swt.Key, currentAct);
            }
            if (Default != null)
            {
                nextNodes.Add(@"Default", Default.FirstOrDefault());
            }
            return nextNodes;
        }
        public string Switch { get; set; }

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return null;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return null;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Result { get; set; }


        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _debugOutputs.Clear();
            _debugInputs.Clear();
            NextNodes = new List<IDev2Activity>();
            try
            {
                Dev2Switch ds = new Dev2Switch { SwitchVariable = Switch };
                var firstOrDefault = dataObject.Environment.EvalAsListOfStrings(ds.SwitchVariable, update).FirstOrDefault();
                if (dataObject.IsDebugMode())
                {
                    InitializeDebug(dataObject);
                    Debug(dataObject, firstOrDefault, ds);
                    DispatchDebugState(dataObject, StateType.Before, update);
                }
                if (firstOrDefault != null)
                {
                    var a = firstOrDefault;
                    if (Switches.ContainsKey(a))
                    {
                        Result = a;
                        if (dataObject.IsDebugMode())
                        {
                            DebugOutput(dataObject);
                        }

                        NextNodes = new List<IDev2Activity> { Switches[a] };
                    }
                    else
                    {
                        if (Default != null)
                        {
                            Result = "Default";
                            var activity = Default.FirstOrDefault();
                            if (dataObject.IsDebugMode())
                            {
                                DebugOutput(dataObject);
                            }
                            NextNodes = new List<IDev2Activity> { activity };
                        }
                    }
                }
            }
            catch (Exception err)
            {
                dataObject.Environment.Errors.Add(err.Message);
            }
            finally
            {
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.After, update);
                    _debugOutputs = new List<DebugItem>();
                }
            }
        }

        void Debug(IDSFDataObject dataObject, string firstOrDefault, Dev2Switch ds)
        {
            try
            {
                if (dataObject.IsDebugMode())
                {
                    List<DebugItem> result = new List<DebugItem>();
                    DebugItem itemToAdd = new DebugItem();
                    var debugResult = new DebugItemWarewolfAtomResult(firstOrDefault, "", ds.SwitchVariable, "", "Switch on", "", "=");
                    itemToAdd.AddRange(debugResult.GetDebugItemResult());
                    result.Add(itemToAdd);
                    _debugInputs = result;

                    if (Inner != null)
                    {
                        Inner.SetDebugInputs(_debugInputs);
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Warn(e.Message, "Warewolf Warn");
            }
        }

        private void DebugOutput(IDSFDataObject dataObject)
        {
            try
            {
                if (dataObject.IsDebugMode())
                {
                    List<DebugItem> result = new List<DebugItem>();
                    var debugOutputBase = new DebugItemStaticDataParams(Result, "");
                    DebugItem itemToAdd = new DebugItem();
                    itemToAdd.AddRange(debugOutputBase.GetDebugItemResult());
                    result.Add(itemToAdd);
                    _debugOutputs = result;

                    if (Inner != null)
                    {
                        Inner.SetDebugOutputs(_debugOutputs);
                    }
                }
            }

            catch (Exception e)
            {
                Dev2Logger.Warn(e.Message, "Warewolf Warn");
            }
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            return _debugOutputs;
        }

        public bool Equals(DsfSwitch other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var switchesComparer = new SwitchesActivityComparer();
            var switchesAreEqual = switchesComparer.Equals(Switches, other.Switches);
            return base.Equals(other) 
                && switchesAreEqual
                && string.Equals(Switch, other.Switch) 
                && string.Equals(Result, other.Result);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DsfSwitch) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Inner != null ? Inner.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Switches != null ? Switches.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Default != null ? Default.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Switch != null ? Switch.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class TestMockSwitchStep : DsfActivityAbstract<string>
    {
        private readonly DsfSwitch _dsfSwitch;

        public TestMockSwitchStep(DsfSwitch dsfSwitch)
            : base(dsfSwitch.DisplayName)
        {
            _dsfSwitch = dsfSwitch;
            UniqueID = _dsfSwitch.UniqueID;
        }

        public override List<string> GetOutputs()
        {
            return new List<string>();
        }

        public string ConditionToUse { get; set; }

        #region Overrides of DsfNativeActivity<string>

        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return null;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return null;
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            var dsfSwitchSwitches = _dsfSwitch.Switches;
            bool hasResult = false;
            if (dataObject.IsDebugMode())
            {
                InitializeDebug(dataObject);
            }
            IDev2Activity activity = null;
            if (ConditionToUse == "Default")
            {
                if (_dsfSwitch.Default != null)
                {
                    activity = _dsfSwitch.Default.FirstOrDefault();
                    if (dataObject.IsDebugMode())
                    {
                        var debugItemStaticDataParams = new DebugItemStaticDataParams("Default", "", true);
                        AddDebugOutputItem(debugItemStaticDataParams);
                        AddDebugAssertResultItem(debugItemStaticDataParams);
                    }
                    hasResult = true;
                }
            }
            else
            {
                if (dsfSwitchSwitches.ContainsKey(ConditionToUse))
                {
                    activity = dsfSwitchSwitches[ConditionToUse];
                    if (dataObject.IsDebugMode())
                    {
                        var debugItemStaticDataParams = new DebugItemStaticDataParams(ConditionToUse, "", true);
                        AddDebugOutputItem(debugItemStaticDataParams);
                        AddDebugAssertResultItem(debugItemStaticDataParams);
                    }
                    hasResult = true;
                }
            }
            if (dataObject.IsDebugMode() && hasResult)
            {

                DispatchDebugState(dataObject, StateType.After, update);
                DispatchDebugState(dataObject, StateType.Duration, update);
            }

            if (!hasResult)
            {
                throw new ArgumentException($"No matching arm for Switch Mock. Mock Arm value '{ConditionToUse}'. Switch Arms: '{string.Join(",", dsfSwitchSwitches.Select(pair => pair.Key))}'.");
            }
            NextNodes = new List<IDev2Activity> { activity };
        }

        #endregion
    }
}