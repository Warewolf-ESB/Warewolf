using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces
{
    public interface IExchange
    {
        string AutoDiscoverUrl { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        enSourceType Type { get; set; }
        string Path { get; set; }
        int Timeout { get; set; }
        string EmailFrom { get; set; }
        string EmailTo { get; set; }
        /// <summary>
        /// This must NEVER be persisted - here for JSON transport only!
        /// </summary>
        string TestFromAddress { get; set; }
        /// <summary>
        /// This must NEVER be persisted - here for JSON transport only!
        /// </summary>
        string TestToAddress { get; set; }
        string DataList { get; set; }
        /// <summary>
        /// The resource ID that uniquely identifies the resource.
        /// </summary>   
        Guid ResourceID { get; set; }
        /// <summary>
        /// Gets or sets the type of the resource.
        /// </summary>
        string ResourceType { get; set; }
        /// <summary>
        /// The display name of the resource.
        /// </summary>
        string ResourceName { get; set; }
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
        IList<IResourceForTree> Dependencies { get; set; }
    }
}