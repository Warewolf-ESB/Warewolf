using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using Dev2.Activities.Specs.Composition;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Explorer;
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
using Warewolf.UnitTestAttributes;

namespace Dev2.Activities.Specs.Sources
{
    [Binding]
    public sealed class ServerSourceSteps : RecordSetBases
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

        string GetIPAddress(string server)
        {
            var serverName = server.Replace("http://", "").Replace(":3142", "").Replace(":3144", "").Replace(":" + declaredDependency.Container.Port, "");
            var ipHostInfo = Dns.GetHostEntry(serverName);
            var ipAddress = ipHostInfo.AddressList[0];
            return ipAddress.ToString();
        }

        private static void IsServerOnline(string server)
        {
            PingReply pingReply;
            using (var ping = new Ping())
            {
                pingReply = ping.Send(server);
            }
            if (pingReply.Status != IPStatus.Success)
            {
                Assert.Fail(server + " is unavailable");
            }
        }

        [AfterScenario]
        public void Cleanup()
        {
            ScenarioContext.Current.TryGetValue("resourceModel", out IResourceModel resourceModel);
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
                declaredDependency = new Depends(Depends.ContainerType.CIRemote);
                address = "http://" + declaredDependency.Container.IP + ":" + declaredDependency.Container.Port;
            }
            else if (address == "http://wolfs-den.premier.local:3142")
            {
                declaredDependency = new Depends(Depends.ContainerType.AnonymousWarewolf);
                address = "http://" + declaredDependency.Container.IP + ":" + declaredDependency.Container.Port;
            }
            if (!address.Contains("localhost"))
            {
                var ipAddress = GetIPAddress(address);
                IsServerOnline(ipAddress);
            }
            var authenticationType = table.Rows[0]["AuthenticationType"];
            Enum.TryParse(authenticationType, true, out AuthenticationType result);

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
                Console.WriteLine(e.StackTrace);
                return;
            }
            var queryManagerProxy = buildManageNewServerSourceModel.Item3;

            var load = queryManagerProxy.Load(true);
            var explorerItem = load.Result;
            var explorerItems = explorerItem.Children.Flatten(item => item.Children ?? new List<IExplorerItem>());
            var firstOrDefault = explorerItems.FirstOrDefault(item => item.DisplayName.Equals(p0, StringComparison.InvariantCultureIgnoreCase));
            IResourceModel resourceModel = server.ResourceRepository.LoadContextualResourceModel(firstOrDefault.ResourceId);
            ScenarioContext.Current.Add("resourceModel", resourceModel);
            server.ResourceRepository.ReLoadResources();
        }

        [When(@"I Test ""(.*)""")]
        public void WhenITest(string p0)
        {
            var manageNewServerSourceModel = BuildManageNewServerSourceModel().Item1;
            var resourceModel = ScenarioContext.Current.Get<IResourceModel>("resourceModel");
            var resourceModelWorkflowXaml = resourceModel.WorkflowXaml;
            Console.WriteLine(resourceModelWorkflowXaml);
            var source = manageNewServerSourceModel.FetchSource(resourceModel.ID);
            manageNewServerSourceModel.TestConnection(source);
            ScenarioContext.Current.Add("result", "success");
        }

        [When(@"I Test the connection")]
        public void WhenITestTheConnection()
        {
            try
            {
                var serverSource = ScenarioContext.Current.Get<IServerSource>("serverSource");
                var manageNewServerSourceModel = BuildManageNewServerSourceModel().Item1;
                manageNewServerSourceModel.TestConnection(serverSource);
                ScenarioContext.Current.Add("result", "success");
            }
            catch (Exception ex)
            {
                ScenarioContext.Current.Add("result", ex.Message);
            }
        }

        static Tuple<ManageNewServerSourceModel, IServer, QueryManagerProxy> BuildManageNewServerSourceModel()
        {
            ICommunicationControllerFactory factory = new CommunicationControllerFactory();

            var instanceSource = ServerRepository.Instance.Source;
            var serverSource = ScenarioContext.Current.Get<IServerSource>("serverSource");
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
            var resourceModel = ScenarioContext.Current.Get<IResourceModel>("resourceModel");
            var server = BuildManageNewServerSourceModel().Item2;
            server.ResourceRepository.DeleteResource(resourceModel);
        }

        [Then(@"The result is ""(.*)""")]
        public void ThenTheResultIs(string p0)
        {
            var result = ScenarioContext.Current.Get<string>("result");
            Assert.AreEqual(p0, result);
        }

        [Given(@"User as ""(.*)""")]
        public void GivenUserAs(string username) => AddUserToServerSource(username, TestEnvironmentVariables.GetVar(username));

        [Given(@"User as ""(.*)"" and with ""(.*)"" as password")]
        public void GivenUserAsWithPassword(string username, string password) => AddUserToServerSource(username, password);

        private static void AddUserToServerSource(string username, string password)
        {
            var serverSource = ScenarioContext.Current.Get<IServerSource>("serverSource");
            serverSource.UserName = username;
            serverSource.Password = password;
            ScenarioContext.Current.Set(serverSource, "serverSource");
        }

        protected override void BuildDataList() => throw new NotImplementedException();

        [BeforeFeature("ServerSourceTests")]
        public static void StartRemoteContainer() => WorkflowExecutionSteps._containerOps = new Depends(Depends.ContainerType.Warewolf);
    }
}
