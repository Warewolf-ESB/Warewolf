using System;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Exceptions
{
    public class WebResourceRemoveFailedException : Exception
    {

        readonly WebResourceViewModel _failedResource;
        readonly string _errorMessage;

        public WebResourceRemoveFailedException(WebResourceViewModel failedResource, string errorMessage)
        {
            _failedResource = failedResource;
            _errorMessage = errorMessage;
        }

        public override string Message
        {
            get
            {
                return _errorMessage;
            }
        }

        public WebResourceViewModel FailedWebResource
        {
            get
            {
                return _failedResource;
            }
        }

    }
}
