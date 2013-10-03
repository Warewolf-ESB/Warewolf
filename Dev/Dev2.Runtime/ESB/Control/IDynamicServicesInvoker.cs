using System;
using Dev2.DataList.Contract;
using Dev2.Runtime.ESB.Execution;

namespace Dev2.DynamicServices {
    public interface IDynamicServicesInvoker {

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
        /// <param name="serviceID">The service unique identifier.</param>
        /// <param name="isLocal">if set to <c>true</c> [is local].</param>
        /// <param name="masterDataListID">The master data list unique identifier.</param>
        /// <returns></returns>
        EsbExecutionContainer GenerateInvokeContainer(IDSFDataObject dataObject, Guid serviceID, bool isLocal, Guid masterDataListID);
    }
}
