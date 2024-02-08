using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.SignalR.Wrappers.New
{
    internal static class HubConnectionStateExtensions
    {
        public static ConnectionStateWrapped ToConnectionStateWrapped(this HubConnectionState hubConnntState)
        {
            switch (hubConnntState)
            {
                case HubConnectionState.Connecting:
                    return ConnectionStateWrapped.Connecting;
                case HubConnectionState.Connected:
                    return ConnectionStateWrapped.Connected;
                case HubConnectionState.Disconnected:
                    return ConnectionStateWrapped.Disconnected;
                case HubConnectionState.Reconnecting:
                    return ConnectionStateWrapped.Reconnecting;
                default:
                    return ConnectionStateWrapped.Disconnected;
            }
        }
    }
}
