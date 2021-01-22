/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
    public enum enFormDataTableType
    {
        [Index(0)] [DecisionTypeDisplayValue("Text")] Text,
        [Index(1)] [DecisionTypeDisplayValue("File")] File,
    }

    public static class enFormDataTableTypeExtensionMethods
    {
        public static bool IsTripleOperand(this enFormDataTableType enFormDataTableType)
        {
            return
                enFormDataTableType == enFormDataTableType.File;
        }

        public static void RenderDescription(this enFormDataTableType enFormDataTableType, StringBuilder sb)
        {
            var name = Enum.GetName(typeof(enFormDataTableType), enFormDataTableType);
            var memberInfo = typeof(enFormDataTableType).GetMember(name).First();
            var attribute = memberInfo.GetCustomAttributes(typeof(DecisionTypeDisplayValue), false).First() as DecisionTypeDisplayValue;
            sb.Append(attribute.DisplayValue);
        }

    }

}
