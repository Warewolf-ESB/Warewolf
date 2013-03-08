using System.Collections.Generic;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management.Sources;

namespace Dev2.Runtime.ESB.Management
{
    /// <summary>
    /// Responsible for loading all the managment services ;)
    /// Replaces GetDefaultServices() in DynamicservicesHost
    /// </summary>
    public class EsbManagementServiceLocator : SpookyAction<IEsbManagementEndpoint, string>
    {

        /// <summary>
        /// Loads the managment services.
        /// </summary>
        /// <returns></returns>
        public IList<IEsbManagementEndpoint> FetchManagmentServices()
        {
            return FindAll();
        }

        /// <summary>
        /// Locates the management service.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns></returns>
        public IEsbManagementEndpoint LocateManagementService(string serviceName)
        {
            return FindMatch(serviceName);
        }

        /// <summary>
        /// Locates the management source.
        /// </summary>
        /// <returns></returns>
        public Source LocateManagementSource()
        {
            return new ManagementServiceSource().CreateSourceEntry();
        }

    }
}
