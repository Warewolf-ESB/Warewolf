
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
using System.Net;

namespace HttpFramework.Exceptions
{
    /// <summary>
    /// The requested resource was not found in the web server.
    /// </summary>
    public class NotFoundException : HttpException
    {
        /// <summary>
        /// Create a new exception
        /// </summary>
        /// <param name="message">message describing the error</param>
        /// <param name="inner">inner exception</param>
        public NotFoundException(string message, Exception inner) : base(HttpStatusCode.NotFound, message, inner)
        {
        }

        /// <summary>
        /// Create a new exception
        /// </summary>
        /// <param name="message">message describing the error</param>
        public NotFoundException(string message)
            : base(HttpStatusCode.NotFound, message)
        {
        }
    }
}
