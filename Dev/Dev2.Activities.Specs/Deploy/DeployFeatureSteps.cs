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
using System.IO;
using tar_cs;
using System.Net;

namespace Dev2.Activities.Specs.Deploy
{
    [Binding]
    public sealed class DeployFeatureSteps
    {
        static ScenarioContext _scenarioContext;
        readonly CommonSteps _commonSteps;
        Guid _resourceId = Guid.Parse("fbc83b75-194a-4b10-b50c-b548dd20b408");

        public DeployFeatureSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException("scenarioContext");
            _commonSteps = new CommonSteps(_scenarioContext);
        }

        [Given(@"localhost and destination server ""(.*)"" are connected")]
        public void ConnectServers(string destinationServer)
        {
            var formattableString = $"http://{destinationServer}:3142";
            AppUsageStats.LocalHost = $"http://{Environment.MachineName}:3142";
            IServer remoteServer = new Server(new Guid(), new ServerProxy(formattableString, "WarewolfUser", "Dev2@dmin123"))
            {
                Name = destinationServer
            };
            ScenarioContext.Current.Add("destinationServer", remoteServer);
            ConnectToRemoteServerContainer();
            remoteServer.Connect();
            var previousVersions = remoteServer.ProxyLayer.GetVersions(_resourceId);
            if (previousVersions != null && previousVersions.Count > 0)
            {
                remoteServer.ProxyLayer.Rollback(_resourceId, previousVersions.First().VersionNumber);
            }
            var localhost = ServerRepository.Instance.Source;
            ScenarioContext.Current.Add("sourceServer", localhost);
            localhost.Connect();
        }

        private void ConnectToRemoteServerContainer()
        {
            var arg = @"E:\Repos\SalamiArmyWarewolf\Dev\Dev2.Server\bin\Debug";
            using (var archUsTar = WebRequest.Create("http://localhost:2375/build").GetRequestStream())
            using (var tar = new TarWriter(archUsTar))
            {
                tar.WriteDirectoryEntry(arg);
            }
        }

        [Then(@"And the destination resource is ""(.*)""")]
        public void ThenAndTheLocalhostResourceIs(string p0)
        {
            var remoteServer = ScenarioContext.Current.Get<IServer>("destinationServer");
            var loadContextualResourceModel = remoteServer.ResourceRepository.LoadContextualResourceModel(_resourceId);
            Assert.AreEqual(p0, loadContextualResourceModel.DisplayName, "Expected Resource to be " + p0 + " on load for ci-remote");
        }

        [Given(@"And the localhost resource is ""(.*)""")]
        public void GivenAndTheLocalhostResourceIs(string p0)
        {
            var loaclHost = ScenarioContext.Current.Get<IServer>("sourceServer");
            var loadContextualResourceModel = loaclHost.ResourceRepository.LoadContextualResourceModel(_resourceId);
            Assert.AreEqual(p0, loadContextualResourceModel.DisplayName, "Expected Resource to be " + p0 + " on load for localhost");
            Assert.AreEqual(p0, loadContextualResourceModel.ResourceName, "Expected Resource to be " + p0 + " on load for localhost");
        }

        [Given(@"I reload the destination resources")]
        [When(@"I reload the destination resources")]
        [Then(@"I reload the destination resources")]
        public void WhenIReloadTheRemoteServerResources()
        {
            var remoteServer = ScenarioContext.Current.Get<IServer>("destinationServer");
            var loadContextualResourceModel = remoteServer.ResourceRepository.LoadContextualResourceModel(_resourceId);
            ScenarioContext.Current["serverResource"] = loadContextualResourceModel;
        }

        [Given(@"I reload the source resources")]
        [When(@"I reload the source resources")]
        [Then(@"I reload the source resources")]
        public void WhenIReloadTheSourceResources()
        {
            var localhost = ScenarioContext.Current.Get<IServer>("sourceServer");
            localhost.ResourceRepository.ForceLoad();
        }

        [Then(@"the destination resource is ""(.*)""")]
        [Given(@"the destination resource is ""(.*)""")]
        [When(@"the destination resource is ""(.*)""")]
        public void ThenDestinationResourceIs(string p0)
        {
            var destinationServer = ScenarioContext.Current.Get<IServer>("destinationServer");
            var loadContextualResourceModel = destinationServer.ResourceRepository.LoadContextualResourceModel(_resourceId);
            Assert.AreEqual(p0, loadContextualResourceModel.DisplayName, "Failed to Update " + loadContextualResourceModel.DisplayName + " after deploy");
            Assert.AreEqual(p0, loadContextualResourceModel.ResourceName, "Failed to Update " + loadContextualResourceModel.ResourceName + " after deploy");
        }

        [Given(@"I RollBack Resource")]
        [When(@"I RollBack Resource")]
        [Then(@"I RollBack Resource")]
        public void RollBackResource()
        {
            var destinationServer = ScenarioContext.Current.Get<IServer>("destinationServer");
            var previousVersions = destinationServer.ProxyLayer.GetVersions(_resourceId);
            destinationServer.ProxyLayer.Rollback(_resourceId, previousVersions.First().VersionNumber);
        }

        [Then(@"Remote server has updated name")]
        public void ThenLocalServerHasUpdatedName()
        {
            _scenarioContext.TryGetValue("resourceId", out Guid resourceId);
            _scenarioContext.TryGetValue("parentWorkflowName", out string originalName);

            var destinationServer = ScenarioContext.Current.Get<IServer>("destinationServer");
            var localResource = destinationServer.ResourceRepository.LoadContextualResourceModel(resourceId);

            Assert.IsNotNull(localResource, originalName + " failed to deploy.");

            Assert.AreEqual(originalName, localResource.DisplayName, "Failed to Update " + localResource.DisplayName + " after deploy");
            Assert.AreEqual(originalName, localResource.ResourceName, "Failed to Update " + localResource.ResourceName + " after deploy");
        }
        
        [When(@"I select and deploy resource from source server")]
        public void GivenISelectResourceFromSourceServer()
        {
            _scenarioContext.TryGetValue("resourceId", out Guid resourceId);
            var localhost = ScenarioContext.Current.Get<IServer>("sourceServer");
            var remoteServer = ScenarioContext.Current.Get<IServer>("destinationServer");
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
                        Assert.Fail("Error returned from deploy operation. " + result.Message);
                    }
                }
            }
        }
    }
}
