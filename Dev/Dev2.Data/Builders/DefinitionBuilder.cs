/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Text;
using System.Xml;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Interfaces.Enums;

namespace Dev2.Data.Builders
{
    public class DefinitionBuilder
    {
        const string NameAttribute = "Name";
        const string MapsToAttribute = "MapsTo";
        const string ValueAttribute = "Value";
        const string RecordsetAttribute = "Recordset";
        const string DefaultValueAttribute = "DefaultValue";
        const string ValidateTag = "Validator";
        const string TypeAttribute = "Type";
        const string RequiredValue = "Required";
        const string SourceAttribute = "Source";

        public enDev2ArgumentType ArgumentType { get; set; }

        public IList<IDev2Definition> Definitions { get; set; }

        public string Generate()
        {
            var result = new StringBuilder();

            var xDoc = new XmlDocument();
            var rootNode = xDoc.CreateElement(string.Concat(ArgumentType.ToString(), "s"));


            foreach (IDev2Definition def in Definitions)
            {
                var tmp = xDoc.CreateElement(ArgumentType.ToString());
                tmp.SetAttribute(NameAttribute, def.Name);

                tmp.SetAttribute(ArgumentType != enDev2ArgumentType.Input ? MapsToAttribute : SourceAttribute, def.MapsTo);

                if(ArgumentType != enDev2ArgumentType.Input)
                {
                    tmp.SetAttribute(ValueAttribute, def.Value);
                }

                tmp.SetAttribute("IsObject", def.IsObject.ToString());

                if(def.RecordSetName.Length > 0)
                {
                    tmp.SetAttribute(RecordsetAttribute, def.RecordSetName);
                }

                if(def.DefaultValue.Length > 0 && ArgumentType == enDev2ArgumentType.Input)
                {
                    tmp.SetAttribute(DefaultValueAttribute, def.DefaultValue);
                }

                if(def.IsRequired && ArgumentType == enDev2ArgumentType.Input)
                {
                    var requiredElm = xDoc.CreateElement(ValidateTag);
                    requiredElm.SetAttribute(TypeAttribute, RequiredValue);
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
