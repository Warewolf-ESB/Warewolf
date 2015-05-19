
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
                    Dev2Logger.Log.Error("EsbManagementServiceLocator", new Exception("Failed to load management service [ " + endpoint.HandlesType() + " ]"));
                }
            }

            return result;
        }
    }
}
