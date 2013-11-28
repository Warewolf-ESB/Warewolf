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
        [DecisionTypeDisplayValue("=")]
        IsEqual,
        [DecisionTypeDisplayValue("≠")]
        IsNotEqual,
        [DecisionTypeDisplayValue("<")]
        IsLessThan,
        [DecisionTypeDisplayValue("≤")]
        IsLessThanOrEqual,
        [DecisionTypeDisplayValue(">")]
        IsGreaterThan,
        [DecisionTypeDisplayValue("≥")]
        IsGreaterThanOrEqual,
        [DecisionTypeDisplayValue("Contains")]
        IsContains,
        [DecisionTypeDisplayValue("Ends With")]
        IsEndsWith,
        [DecisionTypeDisplayValue("Starts With")]
        IsStartsWith,
        [DecisionTypeDisplayValue("Is Between")]
        IsBetween,
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
