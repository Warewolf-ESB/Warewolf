
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    /// <summary>
    /// Defines the requirements for a deploy DTO.
    /// </summary>
    public interface IDeployDto
    {
        /// <summary>
        /// Gets or sets the resource models.
        /// </summary>
        IList<IResourceModel> ResourceModels
        {
            get;
            set;
        }

    }
}
