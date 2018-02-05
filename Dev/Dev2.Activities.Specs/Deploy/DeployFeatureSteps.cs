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
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Net.Http;

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
            var tempTarFilePath = @"c:\gzip-server.tar.gz";
            if (!File.Exists(tempTarFilePath)) CreateTarGZ(tempTarFilePath, arg);
            Upload("http://localhost:2375/build", File.ReadAllBytes(tempTarFilePath));
        }

        private void CreateTarGZ(string tgzFilename, string sourceDirectory)
        {
            Stream outStream = File.Create(tgzFilename);
            Stream gzoStream = new GZipOutputStream(outStream);
            TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);

            // Note that the RootPath is currently case sensitive and must be forward slashes e.g. "c:/temp"
            // and must not end with a slash, otherwise cuts off first char of filename
            // This is scheduled for fix in next release
            tarArchive.RootPath = sourceDirectory.Replace('\\', '/');
            if (tarArchive.RootPath.EndsWith("/"))
                tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

            AddDirectoryFilesToTar(tarArchive, sourceDirectory, true);

            tarArchive.Close();
        }

        private Stream Upload(string url, byte[] paramFileBytes)
        {
            HttpContent bytesContent = new ByteArrayContent(paramFileBytes);
            bytesContent.Headers.Remove("Content-Type");
            bytesContent.Headers.Add("Content-Type", "application/x-tar");
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(1,0,0);
                var response = client.PostAsync(url, bytesContent).Result;
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                return response.Content.ReadAsStreamAsync().Result;
            }
        }

        private void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
        {
            //
            // Optionally, write an entry for the directory itself.
            // Specify false for recursion here if we will add the directory's files individually.
            //
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarArchive.WriteEntry(tarEntry, false);
            //
            // Write each file to the tar.
            //
            string[] filenames = Directory.GetFiles(sourceDirectory);
            foreach (string filename in filenames)
            {
                tarEntry = TarEntry.CreateEntryFromFile(filename);
                tarArchive.WriteEntry(tarEntry, true);
            }

            if (recurse)
            {
                string[] directories = Directory.GetDirectories(sourceDirectory);
                foreach (string directory in directories)
                    AddDirectoryFilesToTar(tarArchive, directory, recurse);
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
