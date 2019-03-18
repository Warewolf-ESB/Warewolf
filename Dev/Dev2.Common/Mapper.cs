#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dev2.Common
{
    public static class Mapper
    {
        static readonly Dictionary<KeyValuePair<Type, Type>, object> Maps = new Dictionary<KeyValuePair<Type, Type>, object>();

        public static void Clear()
        {
            Maps.Clear();
        }

        public static void AddMap<TFrom, TTo>()
            where TFrom : class
            where TTo : class
        {
            AddMap<TFrom, TTo>(null);
        }

        public static void AddMap<TFrom, TTo>(Action<TFrom, TTo> mapFunc)
            where TFrom : class
            where TTo : class
        {
            var keyValuePair = new KeyValuePair<Type, Type>(typeof(TFrom), typeof(TTo));
            if (!Maps.ContainsKey(keyValuePair))
            {
                Maps.Add(keyValuePair, mapFunc);
            }
        }

        public static void Map<TMapFrom, TMapTo>(TMapFrom mapFrom, TMapTo mapTo, params string[] ignoreList)
            where TMapFrom : class
            where TMapTo : class
        {
            Map(mapFrom, mapTo, true, ignoreList);
        }

        public static void Map<TMapFrom, TMapTo>(TMapFrom mapFrom, TMapTo mapTo, bool mapFields, params string[] ignoreList)
            where TMapFrom : class
            where TMapTo : class
        {
            if (mapFrom == null)
            {
                throw new ArgumentNullException(nameof(mapFrom));
            }

            var key = new KeyValuePair<Type, Type>(typeof(TMapFrom), typeof(TMapTo));

            Action<TMapFrom, TMapTo> map = null;
            if (Maps.ContainsKey(key))
            {
                map = (Action<TMapFrom, TMapTo>)Maps[key];
            }
            var tFrom = mapFrom.GetType();

            if (mapTo == null)
            {
                mapTo = Activator.CreateInstance<TMapTo>();
            }

            var tTo = mapTo.GetType();
            var hasMap = Maps.Any(m => m.Key.Equals(key));
            if (!hasMap)
            {
                throw new ArgumentException($"No mapping exists from {tFrom.Name} to {tTo.Name}");
            }

            var mapToProperties = tTo.GetProperties().Where(info => !ignoreList.Contains(info.Name));
            var mapFromProperties = tFrom.GetProperties().Where(info => !ignoreList.Contains(info.Name));
            var mapToFields = tTo.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(info => !info.Name.Contains("k__BackingField"));
            var mapFromFields = tFrom.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(info => !info.Name.Contains("k__BackingField"));

            var equalProps = from tP in mapToProperties
                             join fP in mapFromProperties on tP.Name equals fP.Name
                             select new
                             {
                                 ToProperty = tP,
                                 FromProperty = fP
                             };

            var equalFields = from tF in mapToFields
                              join fF in mapFromFields on tF.Name equals fF.Name
                              select new
                              {
                                  ToField = tF,
                                  FromField = fF
                              };
            foreach (var prop in equalProps)
            {
                if (prop.FromProperty.PropertyType.Name != prop.ToProperty.PropertyType.Name)
                {
                    continue;
                }

                var fromValue = prop.FromProperty.GetValue(mapFrom, null);
                if (prop.ToProperty.CanWrite)
                {
                    prop.ToProperty.SetValue(mapTo, fromValue, null);
                }
            }
            if (mapFields)
            {
                foreach (var field in equalFields)
                {
                    if (field.FromField.FieldType.Name != field.ToField.FieldType.Name)
                    {
                        continue;
                    }

                    var fieldValue = field.FromField.GetValue(mapFrom);
                    field.ToField.SetValue(mapTo, fieldValue);
                }
            }

            map?.Invoke(mapFrom, mapTo);
        }      
    }
}
