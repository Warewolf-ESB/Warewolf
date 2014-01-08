using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.InterfaceImplementors
{
    /// <summary>
    /// A deploy DTO.
    /// </summary>
    public class DeployDto : IDeployDto
    {
        #region ResourceModels

        /// <summary>
        /// Gets or sets the resource models.
        /// </summary>
        public IList<IResourceModel> ResourceModels
        {
            get;
            set;
        }

        #endregion
    }
}
