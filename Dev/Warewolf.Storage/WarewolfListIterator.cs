using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;

namespace Warewolf.Storage
{
    public class WarewolfListIterator : IWarewolfListIterator
    {
        readonly List<IWarewolfIterator> _variablesToIterateOn;

        public WarewolfListIterator()
        {
            _variablesToIterateOn = new List<IWarewolfIterator>();
        }

        public string FetchNextValue(IWarewolfIterator expression)
        {
            var warewolfEvalResult = _variablesToIterateOn[_variablesToIterateOn.IndexOf(expression)];
            if (warewolfEvalResult!=null)
            {
                return warewolfEvalResult.GetNextValue();
            }            
            return null;
        }

        public void AddVariableToIterateOn(IWarewolfIterator iterator)
        {
            _variablesToIterateOn.Add(iterator);            
        }

        public bool HasMoreData()
        {
            return _variablesToIterateOn.Any(iterator => iterator.HasMoreData());
        }
    }

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
                return ExecutionEnvironment.WarewolfAtomToString(_listResult.Item.GetNextValue());
            }
            return _scalarResult!=null ? ExecutionEnvironment.WarewolfAtomToString(_scalarResult.Item) : null;
        }

        public bool HasMoreData()
        {
            return _currentValue<=_maxValue-1;
        }
        #endregion
    }
}
