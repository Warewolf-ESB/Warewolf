/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.TO;

namespace Dev2.Data.Interfaces
{
    public interface IDev2ReplaceOperation
    {
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
