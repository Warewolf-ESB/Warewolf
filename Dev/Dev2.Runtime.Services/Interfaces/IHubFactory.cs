using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Client;

namespace Dev2.Runtime.Interfaces
{
    public interface IHubFactory
	{
		IHubProxy CreateHubProxy(Data.ServiceModel.Connection connection);
        Microsoft.AspNetCore.SignalR.Client.HubConnection GetHubConnection(Data.ServiceModel.Connection connection);
    }
}