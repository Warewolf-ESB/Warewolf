using System;
using System.Collections.Generic;
using System.Threading;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Runtime.Hosting
{
    public interface IInitializingSingleton
    {
        void Initialize();
    }
    public class Singleton<T> where T : class, IInitializingSingleton, new()
    {
        static readonly Lazy<T> _instance = new Lazy<T>(() =>
        {
            var instance = CustomContainer.Get<T>();
            if (instance is null)
            {
                var newResourceCatalog = new T();
                CustomContainer.Register<T>(newResourceCatalog);

                newResourceCatalog.Initialize();
            }
            return instance;
        }, LazyThreadSafetyMode.PublicationOnly);

        public static T Instance => _instance.Value;
    }
    public interface IClusterCatalog
    {
        void NotifyResourceChanged(IResource resource);
        void AddFollower(string key);
    }

    public class ClusterCatalog : Singleton<ClusterCatalog>, IClusterCatalog, IInitializingSingleton
    {
        private readonly List<string> _followers = new List<string>();

        public void Initialize()
        {
            
        }

        public void NotifyResourceChanged(IResource resource)
        {
            
        }

        public void AddFollower(string key)
        {
            _followers.Add(key);
        }
    }
}