using System;
using System.Collections.Generic;
using Dev2.Diagnostics;

namespace Dev2.Runtime.Hosting
{
    /// <summary>
    /// Used to store remote debug data ;)
    /// </summary>
    public class RemoteDebugMessageRepo
    {

        private IDictionary<Guid, IList<DebugState>> _data = new Dictionary<Guid, IList<DebugState>>();
        private static object _lock = new object();

        private static RemoteDebugMessageRepo _instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static RemoteDebugMessageRepo Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new RemoteDebugMessageRepo();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Adds the debug item.
        /// </summary>
        /// <param name="remoteInvokeID">The remote invoke ID.</param>
        /// <param name="ds">The ds.</param>
        public void AddDebugItem(string remoteInvokeID, DebugState ds)
        {
            Guid id;
            Guid.TryParse(remoteInvokeID, out id);
            if (id != Guid.Empty)
            {
                lock (_lock)
                {
                    IList<DebugState> list;
                    if (_data.TryGetValue(id, out list))
                    {
                        list.Add(ds);
                    }
                    else
                    {
                        list = new List<DebugState>();
                        list.Add(ds);
                        _data[id] = list;
                    }
                }
            }
        }

        /// <summary>
        /// Fetches the debug items.
        /// </summary>
        /// <param name="remoteInvokeID">The remote invoke ID.</param>
        /// <returns></returns>
        public IList<DebugState> FetchDebugItems(Guid remoteInvokeID)
        {

            lock (_lock)
            {
                IList<DebugState> list;
                if (_data.TryGetValue(remoteInvokeID, out list))
                {
                    _data.Remove(remoteInvokeID); // clear out all messages ;)
                    return list;
                }
            }

            return null;
        }
    }
}
