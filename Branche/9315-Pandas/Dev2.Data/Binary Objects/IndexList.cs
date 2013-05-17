using System;
using System.Collections.Generic;

namespace Dev2.Data.Binary_Objects
{

    [Serializable]
    public class IndexList
    {
        public int MaxValue { get;  set; }
        public int MinValue { get; set; }

        public HashSet<int> Gaps {get; private set;}

        private IndexList()
        {
            
        }

        public IndexList(HashSet<int> gaps, int maxValue)
        {
            if (gaps == null) gaps = new HashSet<int>();
            Gaps = gaps;
            MinValue = 1;
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
            int result = 1;
            while (Gaps.Contains(result))
            {
                result++;
            }

            return result;
        }

        public void AddGap(int idx)
        {
            Gaps.Add(idx);
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
            // Travis.Frisinger - Count bug change
            return (MaxValue - Gaps.Count);
        }

        public IIndexIterator FetchIterator()
        {
            return new IndexIterator(Gaps, MaxValue);
        }
    }
}

