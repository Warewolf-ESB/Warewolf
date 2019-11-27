using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Warewolf.Interfaces.Pooling;

namespace Warewolf.Pooling
{
    public class ObjectPool<T> : IObjectPool<T> where T:class, new()
    {
        private readonly ConcurrentBag<T> objects;
        private readonly Func<T> objectGenerator;
        private readonly object locker;

        public ObjectPool(Func<T> objectGenerator)
        {
            this.objectGenerator = objectGenerator;
            objects = new ConcurrentBag<T>();
            locker = new object();
        }

        public T AcquireObject()
        {
            lock (locker)
            {
                if (objects.TryTake(out T concrete))
                {
                    return concrete;
                }
                else
                {
                    concrete = objectGenerator();
                    objects.Add(concrete);
                    return concrete;
                }
            }
        }

        public void ReleaseObject(T concrete)
        {
            lock (locker)
            {
                objects.Add(concrete);
            }
        }

        public void Dispose()
        {
            lock (locker)
            {
                while (objects.TryTake(out T concrete))
                {
                    IDisposable disposable = (IDisposable)concrete;
                    disposable.Dispose();
                }
            }
        }
    }
}
