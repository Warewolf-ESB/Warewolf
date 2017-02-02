/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;

namespace Dev2.Data.Binary_Objects
{
    public class ListIndexIterator : IIndexIterator
    {
        private int _curValue;
        private int _curPos;
        private ListOfIndex _indexList;

        public ListOfIndex IndexList
        {
            get
            {
                return _indexList;
            }
            set
            {
                _indexList = value;
                _curValue = value.MinValue;
            }
        }

        public int Count => IndexList.Count();

        public bool IsEmpty => _curValue - Count == 0;

        public ListIndexIterator(List<int> indexes)
        {

            IndexList = new ListOfIndex(indexes);
            _curPos = 0;
            _curValue = IndexList.Indexes[_curPos];
        }

        public bool HasMore()
        {
            bool result = _curPos < IndexList.Count();
            return result;
        }

        public int FetchNextIndex()
        {
            _curPos++;

            _curValue = IndexList.Indexes[_curPos - 1];

            return _curValue;
        }

        public int MaxIndex()
        {
            return IndexList.GetMaxIndex();
        }
    }
}
