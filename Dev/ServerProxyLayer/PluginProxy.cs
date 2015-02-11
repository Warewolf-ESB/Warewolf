using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Communication;
using Dev2.Controller;

namespace Warewolf.Studio.ServerProxyLayer
{
    public class PluginProxy : ProxyBase,IPluginProxy
    {
        #region Implementation of IPluginProxy

        public PluginProxy(ICommunicationControllerFactory communicationControllerFactory, IEnvironmentConnection connection)
            : base(communicationControllerFactory, connection)
        {
        }

        public void DeleteTool(IToolDescriptor tool)
        {
            var serialiser = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("DeleteToolService");
            comsController.AddPayloadArgument("tool", serialiser.Serialize(tool));

            comsController.ExecuteCommandWithErrorHandling<string>(Connection, Connection.WorkspaceID);

            
        }

        public void LoadTool(IToolDescriptor tool, byte[] dllBytes)
        {
            var serialiser = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("LoadToolService");
            comsController.AddPayloadArgument("tool", serialiser.Serialize(tool));
            comsController.AddPayloadArgument("dllBytes", serialiser.Serialize(dllBytes));
            comsController.ExecuteCommandWithErrorHandling<IEsbRequestResult<string>>(Connection, Connection.WorkspaceID);

        }

        public IList<IExplorerItemModel> GetToolDependencies(IToolDescriptor tool)
        {
            var serialiser = new Dev2JsonSerializer();
            var comsController = CommunicationControllerFactory.CreateController("GetToolDependenciesService");
            comsController.AddPayloadArgument("tool", serialiser.Serialize(tool));
            var result = comsController.ExecuteCommandWithErrorHandling<IList<IExplorerItemModel>>(Connection, Connection.WorkspaceID);
            return result;
           
        }

        #endregion
    }
}
