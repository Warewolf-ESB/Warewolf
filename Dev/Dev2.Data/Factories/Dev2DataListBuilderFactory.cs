
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Data.Builders;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;

namespace Dev2.Data.Factories
{
    /// <summary>
    /// Factory for creating DataList Builder objects to protect allowed types
    /// </summary>
    public static class Dev2DataListBuilderFactory
    {

        /// <summary>
        /// Creates the string data list upsert builder.
        /// </summary>
        /// <returns></returns>
        public static IDev2DataListUpsertPayloadBuilder<string> CreateStringDataListUpsertBuilder(bool isIterativePayload)
        {
            return new Dev2DataListUpsertPayloadBuilder<string>(isIterativePayload);
        } 
        
        /// <summary>
        /// Creates the string data list upsert builder.
        /// </summary>
        /// <returns></returns>
        public static IDev2DataListUpsertPayloadBuilder<List<string>> CreateStringListDataListUpsertBuilder()
        {
            return new Dev2DataListUpsertPayloadBuilder<List<string>>(false);
        }

        /// <summary>
        /// Creates the string data list upsert builder.
        /// </summary>
        /// <returns></returns>
        public static IDev2DataListUpsertPayloadBuilder<string> CreateStringDataListUpsertBuilder()
        {
            return new Dev2DataListUpsertPayloadBuilder<string>(false);
        }


        /// <summary>
        /// Creates the binary data list upsert builder.
        /// </summary>
        /// <returns></returns>
        public static IDev2DataListUpsertPayloadBuilder<IBinaryDataListEntry> CreateBinaryDataListUpsertBuilder(bool isIterativePayload)
        {
            return new Dev2DataListUpsertPayloadBuilder<IBinaryDataListEntry>(isIterativePayload);
        }

        /// <summary>
        /// Creates the binary data list upsert builder.
        /// </summary>
        /// <returns></returns>
        public static IDev2DataListUpsertPayloadBuilder<IBinaryDataListEntry> CreateBinaryDataListUpsertBuilder()
        {
            return new Dev2DataListUpsertPayloadBuilder<IBinaryDataListEntry>(false);
        }
    }
}
