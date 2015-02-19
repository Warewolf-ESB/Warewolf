using System;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Versioning;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Common.Interfaces
{
    public interface IResourceDefinition
    {
        /// <summary>
        /// The resource ID that uniquely identifies the resource.
        /// </summary>
        // ReSharper disable InconsistentNaming
        Guid ResourceID { get; set; }
        /// <summary>
        /// The display name of the resource.
        /// </summary>
        string ResourceName { get; set; }
        /// <summary>
        /// Gets or sets the type of the resource.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        ResourceType ResourceType { get; set; }
        /// <summary>
        /// Gets or sets the category of the resource.
        /// </summary>   
        string ResourceCategory { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [is valid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is valid]; otherwise, <c>false</c>.
        /// </value>
        bool IsValid { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [is new resource].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is new resource]; otherwise, <c>false</c>.
        /// </value>
        bool IsNewResource { get; set; }

        /// <summary>
        /// Gets or sets the data list.
        /// </summary>
        /// <value>
        /// The data list.
        /// </value>
        string DataList { get; set; }
        /// <summary>
        /// Gets or sets the inputs.
        /// </summary>
        /// <value>
        /// The inputs.
        /// </value>
        string Inputs { get; set; }
        /// <summary>
        /// Gets or sets the outputs.
        /// </summary>
        /// <value>
        /// The outputs.
        /// </value>
        string Outputs { get; set; }
        IVersionInfo VersionInfo { get; set; }
        /// <summary>
        /// Gets or sets the permissions of the resource
        /// </summary>
        Permissions Permissions { get; set; }
    }
}