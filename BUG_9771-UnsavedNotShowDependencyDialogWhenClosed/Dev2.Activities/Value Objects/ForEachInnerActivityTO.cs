using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract;

namespace Unlimited.Applications.BusinessDesignStudio.Activities.Value_Objects
{
    /// <summary>
    /// Used with the ForEach Activity
    /// </summary>
    public class ForEachInnerActivityTO
    {
        public IDev2ActivityIOMapping InnerActivity { get; private set; }
        public string OrigInnerOutputMapping { get; private set; }
        public string OrigInnerInputMapping { get; private set; }

        public IList<Tuple<string, string>> OrigCodedInputs { get; set; }
        public IList<Tuple<string, string>> OrigCodedOutputs { get; set;}

        public IList<Tuple<string, string>> CurCodedInputs { get; set; }
        public IList<Tuple<string, string>> CurCodedOutputs { get; set; }

        public ForEachInnerActivityTO(IDev2ActivityIOMapping act)
        {
            InnerActivity = act;

            if (InnerActivity != null)
            {
                OrigInnerInputMapping = act.InputMapping;
                OrigInnerOutputMapping = act.OutputMapping;
            }
        }

    }
}
