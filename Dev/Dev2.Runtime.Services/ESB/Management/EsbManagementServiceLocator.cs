using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.DynamicServices;

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

        public static List<DynamicService> GetServices()
        {
            var result = new List<DynamicService>();

            var locator = new EsbManagementServiceLocator();

            foreach(var endpoint in locator.FetchManagmentServices())
            {
                var service = endpoint.CreateServiceEntry();
                if(service.Compile())
                {
                    result.Add(service);
                }
                else
                {
                    ServerLogger.LogError("EsbManagementServiceLocator", new Exception("Failed to load management service [ " + endpoint.HandlesType() + " ]"));
                }
            }

            return result;
        }
    }
}
