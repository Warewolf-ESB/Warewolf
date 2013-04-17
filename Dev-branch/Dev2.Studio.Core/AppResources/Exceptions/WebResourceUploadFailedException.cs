using System;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.AppResources.Exceptions {
    public class WebResourceUploadFailedException : Exception {

        readonly IWebResourceViewModel _failedResource;
        readonly string _errorMessage;

        public WebResourceUploadFailedException(IWebResourceViewModel failedResource, string errorMessage) {
            _failedResource = failedResource;
            _errorMessage = errorMessage;
        }

        public override string Message {
            get {
                return _errorMessage;
            }
        }

        public IWebResourceViewModel FailedWebResource {
            get {
                return _failedResource;
            }
        }

    }
}
