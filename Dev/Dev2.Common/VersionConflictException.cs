/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Warewolf.Resource.Errors;

namespace Dev2.Common
{
    public class VersionConflictException : Exception
    {
        private Version SourceVersionNumber { get; set; }
        private Version DestVersionNumber { get; set; }

        public VersionConflictException(Version sourceVersionNumber, Version destVersionNumber)
        {
            SourceVersionNumber = sourceVersionNumber;
            DestVersionNumber = destVersionNumber;
        }

        #region Overrides of Exception

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string ("").
        /// </returns>
        public override string Message => string.Format(ErrorResource.ServerVersionsIncompatiable, SourceVersionNumber, DestVersionNumber);

        #endregion Overrides of Exception
    }
}