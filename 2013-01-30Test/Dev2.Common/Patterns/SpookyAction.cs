using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Common
{
    /// <summary>
    /// Base class for all spooky action at a distanced impls
    /// </summary>
    /// <typeparam name="T">Class</typeparam>
    /// <typeparam name="S">Enum</typeparam>
    public class SpookyAction<T> where T : ISpookyLoadable
    {
        private static readonly ConcurrentDictionary<Enum, T> _options = new ConcurrentDictionary<Enum, T>();

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
        /// <param name="expressionType"></param>
        /// <returns></returns>
        public T FindMatch(Enum typeOf)
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

            T result;
            if (!_options.TryGetValue(typeOf, out result))
            {
                result = default(T);
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
