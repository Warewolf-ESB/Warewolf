using System.Collections.Generic;
using System.Linq;

namespace Dev2.Data.Binary_Objects
{
    public class EnvironmentIndexIterator : IIndexIterator
    {
        readonly IList<int> _indexes;
        IEnumerator<int> _enumerator;
        bool _hasMore;
        #region Implementation of IIndexIterator

        public EnvironmentIndexIterator(IList<int> indexes )
        {
            _indexes = indexes;
            _hasMore = _indexes.Count > 0;
            _enumerator = _indexes.GetEnumerator();
        }

        public int Count { get { return _indexes.Count; } }
        public bool IsEmpty { get {return _indexes.Count==0;} }

        public bool HasMore()
        {
            return _hasMore;
        }

        public int FetchNextIndex()
        {
            _hasMore =  _enumerator.MoveNext();
            if(_hasMore)
            {
                return _enumerator.Current;
            }
            return -1;
        }

        public int MaxIndex()
        {
            return _indexes.Last();
        }

        public int MinIndex()
        {
            return _indexes[0];
        }

        public void AddGap(int idx)
        {
        }

        public void RemoveGap(int idx)
        {
        }

        public HashSet<int> FetchGaps()
        {
            return null;
        }

        public IIndexIterator Clone()
        {
            return null;
        }

        #endregion
    }
}