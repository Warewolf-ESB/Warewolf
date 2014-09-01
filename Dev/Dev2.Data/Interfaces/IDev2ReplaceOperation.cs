using System;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;

namespace Dev2.Data.Interfaces
{
    public interface IDev2ReplaceOperation
    {

        /// <summary>
        /// Replaces a value in and entry with a new value.
        /// </summary>
        /// <param name="exIdx">The execution id.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="oldString">The old string.</param>
        /// <param name="newString">The new string.</param>
        /// <param name="caseMatch">if set to <c>true</c> [case match].</param>
        /// <param name="payloadBuilder">The payload builder.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="ReplaceCount">The replace count.</param>
        /// <returns></returns>
        IDev2DataListUpsertPayloadBuilder<string> Replace(Guid exIdx,
            string expression,
            string oldString,
            string newString,
            bool caseMatch,
            IDev2DataListUpsertPayloadBuilder<string> payloadBuilder,
            out ErrorResultTO errors,
            out int ReplaceCount,
            out IBinaryDataListEntry entryToReplaceIn);
    }
}
