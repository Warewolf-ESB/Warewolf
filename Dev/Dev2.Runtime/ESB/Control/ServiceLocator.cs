#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
        readonly IPerformanceCounter _perfCounter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter("Count of requests for workflows which don't exist");
        readonly IResourceCatalog _resourceCatalog = ResourceCatalog.Instance;
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
            {
                throw new InvalidDataException(ErrorResource.ServiceIsNull);
            }

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
            {
                throw new InvalidDataException(ErrorResource.ServiceIsNull);
            }

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
            {
                throw new InvalidDataException(ErrorResource.ServiceIsNull);
            }

            return _resourceCatalog.GetDynamicObjects<Source>(workspaceID, sourceName).FirstOrDefault();
        }

        #endregion

    }


}
