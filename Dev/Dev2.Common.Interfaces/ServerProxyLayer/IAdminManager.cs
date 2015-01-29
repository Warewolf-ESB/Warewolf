namespace Dev2.Common.Interfaces.ServerProxyLayer
{
    /// <summary>
    /// an admin manager is responsible for all operations that manages the state of a warewolf server
    /// </summary>
    public interface IAdminManager
    {
        /// <summary>
        /// Get the execution queue depth. ie the number of items waiting for execution
        /// </summary>
        /// <returns></returns>
        int GetCurrentQueueDepth();
        /// <summary>
        /// Get the maximum queue depth before warewolf will reject new requests
        /// </summary>
        /// <returns></returns>
        int GetMaxQueueDepth();
        /// <summary>
        /// Get the maximum number of concurrent executions available on a warewolf server
        /// </summary>
        /// <returns></returns>
        int GetMaxThreadCount();
        /// <summary>
        /// Set the maximum queue depth before warewolf rejects messages
        /// </summary>
        void SetMaxQueueDepth(int depth);
        /// <summary>
        /// Set the maximum number of concurrent execution available 
        /// </summary>
        void SetMaxThreadCount(int count);

        /// <summary>
        /// Gets the Warewolf Server version
        /// </summary>
        /// <returns>The version of the Server. Default version text of "less than 0.4.19.1" is returned
        /// if the server is older than that version.</returns>
        string GetServerVersion();
    }




}
