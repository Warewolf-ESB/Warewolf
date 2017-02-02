using System;
using Dev2.DynamicServices;

namespace Dev2.Runtime.ESB.Control
{
    public interface IServiceLocator
    {
        /// <summary>
        /// Finds the service by name
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <exception cref="System.IO.InvalidDataException">Empty or null service passed in</exception>
        /// <exception cref="System.Runtime.Serialization.InvalidDataContractException">Null workspace</exception>
        DynamicService FindService(string serviceName, Guid workspaceID);

        /// <summary>
        /// Finds the service by ID
        /// </summary>
        /// <param name="serviceID">ID of the service.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <exception cref="System.IO.InvalidDataException">Empty or null service passed in</exception>
        /// <exception cref="System.Runtime.Serialization.InvalidDataContractException">Null workspace</exception>
        DynamicService FindService(Guid serviceID, Guid workspaceID);

        /// <summary>
        /// Finds the source by name
        /// </summary>
        /// <param name="sourceName">Name of the source.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <exception cref="System.IO.InvalidDataException">Empty or null service passed in</exception>
        /// <exception cref="System.Runtime.Serialization.InvalidDataContractException">Null workspace</exception>
        Source FindSourceByName(string sourceName, Guid workspaceID);
    }
}