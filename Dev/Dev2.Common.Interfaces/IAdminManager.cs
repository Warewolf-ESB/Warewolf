using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    /// <summary>
    /// an admin manager is responsible for all operations that manages the state of a warewolf server
    /// </summary>
    public interface IAdminManager
    {

        /// <summary>
        /// Gets the Warewolf Server version
        /// </summary>
        /// <returns>The version of the Server. Default version text of "less than 0.4.19.1" is returned
        /// if the server is older than that version.</returns>
        string GetServerVersion();

        Dictionary<string,string> GetServerInformation();

        string GetMinSupportedServerVersion();
    }




}
