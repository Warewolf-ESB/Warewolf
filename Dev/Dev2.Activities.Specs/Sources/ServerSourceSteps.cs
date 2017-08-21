using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Controller;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.Studio.ViewModels;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Sources
{
    [Binding]
    public sealed class ServerSourceSteps : RecordSetBases
    {

        public ServerSourceSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            AppSettings.LocalHost = "http://localhost:3142";
            IServer environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
        }
        [Given(@"I create a server source as")]
        public void GivenICreateAServerSourceAs(Table table)
        {

            var address = table.Rows[0]["Address"];
            var authenticationType = table.Rows[0]["AuthenticationType"];
            AuthenticationType result;
            Enum.TryParse(authenticationType, true, out result);

            IServerSource serverSource = new ServerSource()
            {
                Address = address,
                AuthenticationType = result
            };
            ScenarioContext.Current.Add("serverSource", serverSource);

        }

        [Given(@"I save as ""(.*)""")]
        public void GivenISaveAs(string p0)
        {
            var buildManageNewServerSourceModel = BuildManageNewServerSourceModel();
            var manageNewServerSourceModel = buildManageNewServerSourceModel.Item1;
            var server = buildManageNewServerSourceModel.Item2;
            var serverSource = ScenarioContext.Current.Get<IServerSource>("serverSource");
            serverSource.Name = p0;

            try
            {
                manageNewServerSourceModel.Save(serverSource);

            }
            catch (WarewolfSaveException e)
            {
                ScenarioContext.Current.Add("result", e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                return;
            }
            var queryManagerProxy = buildManageNewServerSourceModel.Item3;

            var load = queryManagerProxy.Load(true);
            var explorerItem = load.Result;
            var explorerItems = explorerItem.Children.Flatten(item => item.Children ?? new List<IExplorerItem>());
            var firstOrDefault = explorerItems.FirstOrDefault(item => item.DisplayName.Equals(p0, StringComparison.InvariantCultureIgnoreCase));
            IResourceModel resourceModel = server.ResourceRepository.LoadContextualResourceModel(firstOrDefault.ResourceId);
            ScenarioContext.Current.Add("resourceModel", resourceModel);
        }

        [When(@"I Test ""(.*)""")]
        public void WhenITest(string p0)
        {
            var manageNewServerSourceModel = BuildManageNewServerSourceModel().Item1;
            try
            {
                var resourceModel = ScenarioContext.Current.Get<IResourceModel>("resourceModel");
                var resourceModelWorkflowXaml = resourceModel.WorkflowXaml;
                System.Diagnostics.Debug.WriteLine(resourceModelWorkflowXaml);

                var source = manageNewServerSourceModel.FetchSource(resourceModel.ID);
                manageNewServerSourceModel.TestConnection(source);
                ScenarioContext.Current.Add("result", "success");
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
            }
        }

        private static Tuple<ManageNewServerSourceModel, IServer, QueryManagerProxy> BuildManageNewServerSourceModel()
        {
            ICommunicationControllerFactory factory = new CommunicationControllerFactory();

            IServer instanceSource = ServerRepository.Instance.Source;
            var environmentConnection = instanceSource.Connection;
            var studioResourceUpdateManager = new StudioResourceUpdateManager(factory, environmentConnection);
            QueryManagerProxy queryManagerProxy = new QueryManagerProxy(factory, environmentConnection);
            ManageNewServerSourceModel manageNewServerSourceModel = new ManageNewServerSourceModel(studioResourceUpdateManager, queryManagerProxy, Environment.MachineName);
            var tuple = Tuple.Create(manageNewServerSourceModel, instanceSource, queryManagerProxy);
            return tuple;
        }


        [Then(@"I delete serversource")]
        public void ThenIDeleteServersource()
        {
            var resourceModel = ScenarioContext.Current.Get<IResourceModel>("resourceModel");
            var server = BuildManageNewServerSourceModel().Item2;
            server.ResourceRepository.DeleteResource(resourceModel);
        }



        [Then(@"The result is ""(.*)""")]
        public void ThenTheResultIs(string p0)
        {
            var result = ScenarioContext.Current.Get<string>("result");
            Assert.AreEqual(result, p0);
        }

        [Given(@"User details as")]
        public void GivenUserDetailsAs(Table table)
        {
            var username = table.Rows[0]["username"];
            var password = table.Rows[0]["Password"];
            var serverSource = ScenarioContext.Current.Get<IServerSource>("serverSource");
            serverSource.UserName = username;
            serverSource.Password = password;
        }


        protected override void BuildDataList()
        {
            throw new NotImplementedException();
        }
    }
}
