using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Webs.Callbacks
{
    public class DropBoxSourceSourceCallbackHandler : SourceCallbackHandler
    {
        readonly string _token;
        readonly string _secret;


        public DropBoxSourceSourceCallbackHandler()
            : this(EnvironmentRepository.Instance,"","")
        {
        }

        public DropBoxSourceSourceCallbackHandler(IEnvironmentRepository environmentRepository, string token, string secret)
            : base(environmentRepository)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>{{"environmentRepository",environmentRepository},{"token",token},{"secret",secret}});
            _token = token;
            _secret = secret;
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
            var dropBoxSource = new OauthSource { Key = Token, Secret = Secret, ResourceName = jsonObj.resourceName, ResourcePath = jsonObj.resourcePath == "root" ? "" : jsonObj.resourcePath, IsNewResource = true, ResourceID = Guid.NewGuid() }.ToStringBuilder();
            environmentModel.ResourceRepository.SaveResource(environmentModel,dropBoxSource , GlobalConstants.ServerWorkspaceID);
        }

        protected virtual void StartUriProcess(string uri)
        {
            Process.Start(uri);
        }
    }
}