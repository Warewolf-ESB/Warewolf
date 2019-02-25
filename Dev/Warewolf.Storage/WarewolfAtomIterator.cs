/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
        
        public WarewolfAtomIterator(IEnumerable<DataStorage.WarewolfAtom> warewolfEvalResult)
        {
            IEnumerable<DataStorage.WarewolfAtom> warewolfAtoms = warewolfEvalResult as DataStorage.WarewolfAtom[] ?? warewolfEvalResult.ToArray();
            _listResult = warewolfAtoms.GetEnumerator();
            _maxValue = warewolfAtoms.Count();
        }

        public int GetLength() => _maxValue;

        public string GetNextValue()
        {
            _currentValue++; 
            
            if (_listResult.MoveNext())
            {
                _currentResult = _listResult.Current;
                return ExecutionEnvironment.WarewolfAtomToStringNullAsNothing(_listResult.Current);
            }
            return ExecutionEnvironment.WarewolfAtomToStringNullAsNothing(_currentResult);
           
        }

        public bool HasMoreData() => _currentValue <= _maxValue - 1;
    }
}