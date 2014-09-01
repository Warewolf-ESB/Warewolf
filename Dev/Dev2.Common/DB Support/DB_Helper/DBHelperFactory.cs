using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.DB
{
    /// <summary>
    /// Used to create helper generation instances
    /// </summary>
    public class DBHelperFactory
    {
        private static readonly Dictionary<enSupportedDBTypes, Type> _options
            = new Dictionary<enSupportedDBTypes, Type>();


        static DBHelperFactory()
        {
            var type = typeof(IDBHelper);

            List<Type> types = typeof(IDBHelper).Assembly.GetTypes()
                   .Where(t => (type.IsAssignableFrom(t))).ToList();

            foreach (Type t in types)
            {
                if (!t.IsAbstract && !t.IsInterface)
                {
                    IDBHelper item = Activator.CreateInstance(t, true) as IDBHelper;
                    if (item != null)
                    {
                        _options.Add(item.HandlesType(), t);
                    }
                }
            }
        }

        /// <summary>
        /// Fetch the type to be handled, then generate a concerte instance
        /// </summary>
        /// <param name="typeOf"></param>
        /// <returns></returns>
        public static IDBHelper GenerateNewHelper(enSupportedDBTypes typeOf)
        {
            IDBHelper result;
            Type outType;

            if (!_options.TryGetValue(typeOf, out outType))
            {
                result = null;
            }
            else
            {
                result = Activator.CreateInstance(outType, true) as IDBHelper;
            }

            return result;
        }
    }
}
