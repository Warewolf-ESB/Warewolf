/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using System.Reflection;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable InconsistentNaming

namespace Dev2.Data.Decisions.Operations
{
    /// <summary>
    /// Used to annotate DecisionTypes with user friendly values
    /// </summary>
    public class DecisionTypeDisplayValue : Attribute
    {

        internal DecisionTypeDisplayValue(string displayValue)
        {
            DisplayValue = displayValue;
        }

        public string DisplayValue { get; set; }
    }    

    /// <summary>
    /// Used to extract the display value for a decision enum
    /// </summary>
    public static class DecisionDisplayHelper
    {
        public static string GetDisplayValue(enDecisionType typeOf)
        {

            MemberInfo mi = typeof(enDecisionType).GetField(Enum.GetName(typeof(enDecisionType), typeOf));

            DecisionTypeDisplayValue attr = (DecisionTypeDisplayValue)Attribute.GetCustomAttribute(mi, typeof(DecisionTypeDisplayValue));

            return attr.DisplayValue;
        }
        
        public static enDecisionType GetValue(string displayValue)
        {
            var values = Enum.GetValues(typeof(enDecisionType));
            return (from object value in values let mi = typeof(enDecisionType).GetField(Enum.GetName(typeof(enDecisionType), value)) let attr = (DecisionTypeDisplayValue)Attribute.GetCustomAttribute(mi, typeof(DecisionTypeDisplayValue)) where attr.DisplayValue.Equals(displayValue) select value as enDecisionType? ?? enDecisionType.Choose).FirstOrDefault();
        }

        public static string GetFailureMessage(enDecisionType decisionType)
        {
            // ReSharper disable once RedundantAssignment
            string errorMsg = string.Empty;
            switch (decisionType)
            {
                case enDecisionType.Choose:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_Choose;
                    break;
                case enDecisionType.IsError:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsError;
                    break;
                case enDecisionType.IsNotError:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotError;
                    break;
                case enDecisionType.IsNull:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNull;
                    break;
                case enDecisionType.IsNotNull:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotNull;
                    break;
                case enDecisionType.IsNumeric:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNumeric;
                    break;
                case enDecisionType.IsNotNumeric:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotNumeric;
                    break;
                case enDecisionType.IsText:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsText;
                    break;
                case enDecisionType.IsNotText:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotText;
                    break;
                case enDecisionType.IsAlphanumeric:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsAlphanumeric;
                    break;
                case enDecisionType.IsNotAlphanumeric:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotAlphanumeric;
                    break;
                case enDecisionType.IsXML:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsXML;
                    break;
                case enDecisionType.IsNotXML:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotXML;
                    break;
                case enDecisionType.IsDate:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsDate;
                    break;
                case enDecisionType.IsNotDate:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotDate;
                    break;
                case enDecisionType.IsEmail:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsEmail;
                    break;
                case enDecisionType.IsNotEmail:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotEmail;
                    break;
                case enDecisionType.IsRegEx:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsRegEx;
                    break;
                case enDecisionType.NotRegEx:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_NotRegEx;
                    break;
                case enDecisionType.IsEqual:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_Equals;
                    break;
                case enDecisionType.IsNotEqual:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotEqual;
                    break;
                case enDecisionType.IsLessThan:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsLessThan;
                    break;
                case enDecisionType.IsLessThanOrEqual:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsLessThanOrEqual;
                    break;
                case enDecisionType.IsGreaterThan:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsGreaterThan;
                    break;
                case enDecisionType.IsGreaterThanOrEqual:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsGreaterThanOrEqual;
                    break;
                case enDecisionType.IsContains:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsContains;
                    break;
                case enDecisionType.NotContain:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_NotContain;
                    break;
                case enDecisionType.IsEndsWith:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsEndsWith;
                    break;
                case enDecisionType.NotEndsWith:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_NotEndsWith;
                    break;
                case enDecisionType.IsStartsWith:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsStartsWith;
                    break;
                case enDecisionType.NotStartsWith:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_NotStartsWith;
                    break;
                case enDecisionType.IsBetween:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsBetween;
                    break;
                case enDecisionType.NotBetween:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_NotBetween;
                    break;
                case enDecisionType.IsBinary:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsBinary;
                    break;
                case enDecisionType.IsNotBinary:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotBinary;
                    break;
                case enDecisionType.IsHex:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsHex;
                    break;
                case enDecisionType.IsNotHex:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotHex;
                    break;
                case enDecisionType.IsBase64:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsBase64;
                    break;
                case enDecisionType.IsNotBase64:
                    errorMsg = Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotBase64;
                    break;
            }
            return errorMsg;
        }
    }

    /// <summary>
    /// Decision types for the wizard
    /// </summary>
    public enum enDecisionType
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
        [DecisionTypeDisplayValue("Is XML")] IsXML,
        [DecisionTypeDisplayValue("Not XML")] IsNotXML,
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
