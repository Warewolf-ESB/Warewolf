using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;

namespace Warewolf.Storage
{
    public class WarewolfAtomIterator : IWarewolfIterator
    {
        readonly IEnumerator<DataStorage.WarewolfAtom> _listResult;

        readonly int _maxValue;
        int _currentValue;
        DataStorage.WarewolfAtom _currentResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WarewolfAtomIterator(IEnumerable<DataStorage.WarewolfAtom> warewolfEvalResult)
        {
            IEnumerable<DataStorage.WarewolfAtom> warewolfAtoms = warewolfEvalResult as DataStorage.WarewolfAtom[] ?? warewolfEvalResult.ToArray();
            _listResult = warewolfAtoms.GetEnumerator();
            _maxValue = warewolfAtoms.Count();
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
                if (_listResult.MoveNext())
                {
                    _currentResult = _listResult.Current;
                    return ExecutionEnvironment.WarewolfAtomToStringNullAsNothing(_listResult.Current);
                }
                return ExecutionEnvironment.WarewolfAtomToStringNullAsNothing(_currentResult);
            }
            return null;
        }

        public bool HasMoreData()
        {
            return _currentValue<=_maxValue-1;
        }
        #endregion
    }
}