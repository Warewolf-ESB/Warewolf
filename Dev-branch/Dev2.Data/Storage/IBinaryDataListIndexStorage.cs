using System;
using System.Collections.Generic;

namespace Dev2.Data.Storage
{
    public interface IBinaryDataListIndexStorage : IDisposable
    {
        int Count { get; }

        string IndexFilePath { get; }

        ICollection<string> Keys { get; }

        bool ContainsKey(string key);

        bool GetPositionLength(string key, out long position, out int length);

        void AddIndex(string key, long position, int length);

        void RemoveIndex(string key);
    }
}
