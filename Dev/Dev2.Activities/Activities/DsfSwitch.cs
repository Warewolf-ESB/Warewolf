using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Diagnostics;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities
{
    public class DsfSwitch:DsfActivityAbstract<string>
    {

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


        protected override void ExecuteTool(IDSFDataObject dataObject)
        {

            Dev2Switch ds = new Dev2Switch { SwitchVariable = Switch };
            DebugItem itemToAdd = new DebugItem();
            var firstOrDefault = dataObject.Environment.EvalAsListOfStrings(ds.SwitchVariable).FirstOrDefault();
            if(firstOrDefault != null)
            {
                var a = firstOrDefault;
                if( Switches.ContainsKey(a))
                {
                    Switches[a].Execute(dataObject);
                }
                else
                {
                    var activity = Default.FirstOrDefault();
                    if(activity != null)
                    {
                        activity.Execute(dataObject);
                    }
                }
            }
        }


        #endregion
    }
}