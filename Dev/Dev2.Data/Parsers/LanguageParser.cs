#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
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

        #region Methods

        internal IList<IDev2Definition> Parse(string mappingDefinition, bool ignoreBlanks = true)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();

            if(!string.IsNullOrEmpty(mappingDefinition))
            {
                ParseMappingDefinition(mappingDefinition, ignoreBlanks, result);
            }

            return result;
        }

        void ParseMappingDefinition(string mappingDefinition, bool ignoreBlanks, IList<IDev2Definition> result)
        {
            var xDoc = new XmlDocument();

            xDoc.LoadXml(mappingDefinition);

            var tmpList = xDoc.GetElementsByTagName(_elementTag);

            foreach (XmlNode tmp in tmpList)
            {
                var value = string.Empty;
                var origValue = string.Empty;

                XmlNode valueNode = tmp.Attributes[_valueTag];
                if (valueNode != null)
                {
                    value = valueNode.Value;
                    origValue = value;
                }

                // is it evaluated?
                var isEvaluated = false;
                var mapsTo = tmp.Attributes[_mapsToAttribute].Value;

                if (tmp.Attributes["IsObject"] == null || !bool.TryParse(tmp.Attributes["IsObject"].Value, out bool isObject))
                {
                    isObject = false;
                }

                if (!_defaultValueToMapsTo)
                { // output

                    // account for blank mapsto in generated output defs
                    if (mapsTo == string.Empty)
                    {
                        mapsTo = GetMapsTo(tmp);
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

                XmlNode defaultValNode = tmp.Attributes[_defaultValueAttribute];
                if (defaultValNode != null)
                {
                    defaultValue = defaultValNode.Value;
                }

                // extract isRequired
                var isRequired = false;

                var nl = tmp.ChildNodes;
                if (nl.Count > 0)
                {
                    isRequired = GetChildNodes(isRequired, nl);
                }

                // extract EmptyToNull
                var emptyToNull = false;
                XmlNode emptyNode = tmp.Attributes[_emptyToNullAttribute];
                if (emptyNode != null)
                {
                    Boolean.TryParse(emptyNode.Value, out emptyToNull);
                }

                // only create if mapsTo is not blank!!
                if (!ignoreBlanks || mapsTo != string.Empty && value != string.Empty || _defaultValueToMapsTo)
                {
                    if (!_defaultValueToMapsTo && String.IsNullOrEmpty(mapsTo)) // Outputs only
                    {
                        continue;
                    }

                    XmlNode recordSetNode = tmp.Attributes[_recordSetAttribute];

                    if (recordSetNode != null)
                    {
                        CheckForRecordsetsInInputMapping(result, tmp, value, origValue, isEvaluated, mapsTo, defaultValue, isRequired, emptyToNull, recordSetNode);
                    }
                    else
                    {
                        var dev2Definition = Dev2Definition.NewObject(tmp.Attributes[_nameAttribute].Value, mapsTo, value, isEvaluated, defaultValue, isRequired, origValue, emptyToNull);
                        dev2Definition.IsObject = isObject;
                        result.Add(dev2Definition);
                    }
                }
            }
        }

        private void CheckForRecordsetsInInputMapping(IList<IDev2Definition> result, XmlNode tmp, string value, string origValue, bool isEvaluated, string mapsTo, string defaultValue, bool isRequired, bool emptyToNull, XmlNode recordSetNode)
        {
            var theName = tmp.Attributes[_nameAttribute].Value;
            if (_defaultValueToMapsTo)
            {
                var recordSet = recordSetNode.Value;
                // we have a recordset set it as such
                result.Add(DataListFactory.CreateDefinition_Recordset(theName, mapsTo, value, recordSet, isEvaluated, defaultValue, isRequired, origValue, emptyToNull));
            }
            else
            {
                // if record set add as such
                var recordSet = recordSetNode.Value;
                result.Add(DataListFactory.CreateDefinition_Recordset(tmp.Attributes[_nameAttribute].Value, mapsTo, value, recordSet, isEvaluated, defaultValue, isRequired, origValue, emptyToNull));
            }
        }

        private static bool GetChildNodes(bool isRequired, XmlNodeList nl)
        {
            var pos = 0;
            while (pos < nl.Count && !isRequired)
            {
                if (nl[pos].Name == _validateTag)
                {
                    XmlNode requiredValidationNode = nl[pos].Attributes[_validateTypeAttribute];
                    if (requiredValidationNode != null && requiredValidationNode.Value == _requiredValidationAttributeValue)
                    {
                        isRequired = true;
                    }
                }
                pos++;
            }

            return isRequired;
        }

        static string GetMapsTo(XmlNode tmp)
        {
            string mapsTo = "";
            XmlNode xn = tmp.Attributes[_outputMapsToAdjust];
            if (xn != null)
            {
                mapsTo = xn.Value;
            }

            return mapsTo;
        }

        #endregion
    }
}
