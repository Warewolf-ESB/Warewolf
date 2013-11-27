using System.Collections.Concurrent;

namespace Dev2.Collections
{
    public class ConcurrentDictionarySafe<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
    {
        public new TValue this[TKey key]
        {
            get
            {
                TValue result;
                TryGetValue(key, out result);
                return result;
            }
            set { base[key] = value; }
        }
    }
}