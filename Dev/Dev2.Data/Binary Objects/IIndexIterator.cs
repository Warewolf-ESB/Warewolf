using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Data.Binary_Objects
{
    public interface IIndexIterator
    {

        int Count { get; }

        bool HasMore();

        int FetchNextIndex();

        int MaxIndex();

        int MinIndex();

        void AddGap(int idx);

        void RemoveGap(int idx);

        IIndexIterator Clone();
    }
}
