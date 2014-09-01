using System;
using System.IO;
using System.Linq;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;

namespace Dev2.Runtime.ESB.Control
{
    /// <summary>
    /// Used to locate a service to execute ;)
    /// </summary>
    public class ServiceLocator
    {
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
                throw new InvalidDataException("Empty or null service passed in");
            }

            var services = ResourceCatalog.Instance.GetDynamicObjects<DynamicService>(workspaceID, serviceName);
            return services.FirstOrDefault();
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
                throw new InvalidDataException("Empty or null service passed in");
            }

            var services = ResourceCatalog.Instance.GetDynamicObjects<DynamicService>(workspaceID, serviceID);
            var firstOrDefault = services.FirstOrDefault();
            if(firstOrDefault != null)
            {
                firstOrDefault.ServiceId = serviceID;
                firstOrDefault.Actions.ForEach(action =>
                {
                    action.ServiceID = serviceID;
                });
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

            if(string.IsNullOrEmpty(sourceName))
            {
                throw new InvalidDataException("Empty or null service passed in");
            }

            var sources = ResourceCatalog.Instance.GetDynamicObjects<Source>(workspaceID, sourceName);
            return sources.FirstOrDefault();
        }

        #endregion

    }
}
