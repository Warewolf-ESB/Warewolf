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
    public class ServiceActivityVariableMapping
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

        protected ServiceActivityVariableMapping(string elementTag, string mapsTo, bool defaultValueToMapsTo)
        {
            _elementTag = elementTag;
            _mapsToAttribute = mapsTo;
            _defaultValueToMapsTo = defaultValueToMapsTo;
        }

        internal IList<IDev2Definition> Parse(string mappingDefinition, bool ignoreBlanks = true)
        {
            if (string.IsNullOrEmpty(mappingDefinition))
            {
                return new List<IDev2Definition>();
            }

            var result = ParseMappingDefinition(mappingDefinition, ignoreBlanks);
            return result;
        }

        string _nodeValue;
        string _originalNodeValue;
        bool _isNodeEvaluated;
        string _nodeMapsTo;

        IList<IDev2Definition> ParseMappingDefinition(string mappingDefinition, bool ignoreBlanks)
        {
            var xDoc = new XmlDocument();
            xDoc.LoadXml(mappingDefinition);
            var nodeList = xDoc.GetElementsByTagName(_elementTag);

            IList<IDev2Definition> result = new List<IDev2Definition>();

            foreach (XmlNode node in nodeList)
            {
                _nodeValue = string.Empty;
                _originalNodeValue = string.Empty;
                _isNodeEvaluated = false;

                XmlNode valueNode = node.Attributes[_valueTag];
                if (valueNode != null)
                {
                    _nodeValue = valueNode.Value;
                    _originalNodeValue = _nodeValue;
                }
                
                _nodeMapsTo = node.Attributes[_mapsToAttribute].Value;

                if (!_defaultValueToMapsTo)
                {
                    ReplaceOutputBrackets(node);
                }
                else
                {
                    ReplaceInputBrackets();
                }

                // only create if mapsTo is not blank!!
                if (!ignoreBlanks || _nodeMapsTo != string.Empty && _nodeValue != string.Empty || _defaultValueToMapsTo)
                {
                    if (!_defaultValueToMapsTo && string.IsNullOrEmpty(_nodeMapsTo)) // Outputs only
                    {
                        continue;
                    }

                    EvaluateOutputMapsTo(result, node);
                }
            }

            return result;
        }

        private void ReplaceOutputBrackets(XmlNode node)
        {
            // account for blank mapsto in generated output defs
            if (_nodeMapsTo == string.Empty)
            {
                _nodeMapsTo = GetMapsTo(node);
            }
            if (_nodeMapsTo.Contains(_magicEval))
            {
                _isNodeEvaluated = true;
                _nodeMapsTo = _nodeMapsTo.Replace(_magicEval, "").Replace("]]", "");
            }
            if (_nodeValue.Contains(_magicEval))
            {
                _nodeValue = _nodeValue.Replace(_magicEval, "").Replace("]]", "");
                _isNodeEvaluated = true;
            }
        }

        private void ReplaceInputBrackets()
        {
            _originalNodeValue = _nodeMapsTo;
            _nodeValue = _nodeMapsTo;
            if (_nodeValue.Contains(_magicEval))
            {
                _isNodeEvaluated = true;
                _nodeValue = _nodeValue.Replace(_magicEval, "").Replace("]]", "");
                _nodeMapsTo = _nodeValue;
            }
        }

        private void EvaluateOutputMapsTo(IList<IDev2Definition> result, XmlNode node)
        {
            // extract default value if present
            var defaultValue = string.Empty;

            XmlNode defaultValNode = node.Attributes[_defaultValueAttribute];
            if (defaultValNode != null)
            {
                defaultValue = defaultValNode.Value;
            }

            // extract EmptyToNull
            var emptyToNull = false;
            XmlNode emptyNode = node.Attributes[_emptyToNullAttribute];
            if (emptyNode != null)
            {
                bool.TryParse(emptyNode.Value, out emptyToNull);
            }

            // extract isRequired
            var isRequired = IsRequired(node.ChildNodes);

            XmlNode recordSetNode = node.Attributes[_recordSetAttribute];

            if (recordSetNode != null)
            {
                result.Add(DataListFactory.CreateDefinition_Recordset(node.Attributes[_nameAttribute].Value, _nodeMapsTo, _nodeValue, recordSetNode.Value, _isNodeEvaluated, defaultValue, isRequired, _originalNodeValue, emptyToNull));
            }
            else
            {
                var dev2Definition = Dev2Definition.NewObject(node.Attributes[_nameAttribute].Value, _nodeMapsTo, _nodeValue, _isNodeEvaluated, defaultValue, isRequired, _originalNodeValue, emptyToNull);

                if (node.Attributes["IsObject"] == null || !bool.TryParse(node.Attributes["IsObject"].Value, out bool isObject))
                {
                    isObject = false;
                }
                dev2Definition.IsObject = isObject;
                result.Add(dev2Definition);
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
