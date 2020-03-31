using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Dev2
{
    public static class CustomContainer
    {
        public static T Get<T>() where T : class
        {
            return Warewolf.CustomContainer.Get<T>();
        }

        public static object Get(Type type)
        {
            return Warewolf.CustomContainer.Get(type);
        }

        public static void Register<T>(T instance)
        {
            Warewolf.CustomContainer.Register<T>(instance);
        }

        public static T GetInstancePerRequestType<T>() where T : class
        {
            return Warewolf.CustomContainer.GetInstancePerRequestType<T>();
        }

        public static T CreateInstance<T>(params object[] constructorParameters)
        {
            return Warewolf.CustomContainer.CreateInstance<T>(constructorParameters);
        }

        public static List<Type> LoadedTypes
        {
            get => Warewolf.CustomContainer.LoadedTypes;
            set => Warewolf.CustomContainer.LoadedTypes = value;
        }

        // TODO: remove this so that its only allowed in tests
        public static int EntiresCount => Warewolf.CustomContainer.EntiresCount;

        public static void AddToLoadedTypes(Type type)
        {
            Warewolf.CustomContainer.AddToLoadedTypes(type);
        }

        public static void RegisterInstancePerRequestType<T>(Func<object> constructorFunc)
        {
            Warewolf.CustomContainer.RegisterInstancePerRequestType<T>(constructorFunc);
        }

        // TODO: remove this so that its only allowed in tests
        public static void DeRegister<T>()
        {
            Warewolf.CustomContainer.DeRegister<T>();
        }

        // TODO: remove this so that its only allowed in tests
        public static void Clear()
        {
            Warewolf.CustomContainer.Clear();
        }
    }
}

