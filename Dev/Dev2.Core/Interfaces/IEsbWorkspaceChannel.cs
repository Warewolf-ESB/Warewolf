using System.ServiceModel;

namespace Dev2.Interfaces
{
    public interface IEsbWorkspaceChannel : IEsbChannel
    {
        IExtensibleObject<OperationContext> GetCurrentOperationContext();
    }
}