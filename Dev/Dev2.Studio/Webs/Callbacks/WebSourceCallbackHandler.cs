using System;
using System.Diagnostics;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;


namespace Dev2.Webs.Callbacks
{
    public class WebSourceCallbackHandler : SourceCallbackHandler
    {
        public readonly static string[] ValidSchemes = { "http", "https", "ftp" };

        public WebSourceCallbackHandler()
            : this(EnvironmentRepository.Instance)
        {
        }

        public WebSourceCallbackHandler(IEnvironmentRepository environmentRepository)
            : base(environmentRepository)
        {
        }

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            ReloadResource(environmentModel, Guid.Parse(jsonObj.ResourceID.Value), ResourceType.Source);
        }

        protected virtual void StartUriProcess(string uri)
        {
            Process.Start(uri);
        }
    }
}
