
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Diagnostics;
using Dev2.Common;
using Dev2.Data.ServiceModel;
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
            _token = token;
            _secret = secret;
        }

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            // ReSharper disable once MaximumChainedReferences
            var dropBoxSource = new DropBoxSource { Key = _token, Secret = _secret, ResourceName = jsonObj.resourceName, ResourcePath = jsonObj.resourcePath == "root" ? "" : jsonObj.resourcePath, IsNewResource = true, ResourceID = Guid.NewGuid() }.ToStringBuilder();
            environmentModel.ResourceRepository.SaveResource(environmentModel,dropBoxSource , GlobalConstants.ServerWorkspaceID);
        }

        protected virtual void StartUriProcess(string uri)
        {
            Process.Start(uri);
        }
    }

}
