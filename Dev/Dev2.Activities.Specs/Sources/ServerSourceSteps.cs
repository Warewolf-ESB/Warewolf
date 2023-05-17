using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using Dev2.Activities.Specs.Composition;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Controller;
using Dev2.Network;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.Studio.ViewModels;
using Warewolf.Tools.Specs.BaseTypes;
using Dev2.Infrastructure.Tests;
using Moq;
using Warewolf.UnitTestAttributes;

namespace Dev2.Activities.Specs.Sources
{
    [Binding]
    public sealed class ServerSourceSteps : RecordSetBases, IDisposable
    {
        Depends declaredDependency;
        IServer environmentModel;
        public ServerSourceSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            AppUsageStats.LocalHost = "http://localhost:3142";
            environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
        }

        private static void IsServerOnline(string server, string port)
        {
            using (var ping = new TcpClient())
            {
                try
                {
                    ping.Connect(server, int.Parse(port));
                }
                catch
                {
                    Assert.Fail(server + " is unavailable");
                }
            }
        }

        [AfterScenario]
        public void Cleanup()
        {
            scenarioContext.TryGetValue("resourceModel", out IResourceModel resourceModel);
            if (resourceModel != null)
            {
                environmentModel.ResourceRepository.DeleteResource(resourceModel);
                environmentModel.ResourceRepository.DeleteResourceFromWorkspace(resourceModel);
            }
            declaredDependency?.Dispose();
        }

        [Given(@"I create a server source as")]
        public void GivenICreateAServerSourceAs(Table table)
        {
            var address = table.Rows[0]["Address"];
            if (address == "http://tst-ci-remote.premier.local:3142")
            {
                declaredDependency = new Depends(Depends.ContainerType.CIRemote, true);
                address = "http://" + declaredDependency.Container.IP + ":" + declaredDependency.Container.Port;
            }
            else if (address == "http://wolfs-den.premier.local:3142")
            {
                declaredDependency = new Depends(Depends.ContainerType.AnonymousWarewolf, true);
                address = "http://" + declaredDependency.Container.IP + ":" + declaredDependency.Container.Port;
            }
            if (!address.Contains("localhost"))
            {
                IsServerOnline(address.Replace("http://", "").Replace(":3142", "").Replace(":3144", "").Replace(":" + declaredDependency.Container.Port, ""), declaredDependency.Container.Port);
            }
            var authenticationType = table.Rows[0]["AuthenticationType"];
            Enum.TryParse(authenticationType, true, out AuthenticationType result);

            IServerSource serverSource = new ServerSource
            {
                Address = address,
                AuthenticationType = result
            };
            scenarioContext.Add("serverSource", serverSource);
        }

        [Given(@"I save as ""(.*)""")]
        public void GivenISaveAs(string p0)
        {
            var buildManageNewServerSourceModel = BuildManageNewServerSourceModel();
            var manageNewServerSourceModel = buildManageNewServerSourceModel.Item1;
            var server = buildManageNewServerSourceModel.Item2;
            var serverSource = scenarioContext.Get<IServerSource>("serverSource");
            serverSource.Name = p0;
            try
            {
                manageNewServerSourceModel.Save(serverSource);
            }
            catch (WarewolfSaveException e)
            {
                scenarioContext.Add("result", e.Message);
                Console.WriteLine(e.StackTrace);
                return;
            }
            var queryManagerProxy = buildManageNewServerSourceModel.Item3;

            var load = queryManagerProxy.Load(true, new Mock<IPopupController>().Object);
            var explorerItem = load.Result;
            var explorerItems = explorerItem.Children.Flatten(item => item.Children ?? new List<IExplorerItem>());
            var firstOrDefault = explorerItems.FirstOrDefault(item => item.DisplayName.Equals(p0, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(firstOrDefault);
            IResourceModel resourceModel = server.ResourceRepository.LoadContextualResourceModel(firstOrDefault.ResourceId);
            scenarioContext.Add("resourceModel", resourceModel);
            server.ResourceRepository.ReLoadResources();
        }

        [When(@"I Test ""(.*)""")]
        public void WhenITest(string p0)
        {
            var manageNewServerSourceModel = BuildManageNewServerSourceModel().Item1;
            var resourceModel = scenarioContext.Get<IResourceModel>("resourceModel");
            var resourceModelWorkflowXaml = resourceModel.WorkflowXaml;
            Console.WriteLine(resourceModelWorkflowXaml);
            var source = manageNewServerSourceModel.FetchSource(resourceModel.ID);
            manageNewServerSourceModel.TestConnection(source);
            scenarioContext.Add("result", "success");
        }

        [When(@"I Test the connection")]
        public void WhenITestTheConnection()
        {
            try
            {
                var serverSource = scenarioContext.Get<IServerSource>("serverSource");
                var manageNewServerSourceModel = BuildManageNewServerSourceModel().Item1;
                manageNewServerSourceModel.TestConnection(serverSource);
                scenarioContext.Add("result", "success");
            }
            catch (Exception ex)
            {
                scenarioContext.Add("result", ex.Message);
            }
        }

        Tuple<ManageNewServerSourceModel, IServer, QueryManagerProxy> BuildManageNewServerSourceModel()
        {
            ICommunicationControllerFactory factory = new CommunicationControllerFactory();

            var instanceSource = ServerRepository.Instance.Source;
            var environmentConnection = instanceSource.Connection;
            var studioResourceUpdateManager = new StudioResourceUpdateManager(factory, environmentConnection);
            var queryManagerProxy = new QueryManagerProxy(factory, environmentConnection);
            var manageNewServerSourceModel = new ManageNewServerSourceModel(studioResourceUpdateManager, queryManagerProxy, Environment.MachineName);
            
            var tuple = Tuple.Create(manageNewServerSourceModel, instanceSource, queryManagerProxy);
            return tuple;
        }

        [Then(@"I delete serversource")]
        public void ThenIDeleteServersource()
        {
            var resourceModel = scenarioContext.Get<IResourceModel>("resourceModel");
            var server = BuildManageNewServerSourceModel().Item2;
            server.ResourceRepository.DeleteResource(resourceModel);
        }

        [Then(@"The result is ""(.*)""")]
        public void ThenTheResultIs(string p0)
        {
            var result = scenarioContext.Get<string>("result");
            Assert.AreEqual(p0, result);
        }

        [Given(@"User as ""(.*)"" and with ""(.*)"" as password")]
        public void GivenUserAsWithPassword(string username, string password) => AddUserToServerSource(username, password);

        void AddUserToServerSource(string username, string password)
        {
            var serverSource = scenarioContext.Get<IServerSource>("serverSource");
            serverSource.UserName = username;
            serverSource.Password = password;
            scenarioContext.Set(serverSource, "serverSource");
        }

        protected override void BuildDataList() => throw new NotImplementedException();

        [BeforeFeature("ServerSourceTests")]
        public static void StartRemoteContainer() => WorkflowExecutionSteps._containerOps = new Depends(Depends.ContainerType.Warewolf, true);

        public void Dispose()
        {
            
        }
    }
}
