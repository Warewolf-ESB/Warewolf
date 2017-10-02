using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Diagnostics.Debug
{
    public abstract class DebugMessegaBase<T> where T : class, new()
    {
        private static T _instance;
        public static T Instance => _instance ?? (_instance = new T());
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
