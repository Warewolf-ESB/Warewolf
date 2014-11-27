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
using System.Linq;
using Dev2.Common.ExtMethods;

namespace Dev2.Common.Interfaces.Enums.Enums
{
    public static class Dev2EnumConverter
    {
        public static IList<TTEnum> GetEnumsToList<TTEnum>() where TTEnum : struct
        {
            Type type = typeof (TTEnum);
            if (!type.IsEnum) throw new InvalidOperationException("Generic parameter T must be an enumeration type.");
            return Enum.GetValues(type).Cast<TTEnum>().ToList();
        }

        public static IList<string> ConvertEnumsTypeToStringList<tEnum>() where tEnum : struct
        {
            Type enumType = typeof (tEnum);

            IList<string> result = new List<string>();

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (object value in Enum.GetValues(enumType))
                // ReSharper restore LoopCanBeConvertedToQuery
            {
                result.Add((value as Enum).GetDescription());
            }

            return result;
        }

        public static string ConvertEnumValueToString(Enum value)
        {
            Type type = value.GetType();
            if (!type.IsEnum) throw new InvalidOperationException("Generic parameter T must be an enumeration type.");

            return value.GetDescription();
        }

        public static object GetEnumFromStringDiscription(string discription, Type type)
        {
            if (!type.IsEnum) throw new InvalidOperationException("Generic parameter T must be an enumeration type.");

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (object value in Enum.GetValues(type))
                // ReSharper restore LoopCanBeConvertedToQuery
            {
                if ((value as Enum).GetDescription() == discription)
                {
                    return value;
                }
            }
            return null;
        }
    }
}