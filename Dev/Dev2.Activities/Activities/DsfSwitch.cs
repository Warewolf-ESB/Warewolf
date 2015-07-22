using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Diagnostics;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities
{
    public class DsfSwitch:DsfActivityAbstract<string>
    {
              public DsfFlowSwitchActivity _inner;

              public DsfSwitch(DsfFlowSwitchActivity inner)
              {
                  _inner = inner;
              }

              public DsfSwitch() { }

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

          public override IDev2Activity Execute(IDSFDataObject dataObject)
          {
              _debugOutputs.Clear();
              _debugInputs.Clear();

              try
              {


                  Dev2Switch ds = new Dev2Switch { SwitchVariable = Switch };
                  var firstOrDefault = dataObject.Environment.EvalAsListOfStrings(ds.SwitchVariable).FirstOrDefault();


                  Debug(dataObject, firstOrDefault, ds);
                  if (firstOrDefault != null)
                  {
                      var a = firstOrDefault;
                      if (Switches.ContainsKey(a))
                      {
                          return Switches[a];
                      }
                      else
                      {
                          if (Default != null)
                          {
                              var activity = Default.FirstOrDefault();
                            return activity;
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

              }
              
          
              return null;
          }

        protected override void ExecuteTool(IDSFDataObject dataObject)
        {
           
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
                DispatchDebugState(dataObject, StateType.Before);
                DispatchDebugState(dataObject, StateType.After);
                if(_inner != null)
                {
                    _inner.SetDebugInputs(_debugInputs);
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
    }
}