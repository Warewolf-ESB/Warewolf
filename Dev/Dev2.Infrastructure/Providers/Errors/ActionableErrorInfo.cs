using Dev2.Common.Interfaces.Infrastructure;
using System;

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