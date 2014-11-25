
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.DataList.Contract
// ReSharper restore CheckNamespace
{
    public abstract class LanguageParser
    {

        private readonly string _elementTag;
        const string _nameAttribute = "Name";
        private readonly string _mapsToAttribute;
        private readonly bool _defaultValueToMapsTo;
        const string _recordSetAttribute = "Recordset";
        const string _defaultValueAttribute = "DefaultValue";
        const string _emptyToNullAttribute = "EmptyToNull";
        const string _validateTag = "Validator";
        const string _validateTypeAttribute = "Type";
        const string _requiredValidationAttributeValue = "Required";
        const string _valueTag = "Value";
        const string _magicEval = "[[";
        const string _outputMapsToAdjust = "Name";

        internal LanguageParser(string elementTag, string mapsTo, bool defaultValueToMapsTo)
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
                XmlDocument xDoc = new XmlDocument();

                xDoc.LoadXml(mappingDefinition);

                XmlNodeList tmpList = xDoc.GetElementsByTagName(_elementTag);

                for(int i = 0; i < tmpList.Count; i++)
                {
                    XmlNode tmp = tmpList[i];
                    string value = string.Empty;
                    string origValue = string.Empty;

                    XmlNode valueNode = tmp.Attributes[_valueTag];
                    if(valueNode != null)
                    {
                        value = valueNode.Value;
                        origValue = value;
                    }

                    // is it evaluated?
                    bool isEvaluated = false;
                    string mapsTo = tmp.Attributes[_mapsToAttribute].Value;


                    if(!_defaultValueToMapsTo)
                    { // output

                        // account for blank mapsto in generated output defs
                        if(mapsTo == string.Empty)
                        {
                            XmlNode xn = tmp.Attributes[_outputMapsToAdjust];
                            if(xn != null)
                            {
                                mapsTo = xn.Value;
                            }
                        }

                        if(mapsTo.Contains(_magicEval))
                        {
                            isEvaluated = true;
                            mapsTo = mapsTo.Replace(_magicEval, "").Replace("]]", "");
                        }
                        if(value.Contains(_magicEval))
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
                        if(value.Contains(_magicEval))
                        {
                            isEvaluated = true;
                            value = value.Replace(_magicEval, "").Replace("]]", "");
                            mapsTo = value;
                        }
                    }

                    // extract default value if present
                    string defaultValue = string.Empty;

                    XmlNode defaultValNode = tmp.Attributes[_defaultValueAttribute];
                    if(defaultValNode != null)
                    {
                        defaultValue = defaultValNode.Value;
                    }

                    // extract isRequired
                    bool isRequired = false;

                    XmlNodeList nl = tmp.ChildNodes;
                    if(nl.Count > 0)
                    {
                        int pos = 0;
                        while(pos < nl.Count && !isRequired)
                        {
                            if(nl[pos].Name == _validateTag)
                            {
                                XmlNode requiredValidationNode = nl[pos].Attributes[_validateTypeAttribute];
                                if(requiredValidationNode != null && requiredValidationNode.Value == _requiredValidationAttributeValue)
                                {
                                    isRequired = true;
                                }
                            }
                            pos++;
                        }
                    }

                    // extract EmptyToNull
                    bool emptyToNull = false;
                    XmlNode emptyNode = tmp.Attributes[_emptyToNullAttribute];
                    if(emptyNode != null)
                    {
                        Boolean.TryParse(emptyNode.Value, out emptyToNull);
                    }

                    // only create if mapsTo is not blank!!
                    if((!ignoreBlanks || (mapsTo != string.Empty && value != string.Empty)) || _defaultValueToMapsTo)
                    {
                        if(!_defaultValueToMapsTo) // Outputs only
                        {
                            if(String.IsNullOrEmpty(mapsTo))
                            {
                                continue;
                            }
                        }
                        XmlNode recordSetNode = tmp.Attributes[_recordSetAttribute];

                        if(recordSetNode != null)
                        {
                            // check for recordsets on input mapping

                            var theName = tmp.Attributes[_nameAttribute].Value;
                            if(_defaultValueToMapsTo)
                            {
                                string recordSet = recordSetNode.Value;
                                // we have a recordset set it as such
                                result.Add(DataListFactory.CreateDefinition(theName, mapsTo, value, recordSet, isEvaluated, defaultValue, isRequired, origValue, emptyToNull));
                            }
                            else
                            {
                                // if record set add as such
                                string recordSet = recordSetNode.Value;
                                result.Add(DataListFactory.CreateDefinition(tmp.Attributes[_nameAttribute].Value, mapsTo, value, recordSet, isEvaluated, defaultValue, isRequired, origValue, emptyToNull));
                            }
                        }
                        else
                        {

                            result.Add(DataListFactory.CreateDefinition(tmp.Attributes[_nameAttribute].Value, mapsTo, value, isEvaluated, defaultValue, isRequired, origValue, emptyToNull));
                        }
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
