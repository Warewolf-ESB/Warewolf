using System;

namespace Dev2.Common.Interfaces
{
    public interface IComPlugin
    {
        string ClsId { get; set; }
        bool Is32Bit { get; set; }
        string ComName { get; set; }
        bool IsSource { get; }
        bool IsService { get; }
        bool IsFolder { get; }
        bool IsReservedService { get; }
        bool IsServer { get; }
        bool IsResourceVersion { get; }
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
    }
}