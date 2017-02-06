using System;
using System.Collections.Generic;
using System.Linq;

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
        /// 
        /// </summary>
        /// <typeparam name="TMapFrom"></typeparam>
        /// <typeparam name="TMapTo"></typeparam>
        /// <param name="mapFrom"></param>
        /// <param name="mapTo"></param>
        public static void Map<TMapFrom, TMapTo>(TMapFrom mapFrom, TMapTo mapTo)
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

            //If object not already instantiated, make it so. A parameterless constructor will need to be defined!
            if (mapTo == null)
                mapTo = Activator.CreateInstance<TMapTo>();

            var tTo = mapTo.GetType();
            var hasMap = Maps.Any(m => m.Key.Equals(key));
            if (!hasMap)
                throw new ArgumentException($"No mapping exists from {tFrom.Name} to {tTo.Name}");

            var mapToProperties = tTo.GetProperties();
            var mapFromProperties = tFrom.GetProperties();
            var mapToFields = tTo.GetFields();
            var mapFromFields = tFrom.GetFields();

            //Map properties on the same name
            var equalProps = from tP in mapToProperties
                             join fP in mapFromProperties on tP.Name equals fP.Name
                             select new
                             {
                                 ToProperty = tP,
                                 FromProperty = fP
                             };

            //Map fields on the same name
            var equalFields = from tF in mapToFields
                              join fF in mapFromFields on tF.Name equals fF.Name
                              select new
                              {
                                  ToField = tF,
                                  FromField = fF
                              };

            //O(n)
            foreach (var prop in equalProps)
            {

                //They have to have the same return type
                if (prop.FromProperty.PropertyType.Name != prop.ToProperty.PropertyType.Name)
                    continue;

                //Get the value from the mapFrom. Caveat: Indexing properties not supported!
                var fromValue = prop.FromProperty.GetValue(mapFrom, null);
                if (prop.ToProperty.CanWrite)
                    prop.ToProperty.SetValue(mapTo, fromValue, null);
            }

            foreach (var field in equalFields)
            {
                //Fields must have the same return type
                if (field.FromField.FieldType.Name != field.ToField.FieldType.Name)
                    continue;

                //Get the value from the mapFrom and set to the mapTo field
                var fieldValue = field.FromField.GetValue(mapFrom);
                field.ToField.SetValue(mapTo, fieldValue);
            }

            //If the custom mapping function is not null, call it.
            //This will overwrite previously mapped properties
            map?.Invoke(mapFrom, mapTo);
        }
    }
}
