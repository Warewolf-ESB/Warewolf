/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common;
using Dev2.SignalR.Wrappers;
using Warewolf.Esb;

namespace Warewolf.Client
{
    public class HubWatcher<T>
    {
        private ISubscriptionWrapper _registeredEventWatcher;

        public HubWatcher(IHubProxyWrapper proxy)
        {
            _registeredEventWatcher = proxy.Subscribe(typeof(T).Name);
            _registeredEventWatcher.Received += (tokens) =>
            {
                if (tokens.Count > 0)
                {
                    var o = tokens[0].ToObject<T>();
                    OnChange?.Invoke(o);
                }
                else
                {
                    Dev2Logger.Error("watcher stream encountered empty value", GlobalConstants.WarewolfError);
                }
            };
        }

        public event Action<T> OnChange;
    }
}