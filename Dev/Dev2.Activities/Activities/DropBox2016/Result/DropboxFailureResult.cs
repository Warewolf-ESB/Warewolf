using System;
using Dev2.Common.Interfaces;


namespace Dev2.Activities.DropBox2016.Result
{
    public class DropboxFailureResult : IDropboxResult
    {
        private readonly Exception _exception;

        public DropboxFailureResult(Exception exception)
        {
            _exception = exception;
        }
        public virtual Exception GetException()
        {
            return _exception;
        }
    }
}