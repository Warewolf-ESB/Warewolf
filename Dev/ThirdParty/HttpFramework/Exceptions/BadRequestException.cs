
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
    /// The request could not be understood by the server due to malformed syntax. 
    /// The client SHOULD NOT repeat the request without modifications.
    /// 
    /// Text taken from: http://www.submissionchamber.com/help-guides/error-codes.php
    /// </summary>
    public class BadRequestException : HttpException
    {
        /// <summary>
        /// Create a new bad request exception.
        /// </summary>
        /// <param name="errMsg">reason to why the request was bad.</param>
        public BadRequestException(string errMsg)
            : base(HttpStatusCode.BadRequest, errMsg)
        {
            
        }

        /// <summary>
        /// Create a new bad request exception.
        /// </summary>
        /// <param name="errMsg">reason to why the request was bad.</param>
        /// <param name="inner">inner exception</param>
        public BadRequestException(string errMsg, Exception inner)
            : base(HttpStatusCode.BadRequest, errMsg, inner)
        {
            
        }
    }
}
