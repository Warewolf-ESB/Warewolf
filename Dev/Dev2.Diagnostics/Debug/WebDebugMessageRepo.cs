using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Diagnostics.Debug
{
    public class WebDebugMessageRepo
    {
        readonly IDictionary<Tuple<Guid, Guid>, IList<IDebugState>> _data = new Dictionary<Tuple<Guid, Guid>, IList<IDebugState>>();
        static readonly object Lock = new object();

        private static WebDebugMessageRepo _instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static WebDebugMessageRepo Instance => _instance ?? (_instance = new WebDebugMessageRepo());

        public void AddDebugItem(Guid clientId, Guid sessionId, IDebugState ds)
        {
            lock (Lock)
            {
                if (ds.ParentID == default(Guid))
                    ds.ParentID = null;
                IList<IDebugState> list;
                var key = new Tuple<Guid, Guid>(clientId, sessionId);
                if (_data.TryGetValue(key, out list))
                {
                   
                    list.Add(ds);
                }
                else
                {
                    list = new List<IDebugState> { ds };
                    _data[key] = list;
                }
            }
        }

        public IList<IDebugState> FetchDebugItems(Guid clientId, Guid sessionId)
        {
            lock (Lock)
            {
                var key = new Tuple<Guid, Guid>(clientId, sessionId);
                IList<IDebugState> list;
                if (_data.TryGetValue(key, out list))
                {
                    _data.Remove(key);
                    return list;
                }
            }

            return null;
        }
    }
}
