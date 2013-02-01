using Dev2.DataList.Contract.Binary_Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Dev2.Diagnostics
{
    public class DebugItem : List<IDebugItemResult>, IDebugItem
    {
        public const int MaxItemDispatchCount = 10;
        public const int MaxCharDispatchCount = 150;
        public const int ActCharDispatchCount = 100;

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
            var xml = new XElement("Items");

            var infoText = new StringBuilder();
            var rowText = new StringBuilder();

            var recName = string.Empty;
            var recIndex = 0;
            XElement recXml = null;

            foreach(var result in this)
            {
                if(string.IsNullOrEmpty(result.Value))
                {
                    continue;
                }

                if(string.IsNullOrEmpty(result.GroupName))
                {
                    Append(infoText, result.Value);
                }
                else
                {
                    AddChild(xml, "Item", infoText);

                    if(result.GroupName != recName)
                    {
                        rowText.Length = 0;
                        recIndex = result.GroupIndex;
                        recName = result.GroupName;
                        recXml = new XElement("Rec", new XAttribute("name", recName));
                        xml.Add(recXml);
                    }

                    if(result.GroupIndex != recIndex)
                    {
                        rowText.Length = 0;
                        recIndex = result.GroupIndex;
                        AddChild(recXml, "Row", rowText);
                    }

                    Append(rowText, result.Value);
                }
            }

            AddChild(xml, "Item", infoText);
            AddChild(recXml, "Row", rowText);

            return xml;
        }

        static void AddChild(XContainer parent, string textName, StringBuilder text)
        {
            if(text.Length > 0 && parent != null)
            {
                parent.Add(new XElement(textName, text.ToString()));
                text.Length = 0;
            }
        }

        static void Append(StringBuilder text, string str)
        {
            if(text.Length > 0)
            {
                text.Append(' ');
            }
            text.Append(str);
        }

        #endregion

    }
}
