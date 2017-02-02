using System;

namespace Dev2.Common.Interfaces.Deploy
{

    public delegate void ServerSate(object sender, IServer server);
    public interface IDeployDestinationExplorerViewModel:IExplorerViewModel    
    {
        event ServerSate ServerStateChanged;
        Version MinSupportedVersion{get;}
        Version ServerVersion { get; }
        bool DeployTests { get; set; }  
    }
}