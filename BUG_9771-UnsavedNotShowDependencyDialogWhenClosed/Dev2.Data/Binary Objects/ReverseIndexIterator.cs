using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public int Count
        {
            get { return (IndexList.Count()); }
        }

        public bool IsEmpty { get { return (_curValue - Count == 0); } }

        public ReverseIndexIterator(HashSet<int> gaps, int maxValue)
        {
            IndexList = new IndexList(gaps, maxValue);
            _curValue = 1;
        }

        public bool HasMore()
        {
            int canidate = _curValue;
            while (IndexList.Gaps.Contains(canidate))
            {
                canidate--;
            }

            return (canidate >= IndexList.MaxValue);
        }

        public int FetchNextIndex()
        {

            int canidate = _curValue;
            int result = _curValue;
            // assign a new curValue
            // _curValue++;
            while (IndexList.Gaps.Contains(canidate))
            {
                canidate--;
            }

            result = canidate;

            _curValue = canidate - 1; // save next value ;)

            return result;
        }

        public int MaxIndex()
        {
            return IndexList.GetMaxIndex();
        }

        public int MinIndex()
        {
            return IndexList.GetMinIndex();
        }

        public void AddGap(int idx)
        {
            IndexList.Gaps.Add(idx);
        }

        public void RemoveGap(int idx)
        {
            IndexList.Gaps.Remove(idx);
        }

        public IIndexIterator Clone()
        {
            HashSet<int> gaps = new HashSet<int>();
            foreach (int g in IndexList.Gaps)
            {
                gaps.Add(g);
            }
            return new IndexIterator(gaps, IndexList.MaxValue);
        }
    }
}
