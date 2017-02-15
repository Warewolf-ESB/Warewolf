using System.Net;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Runtime.ServiceModel.Data
{
    public interface IWebSource : IResource
    {
        string Address { get; set; }
        string DefaultQuery { get; set; }
        AuthenticationType AuthenticationType { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        /// <summary>
        /// This must NEVER be persisted - here for JSON transport only!
        /// </summary>
        string Response { get; set; }
        /// <summary>
        /// This must NEVER be persisted - here so that we only instantiate once!
        /// </summary>
        WebClient Client { get; set; }
    }
}