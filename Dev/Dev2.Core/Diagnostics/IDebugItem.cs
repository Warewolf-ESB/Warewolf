
using System.Collections.Generic;

namespace Dev2.Diagnostics
{
    // If you add/remove columns here then 
    // change DebugState.Serialize/Deserialize
    public interface IDebugItem
    {
        bool Contains(string filterText);
        void Add(IDebugItemResult itemToAdd);
        void AddRange(IList<IDebugItemResult> itemsToAdd);
        IList<IDebugItemResult> FetchResultsList();
    }
}
