using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Warewolf.Storage
{
    public class WarewolfListIterator : IWarewolfListIterator
    {
        readonly IExecutionEnvironment _env;
        readonly Dictionary<string,WarewolfDataEvaluationCommon.WarewolfEvalResult> _variablesToIterateOn;
        int _maxCounter;
        int _currentCounter;

        public WarewolfListIterator(IExecutionEnvironment env)
        {
            if(env == null)
            {
                throw new ArgumentNullException("env");
            }
            _env = env;
            _variablesToIterateOn = new Dictionary<string, WarewolfDataEvaluationCommon.WarewolfEvalResult>();
            _currentCounter = 0;
        }

        public string FetchNextValue(string expression)
        {
            _currentCounter++;
            var warewolfEvalResult = _variablesToIterateOn[expression];
            if (warewolfEvalResult.IsWarewolfAtomListresult)
            {
                var listResult = warewolfEvalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
                if (listResult != null)
                {                    
                   return ExecutionEnvironment.WarewolfAtomToString(listResult.Item.GetNextValue());
                }
            }
            else if (warewolfEvalResult.IsWarewolfAtomResult)
            {
                var scalarResult = warewolfEvalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                if(scalarResult != null)
                {
                    return ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item);
                }
            }            
            return null;
        }

        public void AddVariableToIterateOn(string expression)
        {
            if (_variablesToIterateOn.ContainsKey(expression))
            {
                return;
            }
            var warewolfEvalResult = _env.Eval(expression);
            _variablesToIterateOn.Add(expression,warewolfEvalResult);
            var recSetName = DataListUtils.ExtractRecordsetNameFromValue(expression);
            var makeValueIntoHighLevelRecordset = DataListUtils.MakeValueIntoHighLevelRecordset(DataListUtils.AddBracketsToValueIfNotExist(recSetName));
            if (ExecutionEnvironment.IsRecordSetName(DataListUtils.AddBracketsToValueIfNotExist(makeValueIntoHighLevelRecordset)))
            {
                var newlyAdded = _env.GetLength(recSetName);
                if (newlyAdded > _maxCounter)
                {
                    _maxCounter = newlyAdded;
                }
            }
        }

        public bool HasMoreData()
        {
            return _currentCounter<=_maxCounter;
        }
    }
}
