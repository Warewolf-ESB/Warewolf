/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Patterns;

// ReSharper disable CheckNamespace

namespace Dev2.Common
// ReSharper restore CheckNamespace
{
    /// <summary>
    ///     Base class for all spooky action at a distanced impls
    /// </summary>
    /// <typeparam name="TReflect">The type to be reflected.</typeparam>
    /// <typeparam name="THandle">The type that the ISpookyLoadable.HandlesType() method returns, used as a key.</typeparam>
    public class SpookyAction<TReflect, THandle>
        where TReflect : ISpookyLoadable<THandle>
    {
        private readonly ConcurrentDictionary<THandle, TReflect> _options =
            new ConcurrentDictionary<THandle, TReflect>();

        private bool _initialized;

        /// <summary>
        ///     Private method for intitailizing the list of options
        /// </summary>
        private void Bootstrap()
        {
            Type type = typeof (TReflect);

            List<Type> types =
                type.Assembly.GetTypes()
                    .Where(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                    .ToList();

            foreach (TReflect item in types.Select(t => (TReflect) Activator.CreateInstance(t, true)))
            {
                _options.TryAdd(item.HandlesType(), item);
            }
        }

        /// <summary>
        ///     Find the matching object
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <returns></returns>
        public TReflect FindMatch(THandle typeOf)
        {
            TReflect result;
            if (!_options.TryGetValue(typeOf, out result))
            {
                lock (_options)
                {
                    if (!_initialized)
                    {
                        Bootstrap();
                        _initialized = true;
                    }

                    _options.TryGetValue(typeOf, out result);
                }
            }

            return result;
        }

        /// <summary>
        ///     Find all objects
        /// </summary>
        public IList<TReflect> FindAll()
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