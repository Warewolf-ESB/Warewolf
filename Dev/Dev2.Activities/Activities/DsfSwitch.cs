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
using Warewolf.Storage;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnusedMember.Global

namespace Dev2.Activities
{
    public class DsfSwitch:DsfActivityAbstract<string>
    {
              public DsfFlowSwitchActivity Inner;

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

        public Dictionary<string,IDev2Activity> Switches { get; set; }
        public IEnumerable<IDev2Activity> Default { get; set; }

        public string Switch { get; set; }
        #region Overrides of DsfNativeActivity<string>

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
            var startTime = DateTime.Now;
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

    
            if(dataObject.IsDebugMode())
            {
                List<DebugItem> result = new List<DebugItem>();
                DebugItem itemToAdd = new DebugItem();
                var debugResult = new DebugItemWarewolfAtomResult(firstOrDefault, "", ds.SwitchVariable, "", "Switch on", "", "=");
                itemToAdd.AddRange(debugResult.GetDebugItemResult());
                result.Add(itemToAdd);
                _debugInputs = result;
                
                
                if(Inner != null)
                {
                    Inner.SetDebugInputs(_debugInputs);
                }
            }
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch 
                // ReSharper restore EmptyGeneralCatchClause
            {

            }
        }

        void DebugOutput(IDSFDataObject dataObject)
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
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {

            }
        }

        #endregion

        #region Overrides of DsfNativeActivity<string>

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            return _debugOutputs;
        }


        #endregion
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