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
using System.Collections.Concurrent;
using Warewolf.Interfaces.Pooling;

namespace Warewolf.Pooling
{
    internal class ObjectPool<T> : IObjectPool<T> where T:class, new()
    {
        private readonly ConcurrentBag<T> objects;
        private readonly Func<T> objectGenerator;
        public ObjectPool(Func<T> objectGenerator)
        {
            this.objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
            objects = new ConcurrentBag<T>();
        }

        public T AcquireObject()
        {
            if (objects.TryTake(out T concrete))
            {
                return concrete;
            }
            else
            {
                concrete = objectGenerator?.Invoke();
                objects.Add(concrete);
                return concrete;
            }
        }

        public void ReleaseObject(T concrete)
        {
            objects.Add(concrete);
        }

        public void Dispose()
        {
            while (objects.TryTake(out T concrete))
            {
            }
        }
    }
}
