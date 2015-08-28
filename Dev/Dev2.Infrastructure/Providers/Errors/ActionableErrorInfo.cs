
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
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
    public class ActionableErrorInfo : ErrorInfo, IActionableErrorInfo
    {
        [NonSerialized]
        readonly Action _do;

        public ActionableErrorInfo()
        {
        }

        public ActionableErrorInfo(Action action)
        {
            _do = action;
        }

        public ActionableErrorInfo(IErrorInfo input, Action action)
            : this(action)
        {
            Message = input.Message;
            FixData = input.FixData;
            FixType = input.FixType;
            StackTrace = input.StackTrace;
            ErrorType = input.ErrorType;
            InstanceID = input.InstanceID;
        }

        public void Do()
        {
            if(_do != null)
            {
                _do();
            }
        }
    }
}
