using System;
using Dev2.Common;

namespace Dev2.Runtime.Diagnostics
{
    public abstract class ExceptionManager
    {
        public bool HasErrors { get; private set; }
        public string Error { get; private set; }

        protected ExceptionManager()
        {
            ClearError();
        }

        protected void ClearError()
        {
            HasErrors = false;
            Error = string.Empty;
        }

        protected void RaiseError(Exception ex)
        {
            RaiseError(ex.Message);
            Dev2Logger.Log.Info(ex.Message + " Stacktrace : " + ex.Message);
        }

        protected void RaiseError(string error)
        {
            HasErrors = true;
            Error = error;

            Dev2Logger.Log.Info(error);
        }
    }
}
