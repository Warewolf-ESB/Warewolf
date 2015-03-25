using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;

namespace Warewolf.Storage
{
    public class WarewolfAtomIterator : IWarewolfIterator
    {
        readonly IEnumerator<DataASTMutable.WarewolfAtom> _listResult;

        readonly int _maxValue;
        int _currentValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WarewolfAtomIterator(IEnumerable<DataASTMutable.WarewolfAtom> warewolfEvalResult)
        {
            IEnumerable<DataASTMutable.WarewolfAtom> warewolfAtoms = warewolfEvalResult as DataASTMutable.WarewolfAtom[] ?? warewolfEvalResult.ToArray();
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
            if (_listResult != null && _listResult.MoveNext())
            {
                return ExecutionEnvironment.WarewolfAtomToString(_listResult.Current);
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