/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Specs.BaseTypes;
using Dev2.Studio.Core;
using Dev2.Util;
using System;
using System.Collections.Generic;
using Dev2.Data.ServiceModel;
using Dev2.Network;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using System.Linq;
using Dev2.Activities.Specs.Composition;
using Warewolf.UnitTestAttributes;

namespace Dev2.Activities.Specs.Deploy
{
    [Binding]
    public sealed class DeployFeatureSteps
    {
        static ScenarioContext _scenarioContext;
        readonly CommonSteps _commonSteps;
        readonly Guid _resourceId = Guid.Parse("fbc83b75-194a-4b10-b50c-b548dd20b408");

        public DeployFeatureSteps(ScenarioContext scenarioContext)
        {
#pragma warning disable S3010 // Static fields should not be updated in constructors
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException("scenarioContext");
#pragma warning restore S3010 // Static fields should not be updated in constructors
            _commonSteps = new CommonSteps(_scenarioContext);
        }

        [Given(@"localhost and destination server are connected")]
        public void ConnectServers()
        {
            WorkflowExecutionSteps._containerOps = new Depends(Depends.ContainerType.Warewolf);
            AppUsageStats.LocalHost = $"http://{Environment.MachineName}:3142";
            ConnectToRemoteServerContainer(new Depends(Depends.ContainerType.CIRemote));
            var localhost = ServerRepository.Instance.Source;
            _scenarioContext.Add("sourceServer", localhost);
            localhost.Connect();
        }

        void ConnectToRemoteServerContainer(Depends dependency)
        {
            string destinationServer = dependency.Container.IP + ":" + dependency.Container.Port;

            var formattableString = $"http://{destinationServer}";
            IServer remoteServer = new Server(new Guid(), new ServerProxy(formattableString, "WarewolfAdmin", "W@rEw0lf@dm1n"))
            {
                Name = destinationServer
            };
            _scenarioContext.Add("destinationServer", remoteServer);
            remoteServer.ConnectAsync().Wait(60000);
        }

        [Then(@"And the destination resource is ""(.*)""")]
        public void ThenAndTheLocalhostResourceIs(string p0)
        {
            var remoteServer = _scenarioContext.Get<IServer>("destinationServer");
            var loadContextualResourceModel = remoteServer.ResourceRepository.LoadContextualResourceModel(_resourceId);
            Assert.AreEqual(p0, loadContextualResourceModel.DisplayName, "Expected Resource to be " + p0 + " on load for ci-remote");
        }

        [Given(@"And the localhost resource is ""(.*)""")]
        public void GivenAndTheLocalhostResourceIs(string p0)
        {
            var loaclHost = _scenarioContext.Get<IServer>("sourceServer");
            var loadContextualResourceModel = loaclHost.ResourceRepository.LoadContextualResourceModel(_resourceId);
            Assert.AreEqual(p0, loadContextualResourceModel.DisplayName, "Expected Resource to be " + p0 + " on load for localhost");
            Assert.AreEqual(p0, loadContextualResourceModel.ResourceName, "Expected Resource to be " + p0 + " on load for localhost");
        }

        [Given(@"I reload the destination resources")]
        [When(@"I reload the destination resources")]
        [Then(@"I reload the destination resources")]
        public void WhenIReloadTheRemoteServerResources()
        {
            var remoteServer = _scenarioContext.Get<IServer>("destinationServer");
            var loadContextualResourceModel = remoteServer.ResourceRepository.LoadContextualResourceModel(_resourceId);
            _scenarioContext["serverResource"] = loadContextualResourceModel;
        }

        [Given(@"I reload the source resources")]
        [When(@"I reload the source resources")]
        [Then(@"I reload the source resources")]
        public void WhenIReloadTheSourceResources()
        {
            var localhost = _scenarioContext.Get<IServer>("sourceServer");
            localhost.ResourceRepository.Load(true);
        }

        [Then(@"the destination resource is ""(.*)""")]
        [Given(@"the destination resource is ""(.*)""")]
        [When(@"the destination resource is ""(.*)""")]
        public void ThenDestinationResourceIs(string p0)
        {
            var destinationServer = _scenarioContext.Get<IServer>("destinationServer");
            var loadContextualResourceModel = destinationServer.ResourceRepository.LoadContextualResourceModel(_resourceId);
            Assert.AreEqual(p0, loadContextualResourceModel.DisplayName, "Failed to Update " + loadContextualResourceModel.DisplayName + " after deploy");
            Assert.AreEqual(p0, loadContextualResourceModel.ResourceName, "Failed to Update " + loadContextualResourceModel.ResourceName + " after deploy");
        }

        [Given(@"I RollBack Resource")]
        [When(@"I RollBack Resource")]
        [Then(@"I RollBack Resource")]
        public void RollBackResource()
        {
            var destinationServer = _scenarioContext.Get<IServer>("destinationServer");
            var previousVersions = destinationServer.ProxyLayer.GetVersions(_resourceId);
            destinationServer.ProxyLayer.Rollback(_resourceId, previousVersions.First().VersionNumber);
        }

        [Then(@"Remote server has updated name")]
        public void ThenLocalServerHasUpdatedName()
        {
            _scenarioContext.TryGetValue("resourceId", out Guid resourceId);
            _scenarioContext.TryGetValue("parentWorkflowName", out string originalName);

            var destinationServer = _scenarioContext.Get<IServer>("destinationServer");
            var localResource = destinationServer.ResourceRepository.LoadContextualResourceModel(resourceId);

            Assert.IsNotNull(localResource, originalName + " failed to deploy.");

            Assert.AreEqual(originalName, localResource.DisplayName, "Failed to Update " + localResource.DisplayName + " after deploy");
            Assert.AreEqual(originalName, localResource.ResourceName, "Failed to Update " + localResource.ResourceName + " after deploy");
        }
        
        [When(@"I deploy the workflow")]
        public void GivenISelectResource()
        {
            _scenarioContext.TryGetValue("resourceId", out Guid resourceId);
            Console.WriteLine("Deploying " + resourceId.ToString());
            var localhost = _scenarioContext.Get<IServer>("sourceServer");
            var remoteServer = _scenarioContext.Get<IServer>("destinationServer");
            var destConnection = new Connection
            {
                Address = remoteServer.Connection.AppServerUri.ToString(),
                AuthenticationType = remoteServer.Connection.AuthenticationType,
                UserName = remoteServer.Connection.UserName,
                Password = remoteServer.Connection.Password
            };
            var DeployResults = localhost.UpdateRepository.Deploy(new List<Guid> { resourceId }, false, destConnection);
            if (DeployResults != null)
            {
                foreach (var result in DeployResults)
                {
                    if (result.HasError)
                    {
                        Assert.Fail("Error returned from deploy operation. " + result.ErrorDetails);
                    }
                }
            }
        }

        [When(@"I rename the workflow to ""(.*)"" and re deploy")]
        public void WhenIRenameToAndReDeploy(string newName)
        {
            _scenarioContext.TryGetValue("resourceId", out Guid resourceId);
            Console.WriteLine("Renaming " + resourceId.ToString());
            _scenarioContext.Add("newName", newName);
            var localhost = _scenarioContext.Get<IServer>("sourceServer");
            localhost.ExplorerRepository.UpdateManagerProxy.Rename(resourceId, newName);
        }
    }
}
