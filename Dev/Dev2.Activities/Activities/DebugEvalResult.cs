using System;
using System.Collections.Generic;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Warewolf.Storage;

namespace Dev2.Activities
{
    public class DebugEvalResult : DebugOutputBase
    {
        readonly string _inputVariable;
        readonly string _label;
        readonly WarewolfDataEvaluationCommon.WarewolfEvalResult _evalResult;

        public DebugEvalResult(string inputVariable, string label,IExecutionEnvironment environment)
        {
            _inputVariable = inputVariable;
            _label = label;
            try
            {
                _evalResult = environment.Eval(inputVariable);
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error(e.Message,e);
                _evalResult = WarewolfDataEvaluationCommon.WarewolfEvalResult.NewWarewolfAtomResult(DataASTMutable.WarewolfAtom.Nothing);
            }
            
        }

        #region Overrides of DebugOutputBase

        public override string LabelText
        {
            get
            {
                return _label;
            }
        }

        public override List<IDebugItemResult> GetDebugItemResult()
        {
            if (_evalResult.IsWarewolfAtomResult)
            {
                var scalarResult = _evalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                if (scalarResult != null && !scalarResult.Item.IsNothing)
                {
                    var warewolfAtomToString = ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item);
                    if (warewolfAtomToString == _inputVariable)
                    {
                        warewolfAtomToString = "";
                    }
                    return new DebugItemWarewolfAtomResult(warewolfAtomToString, _inputVariable, LabelText).GetDebugItemResult();
                }
            }
            else if (_evalResult.IsWarewolfAtomListresult)
            {
                var listResult = _evalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
                if (listResult != null)
                {
                    return new DebugItemWarewolfAtomListResult(listResult, "", "", _inputVariable, LabelText, "", "=").GetDebugItemResult();
                }
            }
            return new DebugItemStaticDataParams("",_inputVariable,LabelText).GetDebugItemResult();
        }

        #endregion
    }
}