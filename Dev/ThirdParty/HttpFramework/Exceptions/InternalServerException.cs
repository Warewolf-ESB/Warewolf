
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
    /// The server encountered an unexpected condition which prevented it from fulfilling the request.
    /// </summary>
    public class InternalServerException : HttpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternalServerException"/> class.
        /// </summary>
        public InternalServerException()
            : base(HttpStatusCode.InternalServerError, "The server encountered an unexpected condition which prevented it from fulfilling the request.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalServerException"/> class.
        /// </summary>
        /// <param name="message">error message.</param>
        public InternalServerException(string message)
            : base(HttpStatusCode.InternalServerError, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalServerException"/> class.
        /// </summary>
        /// <param name="message">error message.</param>
        /// <param name="inner">inner exception.</param>
        public InternalServerException(string message, Exception inner)
            : base(HttpStatusCode.InternalServerError, message, inner)
        {
        }
    }
}
