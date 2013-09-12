using Dev2.DataList.Contract;
using System;
using Dev2.Network.Messaging;

namespace Dev2
{
    /// <summary>
    /// Used studio side to replicate old channel functionality
    /// </summary>
    public interface IStudioEsbChannel
    {
        string ExecuteCommand(string xmlRequest, Guid workspaceID, Guid dataListID);
        INetworkMessage SendMessage<T>(T message) where T : INetworkMessage, new();
    }

    public interface IEsbChannel
    {
        /// <summary>
        /// Executes the request placing it into a transactional scope
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid ExecuteRequest(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors);

        T FetchServerModel<T>(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors);

        /// <summary>
        /// Executes the transactionally scoped request.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid ExecuteTransactionallyScopedRequest(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors);

        /// <summary>
        /// Finds the service shape.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="serviceInputs">if set to <c>true</c> [service inputs].</param>
        /// <returns></returns>
        string FindServiceShape(Guid workspaceID, string serviceName, bool serviceInputs);

    }

    public interface IEsbWorkspaceChannel : IEsbChannel
    {
            
    }

    public interface IEsbActivityChannel
    {
        bool ExecuteParallel(IEsbActivityInstruction[] instructions);
    }

    public interface IEsbActivityInstruction
    {
        string Instruction { get; }
        string Result { get; set; }
        Guid DataListID { get; set; }
    }
}
