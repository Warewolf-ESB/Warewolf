using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.UI
{
    // Moved code incorrectly put into ConnectViewModel here
    public class ConnectControlViewModel
    {
        readonly IEnvironmentModel _activeEnvironment;

        public ConnectControlViewModel(IEnvironmentModel activeEnvironment)
        {
            if(activeEnvironment == null)
            {
                throw new ArgumentNullException("activeEnvironment");
            }
            _activeEnvironment = activeEnvironment;
        }

        public IEnvironmentModel ActiveEnvironment
        {
            get
            {

                return _activeEnvironment;
            }
        }

        public IServer GetSelectedServer(IList<IServer> servers, string labelText)
        {
            if(servers == null || servers.Count == 0)
            {
                return null;
            }

            if(string.IsNullOrEmpty(labelText) || !labelText.Contains("Destination"))
            {
                return servers.FirstOrDefault(s => s.Alias == _activeEnvironment.Name) ?? servers.First(s => s.IsLocalHost);
            }

            if(_activeEnvironment.IsLocalHost() && servers.Count(itm => !itm.IsLocalHost && itm.Environment.IsConnected) == 1)
            {
                //Select the only other connected server
                var otherServer = servers.FirstOrDefault(itm => !itm.IsLocalHost && itm.Environment.IsConnected);
                if(otherServer != null)
                {
                    return otherServer;
                }
            }

            if(_activeEnvironment.IsLocalHost() && servers.Count(itm => !itm.IsLocalHost) == 1)
            {
                //Select and connect to the only other server
                var otherServer = servers.FirstOrDefault(itm => !itm.IsLocalHost);
                if(otherServer != null)
                {
                    otherServer.Environment.Connect();
                    otherServer.Environment.ForceLoadResources();
                    return otherServer;
                }
            }

            if(!_activeEnvironment.IsLocalHost())
            {
                //Select localhost
                return servers.FirstOrDefault(itm => itm.IsLocalHost);
            }

            return null;
        }
    }
}