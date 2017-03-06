using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dev2.Common
{
    //http://stackoverflow.com/questions/286294/object-to-object-mapper
    public class Mapper
    {

        private static readonly Dictionary<KeyValuePair<Type, Type>, object> Maps = new Dictionary<KeyValuePair<Type, Type>, object>();
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <param name="mapFunc"></param>
        public static void AddMap<TFrom, TTo>(Action<TFrom, TTo> mapFunc = null)
            where TFrom : class
            where TTo : class
        {
            var keyValuePair = new KeyValuePair<Type, Type>(typeof(TFrom), typeof(TTo));
            if (!Maps.ContainsKey(keyValuePair))
                Maps.Add(keyValuePair, mapFunc);
        }

        /// <summary>
        /// Maps property from obj1 to obj2 with an option to map fields or exclude some properties
        /// </summary>
        /// <typeparam name="TMapFrom"></typeparam>
        /// <typeparam name="TMapTo"></typeparam>
        /// <param name="mapFrom"></param>
        /// <param name="mapTo"></param>
        /// <param name="mapFields"></param>
        /// <param name="ignoreList"></param>
        public static void Map<TMapFrom, TMapTo>(TMapFrom mapFrom, TMapTo mapTo, bool mapFields = true, params string[] ignoreList)
            where TMapFrom : class
            where TMapTo : class
        {
            if (mapFrom == null)
                throw new ArgumentNullException(nameof(mapFrom));

            var key = new KeyValuePair<Type, Type>(typeof(TMapFrom), typeof(TMapTo));

            Action<TMapFrom, TMapTo> map = null;
            if (Maps.ContainsKey(key))
            {
                map = (Action<TMapFrom, TMapTo>)Maps[key];
            }
            var tFrom = mapFrom.GetType();

            if (mapTo == null)
                mapTo = Activator.CreateInstance<TMapTo>();

            var tTo = mapTo.GetType();
            var hasMap = Maps.Any(m => m.Key.Equals(key));
            if (!hasMap)
                throw new ArgumentException($"No mapping exists from {tFrom.Name} to {tTo.Name}");

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
                    continue;
                var fromValue = prop.FromProperty.GetValue(mapFrom, null);
                if (prop.ToProperty.CanWrite)
                    prop.ToProperty.SetValue(mapTo, fromValue, null);
            }
            if (mapFields)
                foreach (var field in equalFields)
                {
                    if (field.FromField.FieldType.Name != field.ToField.FieldType.Name)
                        continue;
                    var fieldValue = field.FromField.GetValue(mapFrom);
                    field.ToField.SetValue(mapTo, fieldValue);
                }
            map?.Invoke(mapFrom, mapTo);
        }

      
    }
}
