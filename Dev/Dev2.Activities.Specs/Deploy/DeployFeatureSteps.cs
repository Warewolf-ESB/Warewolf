using Dev2.Activities.Specs.BaseTypes;
using Dev2.Studio.Core;
using Dev2.Util;
using System;
using System.Collections.Generic;
using Dev2.Common.ExtMethods;
using Dev2.Controller;
using Dev2.Data.ServiceModel;
using Dev2.Network;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Deploy
{
    [Binding]
    public sealed class DeployFeatureSteps
    {
        private static ScenarioContext _scenarioContext;
        private readonly CommonSteps _commonSteps;

        public DeployFeatureSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            _scenarioContext = scenarioContext;
            _commonSteps = new CommonSteps(_scenarioContext);
        }

        [Given(@"I am Connected to source server ""(.*)""")]
        public void GivenIAmConnectedToServer(string connectinName)
        {
            var formattableString = $"http://{connectinName}:3142";
            AppSettings.LocalHost = $"http://{Environment.MachineName}:3142";
            IServer remoteServer = new Server(new Guid(), new ServerProxy(new Uri(formattableString)))
            {
                Name = connectinName
            };
            var localHost = ServerRepository.Instance.Source;
            ScenarioContext.Current.Add("remoteServer", remoteServer);
            ScenarioContext.Current.Add("localHost", localHost);
            localHost.Connect();
            remoteServer.Connect();
        }


        [Given(@"I select resource ""(.*)""")]
        public void GivenISelectResource(string p0)
        {
            var loaclHost = ScenarioContext.Current.Get<IServer>("remoteServer");
            IContextualResourceModel loadContextualResourceModel = loaclHost.ResourceRepository.LoadContextualResourceModel("fbc83b75-194a-4b10-b50c-b548dd20b408".ToGuid());
            Assert.IsNotNull(loadContextualResourceModel, p0 + "does not exist on the local machine " + Environment.MachineName);
            ScenarioContext.Current.Add("localResource", loadContextualResourceModel);
        }

        [Given(@"And the localhost resource is ""(.*)""")]
        public void GivenAndTheLocalhostResourceIs(string p0)
        {
            var loaclHost = ScenarioContext.Current.Get<IServer>("localHost");
            var loadContextualResourceModel = loaclHost.ResourceRepository.LoadContextualResourceModel("fbc83b75-194a-4b10-b50c-b548dd20b408".ToGuid());
            Assert.AreEqual(p0, loadContextualResourceModel.DisplayName);
            Assert.AreEqual(p0, loadContextualResourceModel.ResourceName);
        }

        [Then(@"And the localhost resource is ""(.*)""")]
        public void ThenAndTheLocalhostResourceIs(string p0)
        {
            var loaclHost = ScenarioContext.Current.Get<IServer>("localHost");
            var loadContextualResourceModel = loaclHost.ResourceRepository.LoadContextualResourceModel("fbc83b75-194a-4b10-b50c-b548dd20b408".ToGuid());
            Assert.AreEqual(p0, loadContextualResourceModel.DisplayName);
            Assert.AreEqual(p0, loadContextualResourceModel.ResourceName);
        }


        [When(@"I Deploy resource to localhost")]
        public void WhenIDeployResourceToLocalhost()
        {
            var loaclHost = ScenarioContext.Current.Get<IServer>("localHost");
            var remoteServer = ScenarioContext.Current.Get<IServer>("remoteServer");
            var destConnection = new Connection
            {
                Address = loaclHost.Connection.AppServerUri.ToString(),
                AuthenticationType = loaclHost.Connection.AuthenticationType,
                UserName = loaclHost.Connection.UserName,
                Password = loaclHost.Connection.Password
            };
            remoteServer.UpdateRepository.Deploy(new List<Guid>() { new Guid("fbc83b75-194a-4b10-b50c-b548dd20b408") }, false, destConnection);
        }

        [When(@"I reload the local resource")]
        public void WhenIReloadTheLocalResource()
        {
            var loaclHost = ScenarioContext.Current.Get<IServer>("localHost");
            var loadContextualResourceModel = loaclHost.ResourceRepository.LoadContextualResourceModel("fbc83b75-194a-4b10-b50c-b548dd20b408".ToGuid());
            ScenarioContext.Current["localResource"] = loadContextualResourceModel;
        }

    }
}
