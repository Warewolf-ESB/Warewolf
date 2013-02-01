using Dev2.DataList.Contract.Binary_Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Dev2.Diagnostics
{
    public class DebugItem : List<IDebugItemResult>, IDebugItem
    {
        public const int MaxItemDispatchCount = 10;
        public const int MaxCharDispatchCount = 150;

        public static IDebugItem[] EmptyList = new IDebugItem[0];

        [Obsolete]
        public string Group { get; set; }
        public string MoreText { get; set; }
        public string MoreLink { get; set; }

        #region CTOR

        public DebugItem()
        {
        }

        public DebugItem(IEnumerable<IDebugItemResult> results)
            : base(results)
        {
        }

        // Travis.Frisinger - 28.01.2013 : Added for decision node
        [Obsolete]
        public DebugItem(string scalarVariable, string scalarValue)
            : this()
        {
            Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = scalarVariable });
            Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = scalarValue });
        }
        [Obsolete]
        public DebugItem(string label, string scalarVariable, string scalarValue)
            : this()
        {
            Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = label });
            Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = scalarVariable });
            Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = scalarValue });
        }
        [Obsolete]
        public DebugItem(int index, IBinaryDataListItem recordField)
            : this()
        {
            var idxStr = index.ToString(CultureInfo.InvariantCulture);

            Add(new DebugItemResult { GroupName = recordField.Namespace, Type = DebugItemResultType.Label, Value = idxStr });
            Add(new DebugItemResult { GroupName = recordField.Namespace, Type = DebugItemResultType.Variable, Value = string.Format("[[{0}({1}).{2}]]", recordField.Namespace, idxStr, recordField.FieldName) });
            Add(new DebugItemResult { GroupName = recordField.Namespace, Type = DebugItemResultType.Value, Value = recordField.TheValue });
        }

        #endregion

        #region Contains

        public bool Contains(string filterText)
        {
            return this.Any(r => r.Value.ContainsSafe(filterText) || r.GroupName.ContainsSafe(filterText));
        }

        #endregion

        #region ToXml

        public XElement ToXml()
        {
            var xml = new XElement("Item");
            if(!string.IsNullOrEmpty(Group))
            {
                xml.Add(new XAttribute("Group", Group));
            }
            foreach(var result in this)
            {
                if(!string.IsNullOrEmpty(result.Value))
                {
                    xml.Add(new XElement("Result", new XAttribute("Type", result.Type), result.Value));
                }
            }
            return xml;
        }

        #endregion

    }
}
