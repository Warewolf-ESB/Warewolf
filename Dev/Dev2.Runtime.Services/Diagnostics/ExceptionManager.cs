using Dev2.Common;
using System;

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
            ServerLogger.LogError(ex.Message + " " + ex.StackTrace);
        }

        protected void RaiseError(string error)
        {
            HasErrors = true;
            Error = error;
            ServerLogger.LogError(error);
        }
    }
}
