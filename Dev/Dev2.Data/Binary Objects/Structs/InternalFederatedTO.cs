
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Data.Storage.Binary_Objects;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.Binary_Objects.Structs
{
    /// <summary>
    /// Used to return federated keys ;)
    /// </summary>
    public class InternalFederatedTO
    {

        public FederatedStorageKey ParentKey;

        public FederatedStorageKey ChildKey;

        public IBinaryDataListEntry MasterEntry;

        /// <summary>
        /// Fetches the asynchronous list.
        /// </summary>
        /// <returns></returns>
        public IList<FederatedStorageKey> FetchAsList()
        {
            if (ParentKey != null)
            {
                return new List<FederatedStorageKey> {ParentKey, ChildKey};
            }

            return new List<FederatedStorageKey> { ChildKey };
        }

        /// <summary>
        /// Determines whether [has parent key].
        /// </summary>
        /// <returns></returns>
        public bool HasParentKey()
        {
            return ParentKey != null;
        }

    }
}
