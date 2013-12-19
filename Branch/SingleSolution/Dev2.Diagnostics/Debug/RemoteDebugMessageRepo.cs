using System;
using System.Collections.Generic;

namespace Dev2.Diagnostics
{
    /// <summary>
    /// Used to store remote debug data ;)
    /// </summary>
    public class RemoteDebugMessageRepo
    {
        readonly IDictionary<Guid, IList<DebugState>> _data = new Dictionary<Guid, IList<DebugState>>();
        static readonly object Lock = new object();

        private static RemoteDebugMessageRepo _instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static RemoteDebugMessageRepo Instance
        {
            get { return _instance ?? (_instance = new RemoteDebugMessageRepo()); }
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
            if(id != Guid.Empty)
            {
                lock(Lock)
                {
                    IList<DebugState> list;
                    if(_data.TryGetValue(id, out list))
                    {
                        if(list.Contains(ds)) return;
                        list.Add(ds);
                    }
                    else
                    {
                        list = new List<DebugState> { ds };
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

            lock(Lock)
            {
                IList<DebugState> list;
                if(_data.TryGetValue(remoteInvokeID, out list))
                {
                    _data.Remove(remoteInvokeID); // clear out all messages ;)
                    return list;
                }
            }

            return null;
        }
    }
}
