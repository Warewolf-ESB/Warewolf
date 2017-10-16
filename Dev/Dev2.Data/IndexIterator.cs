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
using System.Linq;
using Dev2.Data.Binary_Objects;

namespace Dev2.Data
{
    /// <summary>
    /// Used to drive recordset lanaguage use
    /// </summary>
    [Serializable]
    public class IndexIterator : IIndexIterator
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

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => IndexList.Count();

        /// <summary>
        /// Gets a value indicating whether [is empty].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is empty]; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get
            {
                int result = _curValue - Count;

                if(result == 0 && HasMore())
                {
                    return false;
                }

                // we know this case things are always empty ;) ... most likely a manual delete
                if(Count == 0)
                {
                    return true;
                }

                return result == 0;
            }
        }

        public IndexIterator(HashSet<int> gaps, int maxValue)
            : this(gaps, maxValue, 1)
        {
        }

        public IndexIterator(HashSet<int> gaps, int maxValue, int minValue)
        {
            IndexList = new IndexList(gaps, maxValue, minValue);
            _curValue = minValue;
        }
        
        public bool HasMore()
        {
            int canidate = _curValue;
            while(IndexList.Gaps.Contains(canidate))
            {
                canidate++;
            }

            return canidate <= IndexList.MaxValue;
        }
        
        public int FetchNextIndex()
        {
            int canidate = _curValue;

            while(IndexList.Gaps.Contains(canidate))
            {
                canidate++;
            }

            int result = canidate;

            _curValue = canidate + 1;

            return result;
        }
        
        public int MaxIndex()
        {
            return IndexList.GetMaxIndex();
        }
    }

    public class IndexListIndexIterator:IIndexIterator
    {

        private readonly IList<int> _values;
        int _current;

        public IndexListIndexIterator(IList<int> values)
        {
            _values = values;
            _current = 0;
        }

        public int Count => _values.Count;

        public bool IsEmpty => _values.Count == 0;

        public bool HasMore()
        {
            return _current < Count;
        }

        public int FetchNextIndex()
        {
            
            return _values[_current++];
            
        }

        public int MaxIndex()
        {
            return _values.Max();
        }
    }
}
