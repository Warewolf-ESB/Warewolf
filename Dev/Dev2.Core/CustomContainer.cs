#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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

namespace Dev2
{
    public static class CustomContainer
    {
        public static List<Type> LoadedTypes { get; set; }

        static readonly Dictionary<Type, object> RegisterdTypes = new Dictionary<Type, object>();
        static readonly Dictionary<Type, Func<object>> RegisterdPerRequestTypes = new Dictionary<Type, Func<object>>();

        public static int EntiresCount => RegisterdTypes.Count;

    
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

        public static void AddToLoadedTypes(Type type)
        {
            if(LoadedTypes is null)
            {
                LoadedTypes = new List<Type>();
            }
            if (!LoadedTypes.Contains(type))
            {
                LoadedTypes.Add(type);
            }
        }

        public static T CreateInstance<T>(params object[] constructorParameters)
        {
            var typeToCreate = typeof(T);
            var assemblyTypes = LoadedTypes;
            object createdObject = null;
            foreach (var assemblyType in assemblyTypes)
            {
                if(assemblyType.IsPublic && !assemblyType.IsAbstract && assemblyType.IsClass && !assemblyType.IsGenericType && typeToCreate.IsAssignableFrom(assemblyType))
                {
                    createdObject = TryInvokeConstructor(assemblyType, constructorParameters);
                }
            }
            if (createdObject != null)
            {
                return (T)createdObject;
            }
            return default(T);
        }

        static object TryInvokeConstructor(Type assemblyType, object[] constructorParameters)
        {
            object createdObject = null;
            var constructorInfos = assemblyType.GetConstructors();
            foreach (var constructorInfo in constructorInfos)
            {
                if (ConstructorMatch(constructorParameters, constructorInfo) && createdObject == null)
                {
                    createdObject = constructorInfo.Invoke(constructorParameters);
                }
            }

            return createdObject;
        }

        static bool ConstructorMatch(object[] constructorParameters, System.Reflection.ConstructorInfo constructorInfo)
        {
            var constructorMatch = false;
            var parameterInfos = constructorInfo.GetParameters();
            var numberOfParameters = parameterInfos.Length;
            if (numberOfParameters == constructorParameters.Length)
            {
                for (int i = 0; i < numberOfParameters; i++)
                {
                    var constructorParameterType = parameterInfos[i].ParameterType;
                    var givenParameterType = constructorParameters[i].GetType();
                    if ((givenParameterType == constructorParameterType) || constructorParameterType.IsAssignableFrom(givenParameterType))
                    {
                        constructorMatch = true;
                    }
                    else
                    {
                        constructorMatch = false;
                        break;
                    }
                }
            }

            return constructorMatch;
        }

        public static void RegisterInstancePerRequestType<T>(Func<object> constructorFunc)
        {
            if (RegisterdPerRequestTypes.ContainsKey(typeof(T)))
            {
                DeRegisterInstancePerRequestType<T>();
            }
            RegisterdPerRequestTypes.Add(typeof(T), constructorFunc);
        }

        public static T GetInstancePerRequestType<T>() where T : class
        {
            var requestedType = typeof(T);
            if (RegisterdPerRequestTypes.ContainsKey(requestedType))
            {
                var registerdType = RegisterdPerRequestTypes[requestedType];
                return registerdType.Invoke() as T;
            }
            return null;
        }

        static void DeRegisterInstancePerRequestType<T>()
        {
            if (RegisterdPerRequestTypes.ContainsKey(typeof(T)))
            {
                RegisterdPerRequestTypes.Remove(typeof(T));
            }
        }
    }
}
