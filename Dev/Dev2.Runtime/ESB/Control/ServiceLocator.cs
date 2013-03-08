using System.IO;
using System.Runtime.Serialization;
using Dev2.DynamicServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Workspaces;

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
        /// <param name="fromHost">From workspace.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.InvalidDataException">Empty or null service passed in</exception>
        /// <exception cref="System.Runtime.Serialization.InvalidDataContractException">Null workspace</exception>
        public DynamicService FindServiceByName(string serviceName, IDynamicServicesHost fromHost)
        {

            if(string.IsNullOrEmpty(serviceName))
            {
                throw new InvalidDataException("Empty or null service passed in");
            }

            if(fromHost == null)
            {
                throw new InvalidDataContractException("Null workspace");
            }

            IEnumerable<DynamicService> service;
            IDynamicServicesHost theHost = fromHost;

            theHost.LockServices();

            try
            {
                service = from c in theHost.Services
                          where c.Name.Trim().Equals(serviceName.Trim(), StringComparison.CurrentCultureIgnoreCase)
                          select c;
            }
            finally
            {
                theHost.UnlockServices();
            }

            return service.ToList().FirstOrDefault();
        }

        #endregion

    }
}
