using System.Collections.Generic;
using System.Linq;

namespace Dev2.Diagnostics
{
    public class DebugItem : List<IDebugItemResult>, IDebugItem
    {
        public const int MaxItemDispatchCount = 10;
        public const int MaxCharDispatchCount = 150;
        public const int ActCharDispatchCount = 100;

        public static IDebugItem[] EmptyList = new IDebugItem[0];

        #region CTOR

        public DebugItem()
        {
        }

        public DebugItem(IEnumerable<IDebugItemResult> results)
            : base(results)
        {
        }

        #endregion

        #region Contains

        public bool Contains(string filterText)
        {
            return this.Any(r => r.Value.ContainsSafe(filterText) || r.GroupName.ContainsSafe(filterText));
        }

        #endregion

    }
}
