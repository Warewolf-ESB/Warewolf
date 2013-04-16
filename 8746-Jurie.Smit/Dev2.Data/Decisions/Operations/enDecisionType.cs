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
        //private static int _size = 30;
        //private static IDictionary _cache = new Dictionary<enDecisionType,string>(_size);

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
        [DecisionTypeDisplayValue("Is Not Numeric")]
        IsNotNumeric,
        [DecisionTypeDisplayValue("Is Text")]
        IsText,
        [DecisionTypeDisplayValue("Is Not Text")]
        IsNotText,
        [DecisionTypeDisplayValue("Is Alphanumeric")]
        IsAlphanumeric,
        [DecisionTypeDisplayValue("Is Not Alphanumeric")]
        IsNotAlphanumeric,
        [DecisionTypeDisplayValue("Is XML")]
        IsXML,
        [DecisionTypeDisplayValue("Is Not XML")]
        IsNotXML,
        [DecisionTypeDisplayValue("Is Date")]
        IsDate,
        [DecisionTypeDisplayValue("Is Not Date")]
        IsNotDate,
        [DecisionTypeDisplayValue("Is Email")]
        IsEmail,
        [DecisionTypeDisplayValue("Is Not Email")]
        IsNotEmail,
        [DecisionTypeDisplayValue("Is Regular Expression")]
        IsRegEx,
        [DecisionTypeDisplayValue("Is Equal")]
        IsEqual,
        [DecisionTypeDisplayValue("Is Not Equal")]
        IsNotEqual,
        [DecisionTypeDisplayValue("Is Less Than")]
        IsLessThan,
        [DecisionTypeDisplayValue("Is Less Than Or Equal To")]
        IsLessThanOrEqual,
        [DecisionTypeDisplayValue("Is Greater Than")]
        IsGreaterThan,
        [DecisionTypeDisplayValue("Is Greater Than Or Equal To")]
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
        [DecisionTypeDisplayValue("Is Not Binary")]
        IsNotBinary,
        [DecisionTypeDisplayValue("Is Hex")]
        IsHex,
        [DecisionTypeDisplayValue("Is Not Hex")]
        IsNotHex,
        [DecisionTypeDisplayValue("Is Base64")]
        IsBase64,
        [DecisionTypeDisplayValue("Is Not Base64")]
        IsNotBase64
    }
}
