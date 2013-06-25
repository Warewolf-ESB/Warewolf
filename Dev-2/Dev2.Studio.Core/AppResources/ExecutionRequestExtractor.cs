using System;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Network;

namespace Dev2.Studio.Core.AppResources
{
    /// <summary>
    /// Extract a Management service request studio side ;)
    /// </summary>
    public class ExecutionRequestExtractor
    {

        /// <summary>
        /// Fetches the results.
        /// </summary>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="dataListChannel">The data list channel.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public string FetchResults(Guid dlID, INetworkDataListChannel dataListChannel, ErrorResultTO existingErrors)
        {
            // PBI : 7913 -  Travis
            ErrorResultTO allErrors = new ErrorResultTO();
            allErrors.MergeErrors(existingErrors);
            ErrorResultTO errors;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler(dataListChannel);
            IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);
            allErrors.MergeErrors(errors);
            compiler.ForceDeleteDataListByID(dlID); // clean up ;)

            string error;
            IBinaryDataListEntry entry;
            bdl.TryGetEntry(DataListUtil.BuildSystemTagForDataList(enSystemTag.ManagmentServicePayload, false), out entry, out error);
            allErrors.AddError(error);
            var data = string.Empty;
            if (entry != null)
            {
                IBinaryDataListItem item = entry.FetchScalar();
                if (item != null)
                {
                    data = item.TheValue;
                }
            }

            if (allErrors.HasErrors())
            {
                throw new Exception(allErrors.MakeDisplayReady());
            }

            return data;
        }
    }
}
