
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
        /// <param name="replaceCount">The replace count.</param>
        /// <param name="entryToReplaceIn"></param>
        /// <returns></returns>
        IDev2DataListUpsertPayloadBuilder<string> Replace(Guid exIdx,
            string expression,
            string oldString,
            string newString,
            bool caseMatch,
            IDev2DataListUpsertPayloadBuilder<string> payloadBuilder,
            out ErrorResultTO errors,
            out int replaceCount,
            out IBinaryDataListEntry entryToReplaceIn);
        
        /// <summary>
        /// Replaces a value in and entry with a new value.
        /// </summary>
        /// <param name="stringToSearch">The old string.</param>
        /// <param name="findString">The old string.</param>
        /// <param name="replacementString">The new string.</param>
        /// <param name="caseMatch">if set to <c>true</c> [case match].</param>
        /// <param name="errors">The errors.</param>
        /// <param name="replaceCount">The replace count.</param>
        /// <returns></returns>
        string Replace(
            string stringToSearch,
            string findString,
            string replacementString,
            bool caseMatch,
            out ErrorResultTO errors,
            ref int replaceCount
            );
    }
}
