using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Data.Util;
using Dev2.MathOperations;
using Warewolf.Storage;

namespace Dev2.Data
{
    public class WarewolfIterator : IWarewolfIterator
    {
        readonly WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult _listResult;
        readonly WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult _scalarResult;
        readonly int _maxValue;
        int _currentValue;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WarewolfIterator(WarewolfDataEvaluationCommon.WarewolfEvalResult warewolfEvalResult)
        {
            if (warewolfEvalResult.IsWarewolfAtomListresult)
            {
                _listResult = warewolfEvalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;                
            }
            else if (warewolfEvalResult.IsWarewolfAtomResult)
            {
                _scalarResult = warewolfEvalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;               
            }
            _maxValue = _listResult != null ? _listResult.Item.Count(atom => !atom.IsNothing) : 1;
            _currentValue = 0;
        }        

        #region Implementation of IWarewolfIterator

        public int GetLength()
        {
            return _maxValue;
        }

        public string GetNextValue()
        {
            _currentValue++;
            if (_listResult != null)
            {
                var warewolfAtomToString = ExecutionEnvironment.WarewolfAtomToString(_listResult.Item.GetNextValue());                
                warewolfAtomToString = DoCalcution(warewolfAtomToString);
                return warewolfAtomToString;
            }
            return _scalarResult!=null ? DoCalcution(ExecutionEnvironment.WarewolfAtomToString(_scalarResult.Item)) : null;
        }

        static string DoCalcution(string warewolfAtomToString)
        {
            string cleanExpression;
            var isCalcEvaluation = DataListUtil.IsCalcEvaluation(warewolfAtomToString, out cleanExpression);
            var functionEvaluator = new FunctionEvaluator();
            if(isCalcEvaluation)
            {
                string eval;
                string error;
                functionEvaluator.TryEvaluateFunction(cleanExpression, out eval, out error);
                warewolfAtomToString = eval;
            }
            return warewolfAtomToString;
        }

        public bool HasMoreData()
        {
            return _currentValue<=_maxValue-1;
        }
        #endregion
    }
}