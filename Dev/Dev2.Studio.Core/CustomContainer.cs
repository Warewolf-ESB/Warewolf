using System;
using System.Collections.Generic;

namespace Dev2
{
    public static class CustomContainer
    {
        static readonly Dictionary<Type, object> RegisterdTypes = new Dictionary<Type, object>();

        public static void Register<T>(object concrete)
        {
            if(RegisterdTypes.ContainsKey(typeof(T)))
            {
                return;
            }
            RegisterdTypes.Add(typeof(T), concrete);
        }

        public static T Get<T>() where T : class
        {
            var requestedType = typeof(T);
            if(RegisterdTypes.ContainsKey(requestedType))
            {
                var registerdType = RegisterdTypes[requestedType];
                return registerdType as T;
            }
            return null;
        }
    }
}
