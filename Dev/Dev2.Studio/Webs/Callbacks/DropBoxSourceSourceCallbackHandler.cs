using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Interfaces;
using Dev2.Utils;

namespace Dev2.Webs.Callbacks
{
    public class DropBoxSourceSourceCallbackHandler : SourceCallbackHandler
    {
        readonly string _token;
        readonly string _secret;
        IServer _server;

        public DropBoxSourceSourceCallbackHandler(IEnvironmentRepository environmentRepository, string token, string secret, IServer server)
            : base(environmentRepository)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>{{"environmentRepository",environmentRepository},{"token",token},{"secret",secret}});
            _token = token;
            _secret = secret;
            _server = server;
        }

        public string Token
        {
            get
            {
                return _token;
            }
        }
        public string Secret
        {
            get
            {
                return _secret;
            }
        }

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            // ReSharper disable once MaximumChainedReferences
            string resName = jsonObj.resourceName;
            string resCat = HelperUtils.SanitizePath((string)jsonObj.resourcePath, resName);
            var oauthSource = new OauthSource { Key = Token, Secret = Secret, ResourceName = resName, ResourcePath = resCat, IsNewResource = true, ResourceID = Guid.NewGuid() };
            var dropBoxSource = oauthSource.ToStringBuilder();
            environmentModel.ResourceRepository.SaveResource(environmentModel,dropBoxSource , GlobalConstants.ServerWorkspaceID);
            _server.UpdateRepository.FireItemSaved();
        }


        protected virtual void StartUriProcess(string uri)
        {
            Process.Start(uri);
        }
    }
}