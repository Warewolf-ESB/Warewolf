
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

namespace HttpFramework
{
    /// <summary>
    /// We dont want to let the server to die due to exceptions thrown in worker threads.
    /// therefore we use this delegate to give you a change to handle uncaught exceptions.
    /// </summary>
    /// <param name="source">Class that the exception was thrown in.</param>
    /// <param name="exception">Exception</param>
    /// <remarks>
    /// Server will throw a InternalServerException in release version if you dont
    /// handle this delegate.
    /// </remarks>
    public delegate void ExceptionHandler(object source, Exception exception);
}
