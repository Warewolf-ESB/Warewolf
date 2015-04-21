using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2.Data.SystemTemplates.Models;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

namespace Dev2.Activities
{
    public class DsfDecision:DsfActivityAbstract<string>
    {

       public IEnumerable<IDev2Activity> TrueArm { get; set; }
       public IEnumerable<IDev2Activity> FalseArm { get; set; }
       public Dev2DecisionStack Conditions { get; set; }

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

        private  Dev2Decision parseDecision(IExecutionEnvironment env , Dev2Decision decision)
        {
            var col1 = WarewolfDataEvaluationCommon.EvalResultToString(env.Eval(decision.Col1));
            var col2 = WarewolfDataEvaluationCommon.EvalResultToString(env.Eval(decision.Col2));
            var col3 = WarewolfDataEvaluationCommon.EvalResultToString(env.Eval(decision.Col3));
            return new Dev2Decision() { Col1 = col1, Col2 = col2, Col3 = col3, EvaluationFn = decision.EvaluationFn };
        }

        protected override void ExecuteTool(IDSFDataObject dataObject)
        {

            var stack = Conditions.TheStack.Select(a => parseDecision(dataObject.Environment, a));

            var factory = Data.Decisions.Operations.Dev2DecisionFactory.Instance();
            var res = stack.Select(a=> factory.FetchDecisionFunction( a.EvaluationFn).Invoke( new []{a.Col1,a.Col2,a.Col3}));
            var resultval = res.Aggregate(true, (a, b) => a && b);
            if (resultval)
            {
                var activity = TrueArm.FirstOrDefault();
                if(activity != null)
                {
                    activity.Execute(dataObject);
                }
            }
            else
            {
                var activity = FalseArm.FirstOrDefault();
                if(activity != null)
                {
                    activity.Execute(dataObject);
                }
            }
        }

        #endregion
    }
}
