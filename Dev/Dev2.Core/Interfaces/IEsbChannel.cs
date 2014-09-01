using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
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
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid ExecuteRequest(IDSFDataObject dataObject, EsbExecuteRequest request, Guid workspaceId, out ErrorResultTO errors);

        /// <summary>
        /// Fetches the server model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        T FetchServerModel<T>(IDSFDataObject dataObject, Guid workspaceId, out ErrorResultTO errors);

        /// <summary>
        /// Executes the sub request.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <param name="inputDefs">The input defs.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid ExecuteSubRequest(IDSFDataObject dataObject, Guid workspaceId, string inputDefs, string outputDefs, out ErrorResultTO errors);

        /// <summary>
        /// Finds the service shape.
        /// </summary>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <param name="resourceId">Name of the service.</param>
        /// <returns></returns>
        string FindServiceShape(Guid workspaceId, Guid resourceId);

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
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="compiler">The compiler.</param>
        /// <returns></returns>
        Guid CorrectDataList(IDSFDataObject dataObject, Guid workspaceId, out ErrorResultTO errors, IDataListCompiler compiler);

        void ExecuteLogErrorRequest(IDSFDataObject dataObject, Guid workspaceId, string uri, out ErrorResultTO errors);
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
// ReSharper disable InconsistentNaming
        Guid DataListID { get; set; }
// ReSharper restore InconsistentNaming
    }
}
