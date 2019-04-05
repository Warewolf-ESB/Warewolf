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
using System.Xml;
using Dev2.Common.Interfaces.Data;

namespace Dev2.DataList.Contract
{
    public abstract class LanguageParser
    {
        readonly string _elementTag;
        const string _nameAttribute = "Name";
        readonly string _mapsToAttribute;
        readonly bool _defaultValueToMapsTo;
        const string _recordSetAttribute = "Recordset";
        const string _defaultValueAttribute = "DefaultValue";
        const string _emptyToNullAttribute = "EmptyToNull";
        const string _validateTag = "Validator";
        const string _validateTypeAttribute = "Type";
        const string _requiredValidationAttributeValue = "Required";
        const string _valueTag = "Value";
        const string _magicEval = "[[";
        const string _outputMapsToAdjust = "Name";

        protected LanguageParser(string elementTag, string mapsTo, bool defaultValueToMapsTo)
        {
            _elementTag = elementTag;
            _mapsToAttribute = mapsTo;
            _defaultValueToMapsTo = defaultValueToMapsTo;
        }

        internal IList<IDev2Definition> Parse(string mappingDefinition, bool ignoreBlanks = true)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();

            if (!string.IsNullOrEmpty(mappingDefinition))
            {
                ParseMappingDefinition(mappingDefinition, ignoreBlanks, result);
            }

            return result;
        }

        void ParseMappingDefinition(string mappingDefinition, bool ignoreBlanks, IList<IDev2Definition> result)
        {
            var xDoc = new XmlDocument();

            xDoc.LoadXml(mappingDefinition);

            var nodeList = xDoc.GetElementsByTagName(_elementTag);

            foreach (XmlNode node in nodeList)
            {
                var value = string.Empty;
                var origValue = string.Empty;

                XmlNode valueNode = node.Attributes[_valueTag];
                if (valueNode != null)
                {
                    value = valueNode.Value;
                    origValue = value;
                }

                // is it evaluated?
                var isEvaluated = false;
                var mapsTo = node.Attributes[_mapsToAttribute].Value;

                if (node.Attributes["IsObject"] == null || !bool.TryParse(node.Attributes["IsObject"].Value, out bool isObject))
                {
                    isObject = false;
                }

                if (!_defaultValueToMapsTo)
                { 
                    // output
                    // account for blank mapsto in generated output defs
                    if (mapsTo == string.Empty)
                    {
                        mapsTo = GetMapsTo(node);
                    }

                    if (mapsTo.Contains(_magicEval))
                    {
                        isEvaluated = true;
                        mapsTo = mapsTo.Replace(_magicEval, "").Replace("]]", "");
                    }
                    if (value.Contains(_magicEval))
                    {
                        value = value.Replace(_magicEval, "").Replace("]]", "");
                        isEvaluated = true;
                    }
                }
                else
                {
                    // input
                    origValue = mapsTo;
                    value = mapsTo;
                    if (value.Contains(_magicEval))
                    {
                        isEvaluated = true;
                        value = value.Replace(_magicEval, "").Replace("]]", "");
                        mapsTo = value;
                    }
                }

                // extract default value if present
                var defaultValue = string.Empty;

                XmlNode defaultValNode = node.Attributes[_defaultValueAttribute];
                if (defaultValNode != null)
                {
                    defaultValue = defaultValNode.Value;
                }

                // extract isRequired
                var isRequired = IsRequired(node.ChildNodes);

                // extract EmptyToNull
                var emptyToNull = false;
                XmlNode emptyNode = node.Attributes[_emptyToNullAttribute];
                if (emptyNode != null)
                {
                    bool.TryParse(emptyNode.Value, out emptyToNull);
                }

                // only create if mapsTo is not blank!!
                if (!ignoreBlanks || mapsTo != string.Empty && value != string.Empty || _defaultValueToMapsTo)
                {
                    if (!_defaultValueToMapsTo && string.IsNullOrEmpty(mapsTo)) // Outputs only
                    {
                        continue;
                    }

                    XmlNode recordSetNode = node.Attributes[_recordSetAttribute];

                    if (recordSetNode != null)
                    {
                        result.Add(DataListFactory.CreateDefinition_Recordset(node.Attributes[_nameAttribute].Value, mapsTo, value, recordSetNode.Value, isEvaluated, defaultValue, isRequired, origValue, emptyToNull));
                    }
                    else
                    {
                        var dev2Definition = Dev2Definition.NewObject(node.Attributes[_nameAttribute].Value, mapsTo, value, isEvaluated, defaultValue, isRequired, origValue, emptyToNull);
                        dev2Definition.IsObject = isObject;
                        result.Add(dev2Definition);
                    }
                }
            }
        }

        private static bool IsRequired(XmlNodeList childNodes)
        {
            if (childNodes is null || childNodes.Count <= 0)
            {
                return false;
            }

            var pos = 0;
            while (pos < childNodes.Count)
            {
                if (childNodes[pos].Name == _validateTag)
                {
                    XmlNode requiredValidationNode = childNodes[pos].Attributes[_validateTypeAttribute];
                    if (requiredValidationNode != null && requiredValidationNode.Value == _requiredValidationAttributeValue)
                    {
                        return true;
                    }
                }
                pos++;
            }

            return false;
        }

        static string GetMapsTo(XmlNode tmp)
        {
            var mapsTo = "";
            XmlNode xn = tmp.Attributes[_outputMapsToAdjust];
            if (xn != null)
            {
                mapsTo = xn.Value;
            }

            return mapsTo;
        }
    }
}
