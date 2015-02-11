using System.Collections.Generic;
using Dev2.Common.Interfaces.Toolbox;

namespace Dev2.Common.Interfaces.ServerProxyLayer
{
    public interface IPluginProxy
    {
        void DeleteTool(IToolDescriptor tool);

        void LoadTool(IToolDescriptor tool, byte[] dllBytes);

        IList<IExplorerItemModel> GetToolDependencies(IToolDescriptor tool);
    }
}