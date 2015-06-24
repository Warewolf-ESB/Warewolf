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

namespace Dev2.Common.Interfaces.Data.TO
{
    public interface IErrorResultTO
    {
        /// <summary>
        ///     Adds the error.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="checkForDuplicates"></param>
        void AddError(string msg, bool checkForDuplicates = false);

        /// <summary>
        ///     Remove the error from the list
        /// </summary>
        /// <param name="msg"></param>
        void RemoveError(string msg);

        /// <summary>
        ///     Fetches the errors.
        /// </summary>
        /// <returns></returns>
        IList<string> FetchErrors();

        /// <summary>
        ///     Determines whether this instance has errors.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance has errors; otherwise, <c>false</c>.
        /// </returns>
        bool HasErrors();

        /// <summary>
        ///     Merges the errors.
        /// </summary>
        /// <param name="toMerge">To merge.</param>
        void MergeErrors(IErrorResultTO toMerge);

        void ClearErrors();

        /// <summary>
        ///     Makes the error collection user ready.
        /// </summary>
        /// <returns></returns>
        string MakeUserReady();

        /// <summary>
        ///     Makes the error collection user ready.
        /// </summary>
        /// <returns></returns>
        string MakeDisplayReady();

        /// <summary>
        ///     Makes the error collection data list insert ready.
        /// </summary>
        /// <returns></returns>
        string MakeDataListReady(bool AsXML = true);
    }
}