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
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Diagnostics.Debug
{
    public abstract class DebugMessegaBase<T> where T : class, new()
    {
#pragma warning disable S2743 // Static fields should not be used in generic types
        private static readonly T _instance = new T();
#pragma warning restore S2743 // Static fields should not be used in generic types
        public static T Instance { get => _instance; }
        protected readonly object Lock = new object();
        protected readonly IDictionary<Tuple<Guid, Guid>, IList<IDebugState>> Data = new Dictionary<Tuple<Guid, Guid>, IList<IDebugState>>();

        public virtual void AddDebugItem(Guid clientId, Guid sessionId, IDebugState ds)
        {
            lock (Lock)
            {
                var key = new Tuple<Guid, Guid>(clientId, sessionId);
                if (Data.TryGetValue(key, out IList<IDebugState> list))
                {
                    list.Add(ds);
                }
                else
                {
                    list = new List<IDebugState> { ds };
                    Data[key] = list;
                }
            }
        }

        public IList<IDebugState> FetchDebugItems(Guid clientId, Guid sessionId)
        {
            lock (Lock)
            {
                var key = new Tuple<Guid, Guid>(clientId, sessionId);
                if (Data.TryGetValue(key, out IList<IDebugState> list))
                {
                    Data.Remove(key);
                    return list;
                }
            }

            return null;
        }
    }
}
