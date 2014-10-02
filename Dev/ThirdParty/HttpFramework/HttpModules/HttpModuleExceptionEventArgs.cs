
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

namespace HttpFramework.HttpModules
{
    /// <summary>
    /// Used to inform http server that 
    /// </summary>
    public class HttpModuleExceptionEventArgs : EventArgs
    {
        private readonly Exception _exception;

        /// <summary>
        /// Eventarguments used when an exception is thrown by a module
        /// </summary>
        /// <param name="e">the exception</param>
        public HttpModuleExceptionEventArgs(Exception e)
        {
            _exception = e;
        }

        /// <summary>
        /// Exception thrown in a module
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
        }
    }
}
