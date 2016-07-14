using System.Collections.Generic;
using Dev2.Runtime.ESB.Management;

namespace Dev2.Runtime.Interfaces
{
    public interface IEsbManagementServiceLocator
    {
        /// <summary>
        /// Loads the managment services.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IEsbManagementEndpoint> FetchManagmentServices();

        /// <summary>
        /// Locates the management service.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns></returns>
        IEsbManagementEndpoint LocateManagementService(string serviceName);
    }
}