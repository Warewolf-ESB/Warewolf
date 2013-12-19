using System.Collections.Generic;

namespace Dev2.Data.Binary_Objects
{
    public interface IIndexIterator
    {

        int Count { get; }

        bool IsEmpty { get;  }

        bool HasMore();

        int FetchNextIndex();

        int MaxIndex();

        int MinIndex();

        void AddGap(int idx);

        void RemoveGap(int idx);

        HashSet<int> FetchGaps();

        IIndexIterator Clone();
    }
}
