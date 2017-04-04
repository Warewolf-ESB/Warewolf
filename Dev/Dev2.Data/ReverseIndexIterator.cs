/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;

namespace Dev2.Data.Binary_Objects
{
    [Serializable]
    public class ReverseIndexIterator : IIndexIterator
    {
        private int _curValue;
        private IndexList _indexList;

        public IndexList IndexList
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

        public ReverseIndexIterator(HashSet<int> gaps, int maxValue)
        {
            IndexList = new IndexList(gaps, maxValue);
            _curValue = 1;
        }

        public bool HasMore()
        {
            int canidate = _curValue;
            while(IndexList.Gaps.Contains(canidate))
            {
                canidate--;
            }

            return canidate >= IndexList.MaxValue;
        }

        public int FetchNextIndex()
        {

            int canidate = _curValue;
            // assign a new curValue

            while(IndexList.Gaps.Contains(canidate))
            {
                canidate--;
            }

            int result = canidate;

            _curValue = canidate - 1; // save next value ;)

            return result;
        }

        public int MaxIndex()
        {
            return IndexList.GetMaxIndex();
        }
    }
}
