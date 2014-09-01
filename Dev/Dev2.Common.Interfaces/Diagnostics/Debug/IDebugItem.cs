using System.Collections.Generic;
using System.Xml.Serialization;

namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    // If you add/remove columns here then 
    // change DebugState.Serialize/Deserialize
    public interface IDebugItem : IXmlSerializable
    {
        bool Contains(string filterText);
        void Add(IDebugItemResult itemToAdd, bool isDeserialize = false);
        void AddRange(List<IDebugItemResult> itemsToAdd);
        IList<IDebugItemResult> FetchResultsList();        
        void FlushStringBuilder();
        void TryCache(IDebugItemResult item);
        string SaveFile(string contents, string fileName);
        List<IDebugItemResult> ResultsList { get; set; }
    }
}
