using System;
using System.Collections.Generic;

namespace Dev2.Data.Binary_Objects
{

    [Serializable]
    public class IndexList
    {
        private int _maxValue;
        private bool _inited = false;
        public int MaxValue { 
            get { return _maxValue; }
            set { 
                _maxValue = value;
                _inited = true;
            }
        }
        public int MinValue { get; set; }

        public HashSet<int> Gaps {get; private set;}

        private IndexList(){}

        public IndexList(HashSet<int> gaps, int maxValue, int minValue = 1)
        {
            if (gaps == null)
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
            while (Gaps.Contains(result) && result > 1)
            {
                result--;
            }

            return result;
        }

        public int GetMinIndex()
        {
            int result = MinValue;
            while (Gaps.Contains(result))
            {
                result++;
            }

            return result;
        }

        public void AddGap(int idx)
        {
            if (idx > 0)
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

            if (MinValue > 1)
            {
                var res = (MaxValue - MinValue);

                return res;
            }

            // Travis.Frisinger - Count bug change
            int result = MaxValue - Gaps.Count;

            if (result == 0 && _inited)
            {
                return 1;
            }

            if (result < 0)
            {
                return 0;
            }

            return result;
        }

        public void SetMaxValue(int idx, bool isEmpty)
        {
            var currMax = MaxValue;

            if (idx > MaxValue && idx > 0)
            {
                MaxValue = idx;

                // set to zero so we populate gaps correctly ;)
                if (isEmpty)
                {
                    currMax = 0;
                }

                // now fill in the gaps?!
                for (int i = (currMax + 1); i < idx; i++)
                {
                    Gaps.Add(i);
                }
            }

            // check to ensure idx is not in the gaps collection ;)
            if (Gaps.Contains(idx))
            {
                Gaps.Remove(idx);
            }
        }

        public IIndexIterator FetchIterator()
        {
            return new IndexIterator(Gaps, MaxValue, MinValue);
        }
    }
}

