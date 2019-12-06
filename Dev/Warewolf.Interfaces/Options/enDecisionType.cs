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

namespace Warewolf.Options
{
    public class DecisionTypeDisplayValue : Attribute
    {
        internal DecisionTypeDisplayValue(string displayValue)
        {
            DisplayValue = displayValue;
        }

        public string DisplayValue { get; set; }
    }

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

    public static class enDecisionTypeExtensionMethods
    {
        public static bool IsTripleOperand(this enDecisionType decisionType)
        {
            return
                decisionType == enDecisionType.IsBetween ||
                decisionType == enDecisionType.NotBetween;
        }
        public static bool IsSingleOperand(this enDecisionType decisionType)
        {
            return
                decisionType == enDecisionType.Choose ||
                decisionType == enDecisionType.IsError ||
                decisionType == enDecisionType.IsNotError ||
                decisionType == enDecisionType.IsNull ||
                decisionType == enDecisionType.IsNotNull ||
                decisionType == enDecisionType.IsNumeric ||
                decisionType == enDecisionType.IsNotNumeric ||
                decisionType == enDecisionType.IsText ||
                decisionType == enDecisionType.IsNotText ||
                decisionType == enDecisionType.IsAlphanumeric ||
                decisionType == enDecisionType.IsNotAlphanumeric ||
                decisionType == enDecisionType.IsXML ||
                decisionType == enDecisionType.IsNotXML ||
                decisionType == enDecisionType.IsDate ||
                decisionType == enDecisionType.IsNotDate ||
                decisionType == enDecisionType.IsEmail ||
                decisionType == enDecisionType.IsNotEmail ||
                decisionType == enDecisionType.IsRegEx ||
                decisionType == enDecisionType.NotRegEx ||
                decisionType == enDecisionType.IsBinary ||
                decisionType == enDecisionType.IsNotBinary ||
                decisionType == enDecisionType.IsHex ||
                decisionType == enDecisionType.IsNotHex ||
                decisionType == enDecisionType.IsBase64 ||
                decisionType == enDecisionType.IsNotBase64
            ;
        }
    }
}
