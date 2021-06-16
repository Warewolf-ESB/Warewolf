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
using System.Text;

namespace Warewolf.Options
{
    public class DecisionTypeDisplayValue : Attribute
    {
        public DecisionTypeDisplayValue(string displayValue)
        {
            DisplayValue = displayValue;
        }

        public string DisplayValue { get; }
        public string Get()
        {
            return DisplayValue;
        }
    }
    public class IndexAttribute : Attribute
    {
        private int _index;

        public IndexAttribute(int index)
        {
            this._index = index;
        }

        public int Get()
        {
            return _index;
        }
    }

    public class IndexAttributeException : Exception
    {
        public IndexAttributeException(string message) : base(message)
        {
            
        }
    }

    public enum EnDecisionType
    {
        [Index(20)] [DecisionTypeDisplayValue("Not a Valid Decision Type")] Choose,
        [Index(21)] [DecisionTypeDisplayValue("There is An Error")] IsError,
        [Index(22)] [DecisionTypeDisplayValue("There is No Error")] IsNotError,
        [Index(23)] [DecisionTypeDisplayValue("Is NULL")] IsNull,
        [Index(24)] [DecisionTypeDisplayValue("Is Not NULL")] IsNotNull,
        [Index(25)] [DecisionTypeDisplayValue("Is Numeric")] IsNumeric,
        [Index(26)] [DecisionTypeDisplayValue("Not Numeric")] IsNotNumeric,
        [Index(27)] [DecisionTypeDisplayValue("Is Text")] IsText,
        [Index(28)] [DecisionTypeDisplayValue("Not Text")] IsNotText,
        [Index(29)] [DecisionTypeDisplayValue("Is Alphanumeric")] IsAlphanumeric,
        [Index(30)] [DecisionTypeDisplayValue("Not Alphanumeric")] IsNotAlphanumeric,
        // ReSharper disable InconsistentNaming
        // Disabled because this text is displayed in the decision dialog.
        [Index(31)] [DecisionTypeDisplayValue("Is XML")] IsXML,
        [Index(32)] [DecisionTypeDisplayValue("Not XML")] IsNotXML,
        // ReSharper restore InconsistentNaming
        [Index(33)] [DecisionTypeDisplayValue("Is Date")] IsDate,
        [Index(34)] [DecisionTypeDisplayValue("Not Date")] IsNotDate,
        [Index(35)] [DecisionTypeDisplayValue("Is Email")] IsEmail,
        [Index(36)] [DecisionTypeDisplayValue("Not Email")] IsNotEmail,
        [Index(37)] [DecisionTypeDisplayValue("Is Regex")] IsRegEx,
        [Index(38)] [DecisionTypeDisplayValue("Not Regex")] NotRegEx,
        [Index(0)] [DecisionTypeDisplayValue("=")] IsEqual,
        [Index(1)] [DecisionTypeDisplayValue("<> (Not Equal)")] IsNotEqual,
        [Index(2)] [DecisionTypeDisplayValue("<")] IsLessThan,
        [Index(3)] [DecisionTypeDisplayValue("<=")] IsLessThanOrEqual,
        [Index(4)] [DecisionTypeDisplayValue(">")] IsGreaterThan,
        [Index(5)] [DecisionTypeDisplayValue(">=")] IsGreaterThanOrEqual,
        [Index(6)] [DecisionTypeDisplayValue("Contains")] IsContains,
        [Index(7)] [DecisionTypeDisplayValue("Doesn't Contain")] NotContain,
        [Index(8)] [DecisionTypeDisplayValue("Ends With")] IsEndsWith,
        [Index(9)] [DecisionTypeDisplayValue("Doesn't End With")] NotEndsWith,
        [Index(10)] [DecisionTypeDisplayValue("Starts With")] IsStartsWith,
        [Index(11)] [DecisionTypeDisplayValue("Doesn't Start With")] NotStartsWith,
        [Index(12)] [DecisionTypeDisplayValue("Is Between")] IsBetween,
        [Index(13)] [DecisionTypeDisplayValue("Not Between")] NotBetween,
        [Index(14)] [DecisionTypeDisplayValue("Is Binary")] IsBinary,
        [Index(15)] [DecisionTypeDisplayValue("Not Binary")] IsNotBinary,
        [Index(16)] [DecisionTypeDisplayValue("Is Hex")] IsHex,
        [Index(17)] [DecisionTypeDisplayValue("Not Hex")] IsNotHex,
        [Index(18)] [DecisionTypeDisplayValue("Is Base64")] IsBase64,
        [Index(19)] [DecisionTypeDisplayValue("Not Base64")] IsNotBase64
    }

    public static class enDecisionTypeExtensionMethods
    {
        public static bool IsTripleOperand(this EnDecisionType decisionType)
        {
            return
                decisionType == EnDecisionType.IsBetween ||
                decisionType == EnDecisionType.NotBetween;
        }
        public static bool IsSingleOperand(this EnDecisionType decisionType)
        {
            return
                decisionType == EnDecisionType.Choose ||
                decisionType == EnDecisionType.IsError ||
                decisionType == EnDecisionType.IsNotError ||
                decisionType == EnDecisionType.IsNull ||
                decisionType == EnDecisionType.IsNotNull ||
                decisionType == EnDecisionType.IsNumeric ||
                decisionType == EnDecisionType.IsNotNumeric ||
                decisionType == EnDecisionType.IsText ||
                decisionType == EnDecisionType.IsNotText ||
                decisionType == EnDecisionType.IsAlphanumeric ||
                decisionType == EnDecisionType.IsNotAlphanumeric ||
                decisionType == EnDecisionType.IsXML ||
                decisionType == EnDecisionType.IsNotXML ||
                decisionType == EnDecisionType.IsDate ||
                decisionType == EnDecisionType.IsNotDate ||
                decisionType == EnDecisionType.IsEmail ||
                decisionType == EnDecisionType.IsNotEmail ||
                decisionType == EnDecisionType.IsRegEx ||
                decisionType == EnDecisionType.NotRegEx ||
                decisionType == EnDecisionType.IsBinary ||
                decisionType == EnDecisionType.IsNotBinary ||
                decisionType == EnDecisionType.IsHex ||
                decisionType == EnDecisionType.IsNotHex ||
                decisionType == EnDecisionType.IsBase64 ||
                decisionType == EnDecisionType.IsNotBase64
            ;
        }

        public static void RenderDescription(this EnDecisionType decisionType, StringBuilder sb)
        {
            var name = Enum.GetName(typeof(EnDecisionType), decisionType);
            var memberInfo = typeof(EnDecisionType).GetMember(name).First();
            var attribute = memberInfo.GetCustomAttributes(typeof(DecisionTypeDisplayValue), false).First() as DecisionTypeDisplayValue;
            sb.Append(attribute?.DisplayValue);
        }
    }
}
