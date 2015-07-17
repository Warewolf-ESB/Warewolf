using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Utils;

namespace Dev2.Webs.Callbacks
{
    public class SharepointServerSourceCallbackHandler : SourceCallbackHandler
    {
        readonly string _server;
        readonly string _userName;
        readonly string _password;
        readonly AuthenticationType _authenticationType;

        public SharepointServerSourceCallbackHandler()
            : this(EnvironmentRepository.Instance,"","","",AuthenticationType.Windows)
        {
        }

        public SharepointServerSourceCallbackHandler(IEnvironmentRepository environmentRepository,string server,string userName,string password,AuthenticationType authenticationType)
            : base(environmentRepository)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>{{"environmentRepository",environmentRepository}});
            _server = server;
            _userName = userName;
            _password = password;
            _authenticationType = authenticationType;
        }

        public string Server
        {
            get
            {
                return _server;
            }
        }
        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            // ReSharper disable once MaximumChainedReferences
            string resName = jsonObj.resourceName;
            string resCat = HelperUtils.SanitizePath((string)jsonObj.resourcePath, resName);
            var sharepointSource = new SharepointSource { Server = Server,UserName = _userName,Password = _password,AuthenticationType = _authenticationType, ResourceName = resName, ResourcePath = resCat, IsNewResource = true, ResourceID = Guid.NewGuid() };
            var source = sharepointSource.ToStringBuilder();

            var messaage = environmentModel.ResourceRepository.SaveResource(environmentModel, source, GlobalConstants.ServerWorkspaceID);
            if(!messaage.HasError)
            {
                environmentModel.ResourceRepository.ReloadResource(sharepointSource.ResourceID, ResourceType.Source, ResourceModelEqualityComparer.Current, true);
            }
        }

        protected virtual void StartUriProcess(string uri)
        {
            Process.Start(uri);
        }
    }
}