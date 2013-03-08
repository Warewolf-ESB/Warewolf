using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Common
{
    /// <summary>
    /// Base class for all spooky action at a distanced impls
    /// </summary>
    /// <typeparam name="T">Class</typeparam>
    /// <typeparam name="S">Enum</typeparam>
    public class SpookyAction<T, TS> where T : ISpookyLoadable<TS>
    {
        private static readonly ConcurrentDictionary<TS, T> _options = new ConcurrentDictionary<TS, T>();
        private static bool _inited;

        /// <summary>
        /// Private method for intitailizing the list of options
        /// </summary>
        private void Bootstrap()
        {
            var type = typeof(T);

            List<Type> types = typeof(T).Assembly.GetTypes()
                    .Where(t => (type.IsAssignableFrom(t))).ToList();

            foreach (Type t in types)
            {
                if (!t.IsAbstract && !t.IsInterface)
                {
                    T item = (T)Activator.CreateInstance(t, true);
                    if (item != null)
                    {
                        _options.TryAdd(item.HandlesType(), item);
                    }
                }
            }
        }

        /// <summary>
        /// Find the matching object
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <returns></returns>
        public T FindMatch(TS typeOf)
        {
            T result;
            if (!_options.TryGetValue(typeOf, out result))
            {
                lock (_options)
                {
                    if (!_inited)
                    {
                        Bootstrap();
                        _inited = true;
                    }

                    _options.TryGetValue(typeOf, out result);
                }
            }

            return result;
        }

        /// <summary>
        /// Find all objects
        /// </summary>
        /// <param name="expressionType"></param>
        /// <returns></returns>
        public IList<T> FindAll()
        {
            if (_options.Count == 0)
            {
                lock (_options)
                {
                    if (_options.Count == 0)
                    {
                        Bootstrap();
                    }
                }
            }

            return _options.Values.ToList();
        }
    }
}
