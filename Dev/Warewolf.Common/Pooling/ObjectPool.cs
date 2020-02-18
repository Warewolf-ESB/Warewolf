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
    public class ObjectPool<T> : IObjectPool<T> where T:class
    {
        private readonly ConcurrentBag<T> objects = new ConcurrentBag<T>();
        private readonly Func<T> objectGenerator;
        private readonly Func<T, bool> objectValidator;
        public ObjectPool(Func<T> objectGenerator, Func<T, bool> objectValidator)
        {
            this.objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
            this.objectValidator = objectValidator ?? throw new ArgumentNullException(nameof(objectValidator));
        }

        public T AcquireObject()
        {
            if (objects.TryTake(out T concrete))
            {
                objectValidator.Invoke(concrete);
                return concrete;
            }
            else
            {
                concrete = objectGenerator.Invoke();
                objectValidator.Invoke(concrete);
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
