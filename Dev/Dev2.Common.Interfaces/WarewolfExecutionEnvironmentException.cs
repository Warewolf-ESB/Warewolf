/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;

namespace Dev2.Common.Interfaces
{
    /// <summary>
    /// environment variable errors are considered to be user caused errors
    /// </summary>
    public class WarewolfExecutionEnvironmentException : WarewolfException
    {
        //NOTE: environment variable errors are considered to be user caused errors
        public WarewolfExecutionEnvironmentException(string message, Exception innerException = null)
            : base(message, innerException, ExceptionType.Execution, ExceptionSeverity.User)
        {
        }

        /// <summary>
        /// resource exceptions errors are considered to be inclusive of (but not limited to) code author mishandle caused errors, so they can be investigated holistically
        /// </summary>
        public class WarewolfResourceException : WarewolfException
        {
            //NOTE: resource exceptions errors are considered to be inclusive of (but not limited to) code author mishandle caused errors, so they can be investigated holistically
            public WarewolfResourceException(string message, Exception innerException = null)
                : base(message, innerException, ExceptionType.Execution, ExceptionSeverity.Critical)
            {
            }
        }

        /// <summary>
        /// thread abort exceptions errors are considered to be code author mishandle caused errors, so they can be investigated as bugs
        /// </summary>
        public class WarewolfThreadAbortException : WarewolfException
        {
            //NOTE: thread abort exceptions  errors are considered to be code author mishandle caused errors, so they can be investigated as bugs
            public WarewolfThreadAbortException(string message, Exception innerException = null)
                : base(message, innerException, ExceptionType.Execution, ExceptionSeverity.Critical)
            {
            }
        }
    }
}
