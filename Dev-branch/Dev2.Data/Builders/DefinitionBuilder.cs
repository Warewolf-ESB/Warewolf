using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Dev2.DataList.Contract
{
    public class DefinitionBuilder {

        private static readonly string _nameAttribute = "Name";
        private static readonly string _mapsToAttribute = "MapsTo";
        private static readonly string _valueAttribute = "Value";
        private static readonly string _recordsetAttribute = "Recordset";
        private static readonly string _defaultValueAttribute = "DefaultValue";
        private static readonly string _validateTag = "Validator";
        private static readonly string _typeAttribute = "Type";
        private static readonly string _requiredValue = "Required";
        private static readonly string _sourceAttribute = "Source";

        public enDev2ArgumentType ArgumentType { get; set; }

        public IList<IDev2Definition> Definitions { get; set; }

        /// <summary>
        /// Generates this instance.
        /// </summary>
        /// <returns></returns>
        public string Generate() {
            StringBuilder result = new StringBuilder();

            XmlDocument xDoc = new XmlDocument();
            XmlElement rootNode = xDoc.CreateElement((string.Concat(ArgumentType.ToString(), "s")));


            foreach (IDev2Definition def in Definitions) {
                XmlElement tmp = xDoc.CreateElement(ArgumentType.ToString());
                tmp.SetAttribute(_nameAttribute, def.Name);

                if (ArgumentType != enDev2ArgumentType.Input) {
                    tmp.SetAttribute(_mapsToAttribute, def.MapsTo);
                }else {
                    tmp.SetAttribute(_sourceAttribute, def.MapsTo);
                }
                
                if (ArgumentType != enDev2ArgumentType.Input) {
                    tmp.SetAttribute(_valueAttribute, def.Value);
                }

                if (def.RecordSetName.Length > 0) {
                    tmp.SetAttribute(_recordsetAttribute, def.RecordSetName);
                }
                
                if (def.DefaultValue.Length > 0 && ArgumentType == enDev2ArgumentType.Input) {
                    tmp.SetAttribute(_defaultValueAttribute, def.DefaultValue);
                }

                if (def.IsRequired && ArgumentType == enDev2ArgumentType.Input) {
                    XmlElement requiredElm = xDoc.CreateElement(_validateTag);
                    requiredElm.SetAttribute(_typeAttribute, _requiredValue);
                    tmp.AppendChild(requiredElm);
                }
                rootNode.AppendChild(tmp);
            }

            xDoc.AppendChild(rootNode);

            result.Append(xDoc.OuterXml);

            return result.ToString();
        }
    }
}
