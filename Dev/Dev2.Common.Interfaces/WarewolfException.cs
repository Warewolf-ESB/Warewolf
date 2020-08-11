/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Web;
using Warewolf.Exceptions;

namespace Dev2.Common.Interfaces
{
    public class AccessDeniedException : HttpException, IWarewolfException
    {
        public AccessDeniedException(string message)
            : base(message)
        {
        }
    }
    public class WarewolfException : Exception, IWarewolfException
    {
        
        public WarewolfException(string message, Exception innerException,ExceptionType exceptionType, ExceptionSeverity severity) :
            base(message, innerException??new Exception())
            
        {
            Severity = severity;
            ExceptionType = exceptionType;            
        }

        
        public ExceptionType ExceptionType { get; private set; }
        public ExceptionSeverity Severity{get;private set;}
    }

    public enum ExceptionSeverity
    {
        Critical,
        Error,
        User,
        Etc,
        Minor
    }

    public enum ExceptionType
    {
        Connection,
        Execution,
        Unknown

    }
}
