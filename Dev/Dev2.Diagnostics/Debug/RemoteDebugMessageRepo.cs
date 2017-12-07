/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
    /// <summary>
    /// Used to store remote debug data ;)
    /// </summary>
    public class RemoteDebugMessageRepo
    {
        readonly IDictionary<Guid, IList<IDebugState>> _data = new Dictionary<Guid, IList<IDebugState>>();
        static readonly object Lock = new object();

        static RemoteDebugMessageRepo _instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static RemoteDebugMessageRepo Instance => _instance ?? (_instance = new RemoteDebugMessageRepo());

        /// <summary>
        /// Adds the debug item.
        /// </summary>
        /// <param name="remoteInvokeID">The remote invoke ID.</param>
        /// <param name="ds">The ds.</param>
        public void AddDebugItem(string remoteInvokeID, IDebugState ds)
        {
            Guid.TryParse(remoteInvokeID, out Guid id);
            if (id != Guid.Empty)
            {
                lock(Lock)
                {
                    if (_data.TryGetValue(id, out IList<IDebugState> list))
                    {
                        if (list.Contains(ds))
                        {
                            return;
                        }

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
                if (_data.TryGetValue(remoteInvokeID, out IList<IDebugState> list))
                {
                    _data.Remove(remoteInvokeID); // clear out all messages ;)
                    return list;
                }
            }

            return null;
        }
    }
}
