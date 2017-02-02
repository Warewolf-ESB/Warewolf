/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Text.RegularExpressions;
using Dev2.Data.Interfaces;
using Dev2.Data.TO;

namespace Dev2.Data.Operations
{
    public class Dev2ReplaceOperation : IDev2ReplaceOperation
    {
        private const RegexOptions NoneCompiled = RegexOptions.None | RegexOptions.Compiled;
        private const RegexOptions IgnoreCaseCompiled = RegexOptions.IgnoreCase | RegexOptions.Compiled;

        #region Ctor

        // REVIEW - This needs to be an internal constructor accessed via a factory!
        internal Dev2ReplaceOperation()
        {
        }

        #endregion 

        #region Methods

        
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
        public string Replace(string stringToSearch, string findString, string replacementString, bool caseMatch, out ErrorResultTO errors, ref int replaceCount)
        {

            var oldString = stringToSearch;
            ErrorResultTO allErrors = new ErrorResultTO();
            errors = new ErrorResultTO();
            allErrors.MergeErrors(errors);

            var regexOptions = caseMatch ? NoneCompiled : IgnoreCaseCompiled;

            Regex regex = new Regex(Regex.Escape(findString), regexOptions);
            string tmpVal = oldString;
            replaceCount += regex.Matches(tmpVal).Count;
            var replaceValue = regex.Replace(tmpVal, replacementString);
            errors = allErrors;
            return replaceValue;
        }

        #endregion
    }
}
