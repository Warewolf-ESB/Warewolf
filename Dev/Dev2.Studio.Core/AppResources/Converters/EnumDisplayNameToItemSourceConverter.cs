
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

#endregion

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Converters
{
    internal static class EnumValueDescriptionCache
    {
        private static readonly IDictionary<Type, Tuple<object[], string[]>> Cache
            = new Dictionary<Type, Tuple<object[], string[]>>();

        public static Tuple<object[], string[]> GetValues(Type type)
        {
            if(!type.IsEnum)
                throw new ArgumentException("Type '" + type.Name + "' is not an enum");

            Tuple<object[], string[]> values;
            if(!Cache.TryGetValue(type, out values))
            {
                FieldInfo[] fieldInfos = type.GetFields()
                    .Where(f => f.IsLiteral)
                    .ToArray();

                object[] enumValues = fieldInfos.Select(f => f.GetValue(null)).ToArray();

                DescriptionAttribute[] descriptionAttributes = fieldInfos
                    .Select(f => f.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault())
                    .OfType<DescriptionAttribute>()
                    .ToArray();

                string[] descriptions = descriptionAttributes.Select(a => a.Description).ToArray();
                Debug.Assert(enumValues.Length == descriptions.Length,
                    "Each Enum value must have a description attribute set");

                Cache[type] = values = new Tuple<object[], string[]>(enumValues, descriptions);
            }

            return values;
        }
    }

    public class EnumDisplayNameToItemSourceConverter : IValueConverter
    {
        #region IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return null;
            }

            Tuple<object[], string[]> tuple = EnumValueDescriptionCache.GetValues(value.GetType());
            for(int n = 0; n < tuple.Item1.Length; n++)
            {
                if(Equals(tuple.Item1[n], value))
                {
                    if(targetType == typeof(object))
                    {
                        return tuple.Item2[n]; // property binding
                    }
                    if(targetType == typeof(IEnumerable))
                    {
                        return tuple.Item2; // ItemsSource binding
                    }

                    Debug.Assert(false, "Oops, better check your binding");
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string description = value as string;
            if(description == null)
                return value;

            if((targetType == null) || !targetType.IsEnum)
                return value;

            Tuple<object[], string[]> tuple = EnumValueDescriptionCache.GetValues(targetType);
            for(int i = 0; i < tuple.Item2.Length; i++)
            {
                if(description == tuple.Item2[i])
                {
                    return tuple.Item1[i];
                }
            }

            return value;
        }

        #endregion
    }
}
