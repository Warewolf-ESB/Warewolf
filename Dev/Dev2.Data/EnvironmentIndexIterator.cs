using System.Collections.Generic;
using System.Linq;
using Dev2.Data.Binary_Objects;

namespace Dev2.Data
{
    public class EnvironmentIndexIterator : IIndexIterator
    {
        readonly IList<int> _indexes;
        readonly IEnumerator<int> _enumerator;
        bool _hasMore;
        #region Implementation of IIndexIterator

        public EnvironmentIndexIterator(IList<int> indexes )
        {
            _indexes = indexes;
            _hasMore = _indexes.Count > 0;
            _enumerator = _indexes.GetEnumerator();
        }

        public int Count => _indexes.Count;
        public bool IsEmpty => _indexes.Count==0;

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

        #endregion
    }
}