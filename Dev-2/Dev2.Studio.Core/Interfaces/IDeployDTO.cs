
using System.Collections.Generic;

namespace Dev2.Studio.Core.Interfaces
{
    /// <summary>
    /// Defines the requirements for a deploy DTO.
    /// </summary>
    public interface IDeployDTO
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
