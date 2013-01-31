using Dev2.DataList.Contract.Binary_Objects;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Dev2.Diagnostics
{
    public class DebugItem : List<IDebugItemResult>, IDebugItem
    {
        private const int maxNumOfRows = 20;

        public static IDebugItem[] EmptyList = new IDebugItem[0];

        public string Group { get; set; }

        public bool ReachedMaxData { get; private set; }
        public string Link { get; set; }


        public DebugItem()
        {
        }

        public DebugItem(IEnumerable<IDebugItemResult> results)
            : base(results)
        {
        }

        // Travis.Frisinger - 28.01.2013 : Added for decision node
        public DebugItem(string scalarVariable, string scalarValue)
            : this()
        {
            Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = scalarVariable });
            Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = scalarValue });
        }

        public DebugItem(string label, string scalarVariable, string scalarValue)
            : this()
        {
            Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = label });
            Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = scalarVariable });
            Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = scalarValue });
        }

        public DebugItem(int index, IBinaryDataListItem recordField)
            : this()
        {
            var idxStr = index.ToString(CultureInfo.InvariantCulture);
            Group = recordField.Namespace;

            Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = idxStr });
            Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = string.Format("[[{0}({1}).{2}]]", recordField.Namespace, idxStr, recordField.FieldName) });
            Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = recordField.TheValue });
        }

        public bool Contains(string filterText)
        {
            return Group.ContainsSafe(filterText) || this.Any(r => r.Value.ContainsSafe(filterText));
        }
    }
}
