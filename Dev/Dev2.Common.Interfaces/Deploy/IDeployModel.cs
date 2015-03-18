using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces.Deploy
{
    public interface IDeployModel
    {
        /// <summary>
        /// The connected Server        
        /// /// </summary>
        IServer Server { get; }
        object[] SelectedItems { get; set; }

        /// <summary>
        /// Deploy a resource to a connected server
        /// </summary>
        /// <param name="resource"></param>
        void Deploy(IResource resource);
        /// <summary>
        /// Get the dependencies of a resource
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IList<IResource> GetDependancies(Guid id);
        /// <summary>
        /// Does the user have permissions to Deploy a resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        bool CanDeploy(IResource resource);
    }

    public interface IDeployModelFactory
    {
        IDeployModel Create(IServer server);
    }

}