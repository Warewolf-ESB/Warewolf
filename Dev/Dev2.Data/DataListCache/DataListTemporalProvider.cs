
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.DataListCache {

    /// <summary>
    /// Basic in memory provider for client and test
    /// </summary>
    public class DataListTemporalProvider : IDataListPersistenceProvider {

        private static readonly ConcurrentDictionary<Guid, IBinaryDataList> Repo = new ConcurrentDictionary<Guid, IBinaryDataList>();

        public bool WriteDataList(Guid datalistID, IBinaryDataList datalist, ErrorResultTO errors) {
            bool result = false;

            if (datalistID != GlobalConstants.NullDataListID)
            {
                Repo[datalistID] = datalist;
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
            Repo.TryGetValue(datalistID, out result);
            return result;
        }

        public bool DeleteDataList(Guid id, bool onlyIfNotPersisted)
        {
            bool result = false;
            try
            {
                IBinaryDataList tmp;
                if (Repo.TryRemove(id, out tmp)) // cache miss, check persisted DL cache?
                {
                        result = true;
                }

                BackgroundDispatcher.Instance.Add(tmp);

            }
            catch (Exception ex)
            {
                Dev2Logger.Log.Error(ex);
                /* Fail safe */
            }

            return result;
        }

    }
}
