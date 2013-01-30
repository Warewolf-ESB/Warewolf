
using System.Collections.Generic;

namespace Dev2.Diagnostics
{
    // If you add/remove columns here then 
    // change DebugState.Serialize/Deserialize
    public interface IDebugItem
    {
        string Group { get; set; }
        string Label { get; set; }
        List<IDebugItemResult> Results { get; }

        bool Contains(string filterText);
    }

    public interface IDebugItemResult
    {
        string Variable { get; set; }
        string Value { get; set; }
    }
}
