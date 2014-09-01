using System;
using System.Collections.Generic;

namespace Dev2
{
    public static class CustomContainer
    {
        static readonly Dictionary<Type, object> RegisterdTypes = new Dictionary<Type, object>();

        public static int EntiresCount
        {
            get
            {
                return RegisterdTypes.Count;
            }
        }

        public static void Clear()
        {
            RegisterdTypes.Clear();
        }

        public static void Register<T>(T concrete)
        {
            if(RegisterdTypes.ContainsKey(typeof(T)))
            {
                DeRegister<T>();
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

        public static object Get(Type type)
        {
            var requestedType = type;
            if(RegisterdTypes.ContainsKey(requestedType))
            {
                var registerdType = RegisterdTypes[requestedType];
                return registerdType;
            }
            return null;
        }

        public static void DeRegister<T>()
        {
            if(RegisterdTypes.ContainsKey(typeof(T)))
            {
                RegisterdTypes.Remove(typeof(T));
            }
        }
    }
}
