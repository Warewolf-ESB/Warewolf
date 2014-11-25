
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

namespace Dev2.Data.Binary_Objects
{
    [Serializable]
    public class IndexList
    {
        private int _maxValue;

        public int MaxValue
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
            }
        }

        public int MinValue { get; set; }

        public HashSet<int> Gaps { get; private set; }

        private IndexList() { }

        public IndexList(HashSet<int> gaps, int maxValue, int minValue = 1)
        {
            if(gaps == null)
            {
                gaps = new HashSet<int>();
            }

            Gaps = gaps;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public int GetMaxIndex()
        {
            int result = MaxValue;
            while(Gaps.Contains(result) && result >= 1)
            {
                result--;
            }
            return result;
        }
        public int GetMinIndex()
        {
            int result = MinValue;
            while(Gaps.Contains(result))
            {
                result++;
            }

            return result;
        }

        public void AddGap(int idx)
        {
            if(idx > 0)
            {
                Gaps.Add(idx);
            }
        }

        public void RemoveGap(int idx)
        {
            Gaps.Remove(idx);
        }

        public bool Contains(int idx)
        {
            bool result = (idx <= MaxValue && idx >= 0 && !Gaps.Contains(idx));

            return result;
        }

        public int Count()
        {

            if(MinValue > 1)
            {
                var res = (MaxValue - MinValue);

                return res;
            }

            // Travis.Frisinger - Count bug change
            int result = MaxValue - Gaps.Count;

            return result;
        }

        public void SetMaxValue(int idx, bool isEmpty)
        {
            var currMax = MaxValue;

            if(idx > MaxValue && idx > 0)
            {
                MaxValue = idx;

                // set to zero so we populate gaps correctly ;)
                if(isEmpty)
                {
                    currMax = 0;
                }

                // now fill in the gaps?!
                for(int i = (currMax + 1); i < idx; i++)
                {
                    Gaps.Add(i);
                }
            }

        }

        public void SetGapsCollection(HashSet<int> myGaps)
        {
            Gaps = new HashSet<int>(myGaps);
        }

        public IIndexIterator FetchIterator()
        {
            return new IndexIterator(Gaps, MaxValue, MinValue);
        }
    }
}

