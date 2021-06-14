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
using System.Linq;
using System.Reflection;
using Warewolf.Resource.Messages;

namespace Dev2.Data.Decisions.Operations
{
    public class DecisionTypeDisplayValue : Attribute
    {
        internal DecisionTypeDisplayValue(string displayValue)
        {
            DisplayValue = displayValue;
        }

        public string DisplayValue { get; }
    }    
    
    public static class DecisionDisplayHelper
    {
        public static string GetDisplayValue(EnDecisionType typeOf)
        {

            MemberInfo mi = typeof(EnDecisionType).GetField(Enum.GetName(typeof(EnDecisionType), typeOf));

            var attr = (DecisionTypeDisplayValue)Attribute.GetCustomAttribute(mi, typeof(DecisionTypeDisplayValue));

            return attr.DisplayValue;
        }
        
        public static EnDecisionType GetValue(string displayValue)
        {
            var values = Enum.GetValues(typeof(EnDecisionType));
            return (from object value in values let mi = typeof(EnDecisionType).GetField(Enum.GetName(typeof(EnDecisionType), value)) let attr = (DecisionTypeDisplayValue)Attribute.GetCustomAttribute(mi, typeof(DecisionTypeDisplayValue)) where attr.DisplayValue.Equals(displayValue) select value as EnDecisionType? ?? EnDecisionType.Choose).FirstOrDefault();
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        public static string GetFailureMessage(EnDecisionType decisionType)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            switch (decisionType)
            {
                case EnDecisionType.Choose: return Messages.Test_FailureMessage_Choose;
                case EnDecisionType.IsError: return Messages.Test_FailureMessage_IsError;
                case EnDecisionType.IsNotError: return Messages.Test_FailureMessage_IsNotError;
                case EnDecisionType.IsNull: return Messages.Test_FailureMessage_IsNull;
                case EnDecisionType.IsNotNull: return Messages.Test_FailureMessage_IsNotNull;
                case EnDecisionType.IsNumeric: return Messages.Test_FailureMessage_IsNumeric;
                case EnDecisionType.IsNotNumeric: return Messages.Test_FailureMessage_IsNotNumeric;
                case EnDecisionType.IsText: return Messages.Test_FailureMessage_IsText;
                case EnDecisionType.IsNotText: return Messages.Test_FailureMessage_IsNotText;
                case EnDecisionType.IsAlphanumeric: return Messages.Test_FailureMessage_IsAlphanumeric;
                case EnDecisionType.IsNotAlphanumeric: return Messages.Test_FailureMessage_IsNotAlphanumeric;
                case EnDecisionType.IsXml: return Messages.Test_FailureMessage_IsXML;
                case EnDecisionType.IsNotXml: return Messages.Test_FailureMessage_IsNotXML;
                case EnDecisionType.IsDate: return Messages.Test_FailureMessage_IsDate;
                case EnDecisionType.IsNotDate: return Messages.Test_FailureMessage_IsNotDate;
                case EnDecisionType.IsEmail: return Messages.Test_FailureMessage_IsEmail;
                case EnDecisionType.IsNotEmail: return Messages.Test_FailureMessage_IsNotEmail;
                case EnDecisionType.IsRegEx: return Messages.Test_FailureMessage_IsRegEx;
                case EnDecisionType.NotRegEx: return Messages.Test_FailureMessage_NotRegEx;
                case EnDecisionType.IsEqual: return Messages.Test_FailureMessage_Equals;
                case EnDecisionType.IsNotEqual: return Messages.Test_FailureMessage_IsNotEqual;
                case EnDecisionType.IsLessThan: return Messages.Test_FailureMessage_IsLessThan;
                case EnDecisionType.IsLessThanOrEqual: return Messages.Test_FailureMessage_IsLessThanOrEqual;
                case EnDecisionType.IsGreaterThan: return Messages.Test_FailureMessage_IsGreaterThan;
                case EnDecisionType.IsGreaterThanOrEqual: return Messages.Test_FailureMessage_IsGreaterThanOrEqual;
                case EnDecisionType.IsContains: return Messages.Test_FailureMessage_IsContains;
                case EnDecisionType.NotContain: return Messages.Test_FailureMessage_NotContain;
                case EnDecisionType.IsEndsWith: return Messages.Test_FailureMessage_IsEndsWith;
                case EnDecisionType.NotEndsWith: return Messages.Test_FailureMessage_NotEndsWith;
                case EnDecisionType.IsStartsWith: return Messages.Test_FailureMessage_IsStartsWith;
                case EnDecisionType.NotStartsWith: return Messages.Test_FailureMessage_NotStartsWith;
                case EnDecisionType.IsBetween: return Messages.Test_FailureMessage_IsBetween;
                case EnDecisionType.NotBetween: return Messages.Test_FailureMessage_NotBetween;
                case EnDecisionType.IsBinary: return Messages.Test_FailureMessage_IsBinary;
                case EnDecisionType.IsNotBinary: return Messages.Test_FailureMessage_IsNotBinary;
                case EnDecisionType.IsHex: return Messages.Test_FailureMessage_IsHex;
                case EnDecisionType.IsNotHex: return Messages.Test_FailureMessage_IsNotHex;
                case EnDecisionType.IsBase64: return Messages.Test_FailureMessage_IsBase64;
                case EnDecisionType.IsNotBase64: return Messages.Test_FailureMessage_IsNotBase64;
                default: return string.Empty;
            }
        }
    }
    
