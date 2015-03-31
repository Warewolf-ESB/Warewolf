
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Data.Binary_Objects
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
        public int Count
        {
            get { return (IndexList.Count()); }
        }

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

                return (result == 0);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexIterator"/> class.
        /// </summary>
        /// <param name="gaps">The gaps.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="minValue">The minimum value.</param>
        public IndexIterator(HashSet<int> gaps, int maxValue, int minValue = 1)
        {
            IndexList = new IndexList(gaps, maxValue, minValue);
            _curValue = minValue;
        }

        /// <summary>
        /// Determines whether this instance has more.
        /// </summary>
        /// <returns></returns>
        public bool HasMore()
        {
            int canidate = _curValue;
            while(IndexList.Gaps.Contains(canidate))
            {
                canidate++;
            }

            return (canidate <= IndexList.MaxValue);
        }

        /// <summary>
        /// Fetches the index of the next.
        /// </summary>
        /// <returns></returns>
        public int FetchNextIndex()
        {

            int canidate = _curValue;
            // assign a new curValue

            while(IndexList.Gaps.Contains(canidate))
            {
                canidate++;
            }

            int result = canidate;

            _curValue = canidate + 1; // save next value ;)

            return result;
        }

        /// <summary>
        /// Maximums the index.
        /// </summary>
        /// <returns></returns>
        public int MaxIndex()
        {
            return IndexList.GetMaxIndex();
        }

        /// <summary>
        /// Minimums the index.
        /// </summary>
        /// <returns></returns>
        public int MinIndex()
        {
            return IndexList.GetMinIndex();
        }

        /// <summary>
        /// Adds the gap.
        /// </summary>
        /// <param name="idx">The index.</param>
        public void AddGap(int idx)
        {
            IndexList.Gaps.Add(idx);
        }

        /// <summary>
        /// Removes the gap.
        /// </summary>
        /// <param name="idx">The index.</param>
        public void RemoveGap(int idx)
        {
            IndexList.Gaps.Remove(idx);
        }

        public HashSet<int> FetchGaps()
        {
            return IndexList.Gaps;
        }

        public IIndexIterator Clone()
        {
            HashSet<int> gaps = new HashSet<int>();
            foreach(int g in IndexList.Gaps)
            {
                gaps.Add(g);
            }
            return new IndexIterator(gaps, IndexList.MaxValue);
        }

    }

    public class IndexListIndexIterator:IIndexIterator
    {

        private IList<int> _values;
        IEnumerator<int> _enumerator;
        int _max;
        int _current;

        public IndexListIndexIterator(IList<int> values)
        {
            _values = values;
            _enumerator = _values.GetEnumerator();
            _max = _values.Count;
            _current = 0;
        }

        public int Count
        {
            get { return _values.Count; }
        }

        public bool IsEmpty
        {
            get { return _values.Count == 0; }
        }

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

        public int MinIndex()
        {
            return _values.Min();
        }

        public void AddGap(int idx)
        {
            
        }

        public void RemoveGap(int idx)
        {
          
        }

        public HashSet<int> FetchGaps()
        {
            return  new HashSet<int>();
        }

        public IIndexIterator Clone()
        {
            return new IndexListIndexIterator(_values);
        }
    }
}
