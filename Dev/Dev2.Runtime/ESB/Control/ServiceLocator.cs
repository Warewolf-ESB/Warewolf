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
using System.IO;
using System.Linq;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Control
{
    /// <summary>
    /// Used to locate a service to execute ;)
    /// </summary>
    public class ServiceLocator : IServiceLocator
    {
        readonly IPerformanceCounter _perfCounter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter("Count of requests for workflows which don’t exist");
        private readonly IResourceCatalog _resourceCatalog = ResourceCatalog.Instance;
        #region New Mgt Methods

        /// <summary>
        /// Finds the service by name
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <exception cref="System.IO.InvalidDataException">Empty or null service passed in</exception>
        /// <exception cref="System.Runtime.Serialization.InvalidDataContractException">Null workspace</exception>
        public DynamicService FindService(string serviceName, Guid workspaceID)
        {
            if(string.IsNullOrEmpty(serviceName))
                throw new InvalidDataException(ErrorResource.ServiceIsNull);
            var res = _resourceCatalog.GetResource(workspaceID, serviceName);
            DynamicService ret = null;
            if (res != null)
            {
                ret = ServiceActionRepo.Instance.ReadCache(res.ResourceID);
            }
            if (ret == null)
            {
                ret = _resourceCatalog.GetDynamicObjects<DynamicService>(workspaceID, serviceName).FirstOrDefault();                
                if (ret == null)
                {
                    _perfCounter.Increment();
                }
            }
            return ret;
        }

        /// <summary>
        /// Finds the service by ID
        /// </summary>
        /// <param name="serviceID">ID of the service.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <exception cref="System.IO.InvalidDataException">Empty or null service passed in</exception>
        /// <exception cref="System.Runtime.Serialization.InvalidDataContractException">Null workspace</exception>
        public DynamicService FindService(Guid serviceID, Guid workspaceID)
        {
            if(serviceID == Guid.Empty)
                throw new InvalidDataException(ErrorResource.ServiceIsNull);
            var firstOrDefault = ServiceActionRepo.Instance.ReadCache(serviceID);
                        
            if (firstOrDefault == null)
            {
                firstOrDefault = _resourceCatalog.GetDynamicObjects<DynamicService>(workspaceID, serviceID).FirstOrDefault();
                if (firstOrDefault != null)
                {
                    firstOrDefault.ServiceId = serviceID;
                    firstOrDefault.Actions.ForEach(action =>
                    {
                        action.ServiceID = serviceID;
                    });
                }
                if (firstOrDefault == null)
                {
                    _perfCounter.Increment();
                }
            }

            return firstOrDefault;
        }

        /// <summary>
        /// Finds the source by name
        /// </summary>
        /// <param name="sourceName">Name of the source.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <exception cref="System.IO.InvalidDataException">Empty or null service passed in</exception>
        /// <exception cref="System.Runtime.Serialization.InvalidDataContractException">Null workspace</exception>
        public Source FindSourceByName(string sourceName, Guid workspaceID)
        {
            if (string.IsNullOrEmpty(sourceName))
                throw new InvalidDataException(ErrorResource.ServiceIsNull);
            return _resourceCatalog.GetDynamicObjects<Source>(workspaceID, sourceName).FirstOrDefault();
        }

        #endregion

    }


}
