
using System.Collections.Generic;

namespace Dev2.Diagnostics
{
    // If you add/remove columns here then 
    // change DebugState.Serialize/Deserialize
    public interface IDebugItem : IList<IDebugItemResult>
    {
        string Group { get; set; }

        bool Contains(string filterText);
    }
}
