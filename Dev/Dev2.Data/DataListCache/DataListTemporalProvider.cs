using Dev2.Common;
using Dev2.Data;
using Dev2.Data.DataListCache;
using Dev2.DataList.Contract.Binary_Objects;
using System;
using System.Collections.Concurrent;

namespace Dev2.DataList.Contract.Persistence {

    /// <summary>
    /// Basic in memory provider for client and test
    /// </summary>
    public class DataListTemporalProvider : IDataListPersistenceProvider {

        private static readonly ConcurrentDictionary<Guid, IBinaryDataList> _repo = new ConcurrentDictionary<Guid, IBinaryDataList>();

        public bool WriteDataList(Guid datalistID, IBinaryDataList datalist, ErrorResultTO errors) {
            bool result = false;

            if (datalistID != GlobalConstants.NullDataListID)
            {
                _repo[datalistID] = datalist;
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Reads the datalist.
        /// </summary>
        /// <param name="datalistID">The datalist unique identifier.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public IBinaryDataList ReadDatalist(Guid datalistID, ErrorResultTO errors) {

            IBinaryDataList result;
            _repo.TryGetValue(datalistID, out result);
            return result;
        }

        public bool DeleteDataList(Guid id, bool onlyIfNotPersisted)
        {
            bool result = false;
            try
            {
                IBinaryDataList tmp;
                if (_repo.TryRemove(id, out tmp)) // cache miss, check persisted DL cache?
                {
                        result = true;
                }

                BackgroundDispatcher.Instance.Add(tmp);

            }
            catch (Exception ex)
            {
                this.LogError(ex);
                /* Fail safe */
            }

            return result;
        }

    }
}
