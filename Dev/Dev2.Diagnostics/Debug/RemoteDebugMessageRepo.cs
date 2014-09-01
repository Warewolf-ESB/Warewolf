using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Diagnostics.Debug
{
    /// <summary>
    /// Used to store remote debug data ;)
    /// </summary>
    public class RemoteDebugMessageRepo
    {
        readonly IDictionary<Guid, IList<IDebugState>> _data = new Dictionary<Guid, IList<IDebugState>>();
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
        public void AddDebugItem(string remoteInvokeID, IDebugState ds)
        {
            Guid id;
            Guid.TryParse(remoteInvokeID, out id);
            if(id != Guid.Empty)
            {
                lock(Lock)
                {
                    IList<IDebugState> list;
                    if(_data.TryGetValue(id, out list))
                    {
                        if(list.Contains(ds)) return;
                        list.Add(ds);
                    }
                    else
                    {
                        list = new List<IDebugState> { ds };
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
        public IList<IDebugState> FetchDebugItems(Guid remoteInvokeID)
        {

            lock(Lock)
            {
                IList<IDebugState> list;
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
