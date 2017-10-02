using Dev2.Activities.Specs.BaseTypes;
using Dev2.Studio.Core;
using Dev2.Util;
using System;
using System.Collections.Generic;
using Dev2.Common.ExtMethods;
using Dev2.Data.ServiceModel;
using Dev2.Network;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using System.Linq;

namespace Dev2.Activities.Specs.Deploy
{
    [Binding]
    public sealed class DeployFeatureSteps
    {
        private static ScenarioContext _scenarioContext;
        private readonly CommonSteps _commonSteps;
        private Guid _resourceId = Guid.Parse("fbc83b75-194a-4b10-b50c-b548dd20b408");

        public DeployFeatureSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException("scenarioContext");
            _commonSteps = new CommonSteps(_scenarioContext);
        }

        [BeforeScenario("Deploy")]
        public void RollBack()
        {
            var formattableString = $"http://tst-ci-remote:3142";
            AppSettings.LocalHost = $"http://{Environment.MachineName}:3142";
            IServer remoteServer = new Server(new Guid(), new ServerProxy(new Uri(formattableString)))
            {
                Name = "tst-ci-remote"
            };
            ScenarioContext.Current.Add("destinationServer", remoteServer);
            var previousVersions = remoteServer.ProxyLayer.GetVersions(_resourceId);
            remoteServer.ProxyLayer.Rollback(_resourceId, previousVersions.First().VersionNumber);
        }

        [Given(@"I am Connected to remote server ""(.*)""")]
        public void GivenIAmConnectedToServer(string connectinName)
        {
            var localhost = ServerRepository.Instance.Source;
            ScenarioContext.Current.Add("sourceServer", localhost);
            localhost.Connect();
            var remoteServer = ScenarioContext.Current.Get<IServer>("destinationServer");
            remoteServer.Connect();
        }

        [Then(@"And the destination resource is ""(.*)""")]
        public void ThenAndTheLocalhostResourceIs(string p0)
        {
            var remoteServer = ScenarioContext.Current.Get<IServer>("destinationServer");
            var loadContextualResourceModel = remoteServer.ResourceRepository.LoadContextualResourceModel(_resourceId);
            Assert.AreEqual(p0, loadContextualResourceModel.DisplayName, "Expected Resource to be " + p0 + " on load for ci-remote");
        }

        [Given(@"I select resource ""(.*)"" from source server")]
        public void GivenISelectResourceFromSourceServer(string p0)
        {
            var loaclHost = ScenarioContext.Current.Get<IServer>("sourceServer");
            IContextualResourceModel loadContextualResourceModel = loaclHost.ResourceRepository.LoadContextualResourceModel(_resourceId);
            Assert.IsNotNull(loadContextualResourceModel, p0 + "does not exist on the local machine " + Environment.MachineName);
            ScenarioContext.Current.Add("localResource", loadContextualResourceModel);
        }

        [Given(@"And the localhost resource is ""(.*)""")]
        public void GivenAndTheLocalhostResourceIs(string p0)
        {
            var loaclHost = ScenarioContext.Current.Get<IServer>("sourceServer");
            var loadContextualResourceModel = loaclHost.ResourceRepository.LoadContextualResourceModel(_resourceId);
            Assert.AreEqual(p0, loadContextualResourceModel.DisplayName, "Expected Resource to be " + p0 + " on load for localhost");
            Assert.AreEqual(p0, loadContextualResourceModel.ResourceName, "Expected Resource to be " + p0 + " on load for localhost");
        }

        [When(@"I Deploy resource to remote")]
        public void WhenIDeployResourceToRemote()
        {
            var localhost = ScenarioContext.Current.Get<IServer>("sourceServer");
            var remoteServer = ScenarioContext.Current.Get<IServer>("destinationServer");
            var destConnection = new Connection
            {
                Address = remoteServer.Connection.AppServerUri.ToString(),
                AuthenticationType = remoteServer.Connection.AuthenticationType,
                UserName = remoteServer.Connection.UserName,
                Password = remoteServer.Connection.Password
            };
            localhost.UpdateRepository.Deploy(new List<Guid>() { _resourceId }, false, destConnection);
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


        [Then(@"the destination resource is ""(.*)""")]
        [Given(@"the destination resource is ""(.*)""")]
        [When(@"the destination resource is ""(.*)""")]
        public void ThenDestinationResourceIs(string p0)
        {
            var destinationServer = ScenarioContext.Current.Get<IServer>("destinationServer");
            var loadContextualResourceModel = destinationServer.ResourceRepository.LoadContextualResourceModel(_resourceId);
            Assert.AreEqual(p0, loadContextualResourceModel.DisplayName, "Failed to Update DisplayName after deploy");
            Assert.AreEqual(p0, loadContextualResourceModel.ResourceName, "Failed to Update ResourceName after deploy");
        }
    }
}
