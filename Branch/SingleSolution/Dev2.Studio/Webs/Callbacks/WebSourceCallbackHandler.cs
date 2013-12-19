using System;
using System.Diagnostics;
using System.Linq;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Webs.Callbacks
{
    public class WebSourceCallbackHandler : SourceCallbackHandler
    {
        public readonly static string[] ValidSchemes = new[] { "http", "https", "ftp" };

        public WebSourceCallbackHandler()
            : this(EnvironmentRepository.Instance)
        {
        }

        public WebSourceCallbackHandler(IEnvironmentRepository environmentRepository)
            : base(environmentRepository)
        {
        }
//
//        protected override void NavigateTo(string uri, string args, string returnUri)
//        {
//            Uri theUri;
//            if(!string.IsNullOrEmpty(uri) && Uri.TryCreate(uri, UriKind.Absolute, out theUri) && ValidSchemes.Contains(theUri.Scheme.ToLowerInvariant()))
//            {
//                StartUriProcess(uri);
//            }
//        }

        protected virtual void StartUriProcess(string uri)
        {
            Process.Start(uri);
        }
    }
}
