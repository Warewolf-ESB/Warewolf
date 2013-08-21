using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList.Contract.Builders
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
