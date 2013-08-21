
using System.Collections.Generic;

namespace Dev2.Diagnostics
{
    // If you add/remove columns here then 
    // change DebugState.Serialize/Deserialize
    public interface IDebugItem
    {
        bool Contains(string filterText);
        void Add(DebugItemResult itemToAdd, bool isDeserialize = false);
        void AddRange(IList<DebugItemResult> itemsToAdd);
        IList<DebugItemResult> FetchResultsList();        
        void FlushStringBuilder();
        void TryCache(DebugItemResult item);
        string SaveFile(string contents, string fileName);
    }
}
