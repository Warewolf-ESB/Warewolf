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
using System.Linq;
using System.Reflection;

namespace Dev2.Common
{
    public interface IFieldAndPropertyMapper
    {
        void AddMap<TFrom, TTo>()
            where TFrom : class
            where TTo : class;
        void Map<TMapFrom, TMapTo>(TMapFrom mapFrom, TMapTo mapTo)
            where TMapFrom : class
            where TMapTo : class;
    }
    /// Used in tools that use OutputsRegion to store previous configuration
    /// when changing the tool's configuration
    public class FieldAndPropertyMapper : IFieldAndPropertyMapper
    {
        readonly Dictionary<KeyValuePair<Type, Type>, object> _manualMappingCallbacks = new Dictionary<KeyValuePair<Type, Type>, object>();

        public void Clear()
        {
            _manualMappingCallbacks.Clear();
        }

        public void AddMap<TFrom, TTo>()
            where TFrom : class
            where TTo : class
        {
            AddMap<TFrom, TTo>(null);
        }

        public void AddMap<TFrom, TTo>(Action<TFrom, TTo> mapFunc)
            where TFrom : class
            where TTo : class
        {
            var keyValuePair = new KeyValuePair<Type, Type>(typeof(TFrom), typeof(TTo));
            if (!_manualMappingCallbacks.ContainsKey(keyValuePair))
            {
                _manualMappingCallbacks.Add(keyValuePair, mapFunc);
            }
        }

        public void Map<TMapFrom, TMapTo>(TMapFrom mapFrom, TMapTo mapTo)
            where TMapFrom : class
            where TMapTo : class
        {
            if (mapFrom == null)
            {
                throw new ArgumentNullException(nameof(mapFrom));
            }

            var tMapTo = mapTo;

            if (tMapTo == null)
            {
                tMapTo = Activator.CreateInstance<TMapTo>();
            }

            var key = new KeyValuePair<Type, Type>(typeof(TMapFrom), typeof(TMapTo));

            var hasMap = _manualMappingCallbacks.Any(m => m.Key.Equals(key));
            if (!hasMap)
            {
                throw new ArgumentException($"No mapping exists from {mapFrom.GetType().Name} to {tMapTo.GetType().Name}");
            }

            MapProperties(mapFrom, tMapTo);

            MapFields(mapFrom, tMapTo);

            Action<TMapFrom, TMapTo> map = null;
            if (_manualMappingCallbacks.ContainsKey(key))
            {
                map = (Action<TMapFrom, TMapTo>)_manualMappingCallbacks[key];
            }

            map?.Invoke(mapFrom, tMapTo);
        }

        private static void MapProperties<TMapFrom, TMapTo>(TMapFrom sourceInstance, TMapTo targetInstance)
            where TMapFrom : class
            where TMapTo : class
        {
            var sourceProperties = sourceInstance.GetType().GetProperties();
            var targetProperties = targetInstance.GetType().GetProperties();

            var compatibleProperties = from targetPropInfo in targetProperties
                                     join sourcePropInfo in sourceProperties on targetPropInfo.Name equals sourcePropInfo.Name
                                     where sourcePropInfo.PropertyType.Name == targetPropInfo.PropertyType.Name && targetPropInfo.CanWrite
                                     select new
                                     {
                                         ToProperty = targetPropInfo,
                                         Value = sourcePropInfo.GetValue(sourceInstance, null)
                                     };

            foreach (var prop in compatibleProperties)
            {
                prop.ToProperty.SetValue(targetInstance, prop.Value, null);
            }
        }

        private static void MapFields<TMapFrom, TMapTo>(TMapFrom mapFrom, TMapTo mapTo)
            where TMapFrom : class
            where TMapTo : class
        {
            var sourceFields = mapFrom.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(info => !info.Name.Contains("k__BackingField"));
            var targetFields = mapTo.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(info => !info.Name.Contains("k__BackingField"));

            var compatibleFields = from targetFieldInfo in targetFields
                              join sourceFieldInfo in sourceFields on targetFieldInfo.Name equals sourceFieldInfo.Name
                              where sourceFieldInfo.FieldType.Name == targetFieldInfo.FieldType.Name
                              select new
                              {
                                  ToField = targetFieldInfo,
                                  Value = sourceFieldInfo.GetValue(mapFrom)
                              };

            foreach (var field in compatibleFields)
            {
                field.ToField.SetValue(mapTo, field.Value);
            }
        }
    }
}
