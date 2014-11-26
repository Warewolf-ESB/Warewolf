
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
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;

namespace Dev2.Providers.Errors
{
    public class ErrorInfo : IErrorInfo
    {
        public Guid InstanceID { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }

        public ErrorType ErrorType { get; set; }

        public FixType FixType { get; set; }

        public string FixData { get; set; }

    }
}
