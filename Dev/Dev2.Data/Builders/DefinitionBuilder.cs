using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Dev2.DataList.Contract
{
    public class DefinitionBuilder
    {
        const string _nameAttribute = "Name";
        const string _mapsToAttribute = "MapsTo";
        const string _valueAttribute = "Value";
        const string _recordsetAttribute = "Recordset";
        const string _defaultValueAttribute = "DefaultValue";
        const string _validateTag = "Validator";
        const string _typeAttribute = "Type";
        const string _requiredValue = "Required";
        const string _sourceAttribute = "Source";

        public enDev2ArgumentType ArgumentType { get; set; }

        public IList<IDev2Definition> Definitions { get; set; }

        /// <summary>
        /// Generates this instance.
        /// </summary>
        /// <returns></returns>
        public string Generate()
        {
            StringBuilder result = new StringBuilder();

            XmlDocument xDoc = new XmlDocument();
            XmlElement rootNode = xDoc.CreateElement((string.Concat(ArgumentType.ToString(), "s")));


            foreach(IDev2Definition def in Definitions)
            {
                XmlElement tmp = xDoc.CreateElement(ArgumentType.ToString());
                tmp.SetAttribute(_nameAttribute, def.Name);

                if(ArgumentType != enDev2ArgumentType.Input)
                {
                    tmp.SetAttribute(_mapsToAttribute, def.MapsTo);
                }
                else
                {
                    tmp.SetAttribute(_sourceAttribute, def.MapsTo);
                }

                if(ArgumentType != enDev2ArgumentType.Input)
                {
                    tmp.SetAttribute(_valueAttribute, def.Value);
                }

                if(def.RecordSetName.Length > 0)
                {
                    tmp.SetAttribute(_recordsetAttribute, def.RecordSetName);
                }

                if(def.DefaultValue.Length > 0 && ArgumentType == enDev2ArgumentType.Input)
                {
                    tmp.SetAttribute(_defaultValueAttribute, def.DefaultValue);
                }

                if(def.IsRequired && ArgumentType == enDev2ArgumentType.Input)
                {
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
