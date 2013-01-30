
using Dev2.DataList.Contract.Binary_Objects;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Dev2.Diagnostics
{
    public class DebugItem : IDebugItem
    {
        private const int maxNumOfRows = 20;

        public static IDebugItem[] EmptyList = new IDebugItem[0];

        public string Group { get; set; }
        public string Label { get; set; }
        public List<IDebugItemResult> Results { get; private set; }
        public bool ReachedMaxData { get; private set; }
        public string Link { get; set; }


        public DebugItem()
        {
            Results = new List<IDebugItemResult>();
        }

        // Travis.Frisinger - 28.01.2013 : Added for decision node
        public DebugItem(string scalarVariable, string scalarValue)
            : this()
        {
            Results.Add(new DebugItemResult { Variable = scalarVariable, Value = scalarValue });
        }

        public DebugItem(string label, string scalarVariable, string scalarValue)
            : this()
        {
            Label = label;
            Results.Add(new DebugItemResult { Variable = scalarVariable, Value = scalarValue });
        }

        public DebugItem(int index, IBinaryDataListItem recordField)
            : this()
        {
            var idxStr = index.ToString(CultureInfo.InvariantCulture);
            Group = recordField.Namespace;
            Label = idxStr;
            Results.Add(new DebugItemResult
                {
                    Variable =
                        string.Format("[[{0}({1}).{2}]]", recordField.Namespace, idxStr, recordField.FieldName),
                    Value = recordField.TheValue
                });
        }

        public bool Contains(string filterText)
        {
            return Group.ContainsSafe(filterText)
                   || Label.ContainsSafe(filterText)
                   || Results.Any(r => r.Variable.ContainsSafe(filterText) || r.Value.ContainsSafe(filterText));
        }
    }

    public class DebugItemResult : IDebugItemResult
    {
        public string Variable { get; set; }
        public string Value { get; set; }
    }

}
