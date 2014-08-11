using System;
using Dev2.DataList.Contract;
using Dev2.Runtime.ESB.Execution;

// ReSharper disable CheckNamespace
namespace Dev2.DynamicServices
{
    // ReSharper restore CheckNamespace

    public interface IEsbServiceInvoker
    {

        /// <summary>
        /// Invokes the specified data object.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        Guid Invoke(IDSFDataObject dataObject, out ErrorResultTO errors);

        /// <summary>
        /// Generates the invoke container.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="serviceId">The service unique identifier.</param>
        /// <param name="isLocal">if set to <c>true</c> [is local].</param>
        /// <param name="masterDataListId">The master data list unique identifier.</param>
        /// <returns></returns>
        EsbExecutionContainer GenerateInvokeContainer(IDSFDataObject dataObject, Guid serviceId, bool isLocal, Guid masterDataListId);

        /// <summary>
        /// Generates the invoke container.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="isLocalInvoke">if set to <c>true</c> [is local invoke].</param>
        /// <param name="masterDataListId">The master data list unique identifier.</param>
        /// <returns></returns>
        EsbExecutionContainer GenerateInvokeContainer(IDSFDataObject dataObject, String serviceName, bool isLocalInvoke, Guid masterDataListId);
    }
}
