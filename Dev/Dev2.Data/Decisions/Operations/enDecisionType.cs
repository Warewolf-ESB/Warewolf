
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
using System.Reflection;

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
    }

    /// <summary>
    /// Decision types for the wizard
    /// </summary>
    public enum enDecisionType
    {
        [DecisionTypeDisplayValue("Not a Valid Decision Type")]
        Choose,
        [DecisionTypeDisplayValue("There Is An Error")]
        IsError,
        [DecisionTypeDisplayValue("There Is Not An Error")]
        IsNotError,
        [DecisionTypeDisplayValue("Is Numeric")]
        IsNumeric,
        [DecisionTypeDisplayValue("Not Numeric")]
        IsNotNumeric,
        [DecisionTypeDisplayValue("Is Text")]
        IsText,
        [DecisionTypeDisplayValue("Not Text")]
        IsNotText,
        [DecisionTypeDisplayValue("Is Alphanumeric")]
        IsAlphanumeric,
        [DecisionTypeDisplayValue("Not Alphanumeric")]
        IsNotAlphanumeric,
        [DecisionTypeDisplayValue("Is XML")]
        IsXML,
        [DecisionTypeDisplayValue("Not XML")]
        IsNotXML,
        [DecisionTypeDisplayValue("Is Date")]
        IsDate,
        [DecisionTypeDisplayValue("Not Date")]
        IsNotDate,
        [DecisionTypeDisplayValue("Is Email")]
        IsEmail,
        [DecisionTypeDisplayValue("Not Email")]
        IsNotEmail,
        [DecisionTypeDisplayValue("Is Regular Expression")]
        IsRegEx,
        [DecisionTypeDisplayValue("Not Regular Expression")]
        NotRegEx,
        [DecisionTypeDisplayValue("=")]
        IsEqual,
        [DecisionTypeDisplayValue("<> (Not Equal)")]
        IsNotEqual,
        [DecisionTypeDisplayValue("<")]
        IsLessThan,
        [DecisionTypeDisplayValue("<=")]
        IsLessThanOrEqual,
        [DecisionTypeDisplayValue(">")]
        IsGreaterThan,
        [DecisionTypeDisplayValue(">=")]
        IsGreaterThanOrEqual,
        [DecisionTypeDisplayValue("Contains")]
        IsContains,
        [DecisionTypeDisplayValue("Doesn't Contain")]
        NotContain,
        [DecisionTypeDisplayValue("Ends With")]
        IsEndsWith,
        [DecisionTypeDisplayValue("Doesn't End With")]
        NotEndsWith,
        [DecisionTypeDisplayValue("Starts With")]
        IsStartsWith,
        [DecisionTypeDisplayValue("Doesn't Start With")]
        NotStartsWith,
        [DecisionTypeDisplayValue("Is Between")]
        IsBetween,
        [DecisionTypeDisplayValue("Not Between")]
        NotBetween,
        [DecisionTypeDisplayValue("Is Binary")]
        IsBinary,
        [DecisionTypeDisplayValue("Not Binary")]
        IsNotBinary,
        [DecisionTypeDisplayValue("Is Hex")]
        IsHex,
        [DecisionTypeDisplayValue("Not Hex")]
        IsNotHex,
        [DecisionTypeDisplayValue("Is Base64")]
        IsBase64,
        [DecisionTypeDisplayValue("Not Base64")]
        IsNotBase64
    }
}
