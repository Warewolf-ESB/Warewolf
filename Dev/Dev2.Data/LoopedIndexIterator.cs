/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

namespace Dev2.Data.Binary_Objects
{
    public class LoopedIndexIterator : IIndexIterator
    {
        private readonly int _loopedIdx;
        private int _curPos;
        private readonly int _itrCnt;

        public int Count => _itrCnt;

        public bool IsEmpty => _itrCnt > 0;

        public LoopedIndexIterator(int val, int itrCnt)
        {
            _loopedIdx = val;
            _itrCnt = itrCnt;
        }

        public bool HasMore()
        {
            return _curPos < _itrCnt;
        }

        public int FetchNextIndex()
        {
            _curPos++;
            return _loopedIdx;
        }

        public int MaxIndex()
        {
            return _loopedIdx;
        }


    }
}
