using Dev2.Runtime.ServiceModel.Data;
using System;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Esb
{
    /// <summary>
    /// Represents a contract for an ESB endpoint whose methods are executable.
    /// </summary>
    public interface IExecutableEsbEnpoint
    {
        IOutputDescription TestServiceMethod(Resource resource, ServiceMethod serviceMethod);
        Guid ExecuteServiceMethod(Resource resource, ServiceMethod serviceMethod);
    }
}
