/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Runtime.Interfaces;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management
{
    /// <summary>
    /// Responsible for loading all the managment services ;)
    /// Replaces GetDefaultServices() in DynamicservicesHost
    /// </summary>
    public class EsbManagementServiceLocator : SpookyAction<IEsbManagementEndpoint, string>, IEsbManagementServiceLocator
    {
        /// <summary>
        /// Loads the managment services.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IEsbManagementEndpoint> FetchManagmentServices()
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

        public static IEnumerable<DynamicService> GetServices()
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
                    Dev2Logger.Error("EsbManagementServiceLocator", new Exception(string.Format(ErrorResource.FailedToLoadManagementService, endpoint.HandlesType())));
                }
            }

            return result;
        }
    }
}
