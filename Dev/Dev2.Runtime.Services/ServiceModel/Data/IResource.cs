using System;
using Dev2.Common.ServiceModel;

namespace Dev2.Runtime.ServiceModel.Data
{
    /// <summary>
    /// Describes a resource.
    /// </summary>
    /// <author>Trevor.Williams-Ros</author>
    /// <date>2013/03/02</date>
    public interface IResource : IEquatable<IResource>
    {
        /// <summary>
        /// The resource ID that uniquely identifies the resource.
        /// </summary>
        Guid ResourceID { get; set; }

        /// <summary>
        /// The version that uniquely identifies the resource.
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// The display name of the resource.
        /// </summary>
        string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the type of the resource.
        /// </summary>
        ResourceType ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the category of the resource.
        /// </summary>   
        string ResourcePath { get; set; }

        /// <summary>
        /// Gets or sets the contents of the resource.
        /// </summary>   
        string Contents { get; set; }

        /// <summary>
        /// Gets or sets the file path of the resource.
        /// <remarks>
        /// Must only be used by the catalog!
        /// </remarks>   /// </summary>   
        string FilePath { get; set; }
    }
}