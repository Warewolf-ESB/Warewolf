using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Data.Binary_Objects
{
    public class ListIndexIterator :IIndexIterator
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

        public int Count
        {
            get { return (IndexList.Count()); }
        }

        public bool IsEmpty { get { return (_curValue - Count == 0); } }

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

            _curValue = IndexList.Indexes[_curPos-1];

            return _curValue;
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
            IndexList.Indexes.Add(idx);
        }

        public void RemoveGap(int idx)
        {
            IndexList.Indexes.Remove(idx);
        }

        public IIndexIterator Clone()
        {
            List<int> indexes = new List<int>();
            foreach (int g in IndexList.Indexes)
            {
                indexes.Add(g);
            }
            return new ListIndexIterator(indexes);
        }
    }
}
