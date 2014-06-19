using System;
using System.Collections.Generic;
using Dev2.Communication;
using Dev2.DataList.Contract;

// ReSharper disable CheckNamespace
namespace Dev2
// ReSharper restore CheckNamespace
{
    public interface IEsbChannel
    {
        /// <summary>
        /// Executes the request placing it into a transactional scope
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="request">The request.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid ExecuteRequest(IDSFDataObject dataObject, EsbExecuteRequest request, Guid workspaceID, out ErrorResultTO errors);

        /// <summary>
        /// Fetches the server model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        T FetchServerModel<T>(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors);

        /// <summary>
        /// Executes the sub request.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="inputDefs">The input defs.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid ExecuteSubRequest(IDSFDataObject dataObject, Guid workspaceID, string inputDefs, string outputDefs, out ErrorResultTO errors);

        /// <summary>
        /// Finds the service shape.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="resourceID">Name of the service.</param>
        /// <returns></returns>
        string FindServiceShape(Guid workspaceID, Guid resourceID);

        /// <summary>
        /// Shapes for sub request.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="inputDefs">The input defs.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> ShapeForSubRequest(IDSFDataObject dataObject, string inputDefs, string outputDefs, out ErrorResultTO errors);

        /// <summary>
        /// Gets the correct data list.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="compiler">The compiler.</param>
        /// <returns></returns>
        Guid CorrectDataList(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors, IDataListCompiler compiler);

        void ExecuteLogErrorRequest(IDSFDataObject dataObject, Guid workspaceID, string uri, out ErrorResultTO errors);
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
