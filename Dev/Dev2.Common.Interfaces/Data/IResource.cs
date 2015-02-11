/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Versioning;
using Newtonsoft.Json;

namespace Dev2.Common.Interfaces.Data
{
    public interface IResource : IEquatable<IResource>
    {
        /// <summary>
        ///     The resource ID that uniquely identifies the resource.
        /// </summary>
        // ReSharper disable InconsistentNaming
        Guid ResourceID { get; set; }

        // ReSharper restore InconsistentNaming

        /// <summary>
        ///     The version that uniquely identifies the resource.
        /// </summary>
        // Version Version { get; set; }
        IVersionInfo VersionInfo { get; set; }

        /// <summary>
        ///     The display name of the resource.
        /// </summary>
        string ResourceName { get; set; }

        /// <summary>
        ///     Gets or sets the type of the resource.
        /// </summary>
        ResourceType ResourceType { get; set; }

        /// <summary>
        ///     Gets or sets the category of the resource.
        /// </summary>
        string ResourcePath { get; set; }

        /// <summary>
        ///     Gets or sets the file path of the resource.
        ///     <remarks>
        ///         Must only be used by the catalog!
        ///     </remarks>
        /// </summary>
        string FilePath { get; set; }

        /// <summary>
        ///     Gets or sets the author roles.
        /// </summary>
        string AuthorRoles { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is upgraded.
        /// </summary>
        bool IsUpgraded { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [is new resource].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [is new resource]; otherwise, <c>false</c>.
        /// </value>
        bool IsNewResource { get; set; }

        IList<IResourceForTree> Dependencies { get; set; }
        bool IsValid { get; set; }
        List<IErrorInfo> Errors { get; set; }

        StringBuilder DataList { get; set; }

        [JsonIgnore]
        string Inputs { get; set; }

        [JsonIgnore]
        string Outputs { get; set; }
        Permissions UserPermissions { get; set; }
        IList<IResource> Children { get; set; }
        bool IsSelected { get; set; }

        /// <summary>
        ///     Gets the XML representation of this resource.
        /// </summary>
        /// <returns>The XML representation of this resource.</returns>
        XElement ToXml();

        /// <summary>
        ///     Gets the string builder for this resource.
        /// </summary>
        /// <returns></returns>
        StringBuilder ToStringBuilder();

        /// <summary>
        ///     Determines whether the given user roles are in the <see cref="AuthorRoles" />.
        /// </summary>
        /// <param name="userRoles">The user roles to be queried.</param>
        /// <returns>
        ///     <c>true</c> if the user roles are in the <see cref="AuthorRoles" />; otherwise, <c>false</c>.
        /// </returns>
        bool IsUserInAuthorRoles(string userRoles);

        /// <summary>
        ///     If this instance <see cref="IsUpgraded" /> then sets the ID, Version, Name and ResourceType attributes on the given
        ///     XML.
        /// </summary>
        /// <param name="xml">The XML to be upgraded.</param>
        /// <param name="resource"></param>
        /// <returns>The XML with the additional attributes set.</returns>
        XElement UpgradeXml(XElement xml, IResource resource);

        void ReadDataList(XElement xml);
        void GetInputsOutputs(XElement xml);
        void SetIsNew(XElement xml);
        // ReSharper disable InconsistentNaming
        void UpdateErrorsBasedOnXML(XElement xml);
        // ReSharper restore InconsistentNaming
    }
}