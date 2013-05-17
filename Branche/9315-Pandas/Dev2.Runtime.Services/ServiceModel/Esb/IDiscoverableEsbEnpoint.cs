using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.ServiceModel.Esb
{
    /// <summary>
    /// Represents a contract for an ESB endpoint whose methods are discoverable.
    /// </summary>
    public interface IDiscoverableEsbEnpoint
    {
        ServiceMethodList GetServiceMethods(Resource resource);
    }
}
