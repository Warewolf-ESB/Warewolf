using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Common.ServiceModel;

namespace Dev2.Runtime.ServiceModel.Data
{
    /// <summary>
    /// Describes a resource.
    /// </summary>
    public interface IResource : IEquatable<IResource>
    {
        /// <summary>
        /// The resource ID that uniquely identifies the resource.
        /// </summary>
        Guid ResourceID { get; set; }

        /// <summary>
        /// The version that uniquely identifies the resource.
        /// </summary>
        Version Version { get; set; }

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
        /// Gets or sets the file path of the resource.
        /// <remarks>
        /// Must only be used by the catalog!
        /// </remarks>   
        /// </summary>   
        string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the author roles.
        /// </summary>
        string AuthorRoles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is upgraded.
        /// </summary>
        bool IsUpgraded { get; set; }

        /// <summary>
        /// Saves this resource to the specified workspace.
        /// </summary>
        /// <param name="workspaceID">The workspace ID.</param>
        void Save(Guid workspaceID);

        /// <summary>
        /// Gets the XML representation of this resource.
        /// </summary>
        /// <returns>The XML representation of this resource.</returns>
        XElement ToXml();

        /// <summary>
        /// Determines whether the given user roles are in the <see cref="AuthorRoles"/>.
        /// </summary>
        /// <param name="userRoles">The user roles to be queried.</param>
        /// <returns>
        ///   <c>true</c> if the user roles are in the <see cref="AuthorRoles"/>; otherwise, <c>false</c>.
        /// </returns>
        bool IsUserInAuthorRoles(string userRoles);

        /// <summary>
        /// If this instance <see cref="IsUpgraded"/> then sets the ID, Version, Name and ResourceType attributes on the given XML.
        /// </summary>
        /// <param name="xml">The XML to be upgraded.</param>
        /// <returns>The XML with the additional attributes set.</returns>
        XElement UpgradeXml(XElement xml);

        List<ResourceForTree> Dependencies { get; set; }
    }
}
