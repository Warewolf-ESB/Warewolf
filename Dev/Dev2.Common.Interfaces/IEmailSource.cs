using System;

namespace Dev2.Common.Interfaces
{
    public interface IEmailSource
    {
        string Host { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        int Port { get; set; }
        bool EnableSsl { get; set; }
        int Timeout { get; set; }
        /// <summary>
        /// This must NEVER be persisted - here for JSON transport only!
        /// </summary>
        string TestFromAddress { get; set; }
        /// <summary>
        /// This must NEVER be persisted - here for JSON transport only!
        /// </summary>
        string TestToAddress { get; set; }
        string DataList { get; set; }
        Version Version { get; set; }
        bool ReloadActions { get; set; }
        /// <summary>
        /// The resource ID that uniquely identifies the resource.
        /// </summary>   
        Guid ResourceID { get; set; }
        /// <summary>
        /// The display name of the resource.
        /// </summary>
        string ResourceName { get; set; }
    }
}