    public enum EnDecisionType
    {
        [DecisionTypeDisplayValue("Not a Valid Decision Type")] Choose,
        [DecisionTypeDisplayValue("There is An Error")] IsError,
        [DecisionTypeDisplayValue("There is No Error")] IsNotError,
        [DecisionTypeDisplayValue("Is NULL")] IsNull,
        [DecisionTypeDisplayValue("Is Not NULL")] IsNotNull,
        [DecisionTypeDisplayValue("Is Numeric")] IsNumeric,
        [DecisionTypeDisplayValue("Not Numeric")] IsNotNumeric,
        [DecisionTypeDisplayValue("Is Text")] IsText,
        [DecisionTypeDisplayValue("Not Text")] IsNotText,
        [DecisionTypeDisplayValue("Is Alphanumeric")] IsAlphanumeric,
        [DecisionTypeDisplayValue("Not Alphanumeric")] IsNotAlphanumeric,
        [DecisionTypeDisplayValue("Is XML")] IsXml,
        [DecisionTypeDisplayValue("Not XML")] IsNotXml,
        [DecisionTypeDisplayValue("Is Date")] IsDate,
        [DecisionTypeDisplayValue("Not Date")] IsNotDate,
        [DecisionTypeDisplayValue("Is Email")] IsEmail,
        [DecisionTypeDisplayValue("Not Email")] IsNotEmail,
        [DecisionTypeDisplayValue("Is Regex")] IsRegEx,
        [DecisionTypeDisplayValue("Not Regex")] NotRegEx,
        [DecisionTypeDisplayValue("=")] IsEqual,
        [DecisionTypeDisplayValue("<> (Not Equal)")] IsNotEqual,
        [DecisionTypeDisplayValue("<")] IsLessThan,
        [DecisionTypeDisplayValue("<=")] IsLessThanOrEqual,
        [DecisionTypeDisplayValue(">")] IsGreaterThan,
        [DecisionTypeDisplayValue(">=")] IsGreaterThanOrEqual,
        [DecisionTypeDisplayValue("Contains")] IsContains,
        [DecisionTypeDisplayValue("Doesn't Contain")] NotContain,
        [DecisionTypeDisplayValue("Ends With")] IsEndsWith,
        [DecisionTypeDisplayValue("Doesn't End With")] NotEndsWith,
        [DecisionTypeDisplayValue("Starts With")] IsStartsWith,
        [DecisionTypeDisplayValue("Doesn't Start With")] NotStartsWith,
        [DecisionTypeDisplayValue("Is Between")] IsBetween,
        [DecisionTypeDisplayValue("Not Between")] NotBetween,
        [DecisionTypeDisplayValue("Is Binary")] IsBinary,
        [DecisionTypeDisplayValue("Not Binary")] IsNotBinary,
        [DecisionTypeDisplayValue("Is Hex")] IsHex,
        [DecisionTypeDisplayValue("Not Hex")] IsNotHex,
        [DecisionTypeDisplayValue("Is Base64")] IsBase64,
        [DecisionTypeDisplayValue("Not Base64")] IsNotBase64
    }
}
