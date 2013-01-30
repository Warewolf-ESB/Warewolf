using System;
using System.Collections.Generic;

namespace Dev2.Data.Binary_Objects
{
    [Serializable]
    public class IndexIterator : IIndexIterator
    {
        private readonly HashSet<int> _gaps;
        private int _maxValue;
        private int _curValue;

        public int Count
        {
            get { return (_maxValue - _gaps.Count); }
        }


        public IndexIterator(HashSet<int> gaps, int maxValue)
        {
            _gaps = gaps;
            _maxValue = maxValue;
            _curValue = 1;
        }

        public bool HasMore()
        {
            int canidate = _curValue;
            while (_gaps.Contains(canidate))
            {
                canidate++;
            }

            return (canidate <= _maxValue);
        }

        public int FetchNextIndex()
        {

            int canidate = _curValue;
            int result = _curValue;
            // assign a new curValue
            // _curValue++;
            while (_gaps.Contains(canidate))
            {
                canidate++;
            }

            result = canidate;

            _curValue = canidate + 1; // save next value ;)

            return result;
        }

        public int MaxIndex()
        {
            int result = _maxValue;
            while (_gaps.Contains(result))
            {
                result--;
            }

            _maxValue = result; // reset to new value ;)

            return _maxValue;
        }

        public int MinIndex()
        {
            int result = 1;
            while (_gaps.Contains(result))
            {
                result++;
            }

            return result;
        }

        public void AddGap(int idx)
        {
            _gaps.Add(idx);
        }


        public void RemoveGap(int idx)
        {
            _gaps.Remove(idx);
        }

        public IIndexIterator Clone()
        {

            HashSet<int> gaps = new HashSet<int>();
            foreach (int g in _gaps)
            {
                gaps.Add(g);
            }
            return new IndexIterator(gaps, _maxValue);
        }

    }
}
