/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Tests.Runtime.Hosting;
using Dev2.Tests.Runtime.XML;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable CheckNamespace
namespace Dev2.DynamicServices.Test
{
    /// <summary>
    /// Summary description for WorkspaceTest
    /// </summary>
    [TestClass]
    public class WorkspaceTest
    {
        const string ServiceName = "Calculate_RecordSet_Subtract";
        readonly Guid _serviceID = Guid.Parse("b2b0cc87-32ba-4504-8046-79edfb18d5fd");

        const enDynamicServiceObjectType ServiceType = enDynamicServiceObjectType.DynamicService;

        static readonly object SyncRoot = new object();
        static readonly object MonitorLock = new object();

        #region TestInitialize/Cleanup

        [TestInitialize]
        public void MyTestInitialize()
        {
            Monitor.Enter(MonitorLock);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            Monitor.Exit(MonitorLock);
        }

        #endregion

        #region Update

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateWithNull()
        {
            var workspaceID = Guid.NewGuid();
            var workspace = new Workspace(workspaceID);
            workspace.Update(null);
        }




        [TestMethod]
        public void CanUpdateWorkItemWithCommitActionLocalSaveOnly()
        {
            //Lock because of access to resourcatalog
            lock (SyncRoot)
            {
                XElement testWorkspaceItemXml = XmlResource.Fetch("WorkspaceItem");

                Guid workspaceID;
                var repositoryInstance = SetupRepo(out workspaceID);

                var workspace = repositoryInstance.Get(workspaceID);

                IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
                Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
                data["ItemXml"] = new StringBuilder(testWorkspaceItemXml.ToString().Replace("WorkspaceID=\"B1890C86-95D8-4612-A7C3-953250ED237A\"", "WorkspaceID=\"" + workspaceID + "\""));
                data["IsLocalSave"] = new StringBuilder("true");

                // Now remove the 
                ResourceCatalog.Instance.DeleteResource(GlobalConstants.ServerWorkspaceID, _serviceID, "WorkflowService");

                endpoint.Execute(data, workspace);

                var res = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, _serviceID);

                Assert.IsNull(res);
            }
        }


        [TestMethod]
        public void CanUpdateWorkspaceItemAndRespectIsLocalOption()
        {
            //Lock because of access to resourcatalog
            lock (SyncRoot)
            {
                var workspaceItem = new Mock<IWorkspaceItem>();
                workspaceItem.Setup(m => m.Action).Returns(WorkspaceItemAction.Commit);
                workspaceItem.Setup(m => m.ServiceName).Returns(ServiceName);
                workspaceItem.Setup(m => m.ID).Returns(_serviceID);
                workspaceItem.Setup(m => m.ServiceType).Returns(ServiceType.ToString);

                Guid workspaceID;
                var repositoryInstance = SetupRepo(out workspaceID);

                var workspace = repositoryInstance.Get(GlobalConstants.ServerWorkspaceID);

                var previous = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, _serviceID);
                var next = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, _serviceID);
                Assert.AreSame(previous, next);
            }
        }

        [TestMethod]
        public void VerifyXmlSpeedTest()
        {

            IHostSecurityProvider theHostProvider = HostSecurityProvider.Instance;

            string xmlToVerify =
                @" <Source Name=""Anything To Xml Hook Plugin"" Type=""Plugin"" AssemblyName=""Dev2.AnytingToXmlHook.Plugin.AnythignToXmlHookPlugin"" AssemblyLocation=""Plugins/Dev2.AnytingToXmlHook.Plugin.dll"" ServerID=""" +
                theHostProvider.ServerID + @""">
      <AuthorRoles>Schema Admins,Enterprise Admins,Domain Admins,Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Fax Administrators,Windows SBS Virtual Private Network Users,All Users,Windows SBS Administrators,Windows SBS SharePoint_OwnersGroup,Windows SBS Link Users,Windows SBS Admin Tools Group,Company Users,Business Design Studio Developers,</AuthorRoles> 
      <Comment>.NET Assembly Used as an entry point to the databrowser map functionality and to emit any given string data.</Comment> 
      <HelpLink>http://d</HelpLink> 
      <Category>Conversion</Category> 
      <Tags /> 
      <UnitTestTargetWorkflowService /> 
      <BizRule /> 
      <WorkflowActivityDef /> 
      <XamlDefinition /> 
      <DisplayName>Source</DisplayName> 
     <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
     <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" /> 
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" /> 
     <Reference URI="""">
     <Transforms>
      <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" /> 
      </Transforms>
      <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" /> 
      <DigestValue>FO3yREne/gQ0q2lIeIs3kfHhx9M=</DigestValue> 
      </Reference>
      </SignedInfo>
      <SignatureValue>cZmlTZkMw2jX7Kgo3sOLYDJO2SmkNQFxdRaLs7d4cgTd8frAb+ZIhSdffp2iKb503Yr5qn2/Ns9JxXFg0Iu/ltIipE7hvtCvVtrX1m3KLRMiflQXnjX8GNLfH5XlBA4SgT72Kj0CUjLrazRiLPGu01yDJEvGuA7PA4YDWvjBFls=</SignatureValue> 
      </Signature>
      </Source>";

            DateTime theTime = DateTime.Now;
            theHostProvider.VerifyXml(new StringBuilder(xmlToVerify));
            TimeSpan duration = DateTime.Now - theTime;
            // was 20 moved it to 200
            Assert.IsTrue(duration.TotalMilliseconds < 200, "Duration: " + duration.TotalMilliseconds + "ms");
        }

        #endregion

        #region Delete

        [TestMethod]
        public void DeleteDecreasesItemCountByOne()
        {
            // this will add           
            Guid workspaceID;
            var repositoryInstance = SetupRepo(out workspaceID);
            var workspace = repositoryInstance.Get(workspaceID);

            var expected = repositoryInstance.Count - 1;
            repositoryInstance.Delete(workspace);
            Assert.AreEqual(expected, repositoryInstance.Count);

        }

        [TestMethod]
        public void DeleteNullItemExpectedNoOperationPerformed()
        {
            Guid workspaceID;
            var repositoryInstance = SetupRepo(out workspaceID);
            var expected = repositoryInstance.Count;
            repositoryInstance.Delete(null);
            Assert.AreEqual(expected, repositoryInstance.Count);
        }

        #endregion Delete

        #region Save

        [TestMethod]
        public void SaveWithNewWorkspaceIncreasesItemCountByOne()
        {
            Guid workspaceID;
            var repositoryInstance = SetupRepo(out workspaceID);
            var expected = repositoryInstance.Count + 1;
            Guid myGuid = Guid.NewGuid();
            var workspace = new Workspace(myGuid);
            repositoryInstance.Save(workspace);
            Assert.AreEqual(expected, repositoryInstance.Count);
        }

        [TestMethod]
        public void SaveWithNullWorkspaceIncreasesItemCountByOne()
        {
            Guid workspaceID;
            var repositoryInstance = SetupRepo(out workspaceID);
            var expected = repositoryInstance.Count;
            repositoryInstance.Save(null);
            Assert.AreEqual(expected, repositoryInstance.Count);
        }

        #endregion Save

        #region CTOR Tests

        // PBI 9363 - 2013.05.29 - TWR: Added 
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkspaceRepositoryWithNullResourceCatalogExpectedThrowsArgumentNullException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new WorkspaceRepository(null);
        }

        // PBI 9363 - 2013.05.29 - TWR: Added 
        [TestMethod]
        public void WorkspaceRepositoryWithResourceCatalogExpectedDoesNotLoadResources()
        {
            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(c => c.LoadWorkspace(It.IsAny<Guid>())).Verifiable();

            // ReSharper disable once ObjectCreationAsStatement
            new WorkspaceRepository(catalog.Object);

            catalog.Verify(c => c.LoadWorkspace(It.IsAny<Guid>()), Times.Never());
        }


        [TestMethod]
        public void ServerWorkspaceCreatedAfterInstantiation()
        {
            Guid workspaceID;
            var repositoryInstance = SetupRepo(out workspaceID);
            Assert.IsNotNull(repositoryInstance.ServerWorkspace);
        }

        [TestMethod]
        public void ServerWorkspaceCreatedWithServerWorkspaceID()
        {
            Guid workspaceID;
            var repositoryInstance = SetupRepo(out workspaceID);
            Assert.AreEqual(WorkspaceRepository.ServerWorkspaceID, repositoryInstance.ServerWorkspace.ID);
        }

        #endregion CTOR Tests

        #region Get Tests

        [TestMethod]
        public void GetWithEmptyGuidReturnsServerWorkspace()
        {
            Guid workspaceID;
            var repositoryInstance = SetupRepo(out workspaceID);
            var result = repositoryInstance.Get(Guid.Empty);
            Assert.AreSame(repositoryInstance.ServerWorkspace, result);
        }

        [TestMethod]
        public void GetWithServerWorkspaceIDReturnsServerWorkspace()
        {
            Guid workspaceID;
            var repositoryInstance = SetupRepo(out workspaceID);
            var result = repositoryInstance.Get(WorkspaceRepository.ServerWorkspaceID);
            Assert.AreSame(repositoryInstance.ServerWorkspace, result);
        }

        [TestMethod]
        public void GetWithNewWorkspaceIDIncreasesItemCountByOne()
        {
            Guid workspaceID;
            var repositoryInstance = SetupRepo(out workspaceID);
            var expected = repositoryInstance.Count + 1;
            repositoryInstance.Get(Guid.NewGuid());
            Assert.AreEqual(expected, repositoryInstance.Count);
        }

        [TestMethod]
        public void GetWithForceReloadsWorkspace()
        {
            Guid workspaceID;
            var repositoryInstance = SetupRepo(out workspaceID);
            var previous = repositoryInstance.ServerWorkspace;
            var result = repositoryInstance.Get(WorkspaceRepository.ServerWorkspaceID, true);
            Assert.AreNotSame(previous, result);
        }

        #endregion Get Tests

        public static WorkspaceRepository SetupRepo(out Guid workspaceID)
        {
            var repo = new WorkspaceRepository();
            workspaceID = Guid.NewGuid();
             List<IResource> resources;
             ResourceCatalogTests.SaveResources(Guid.Empty, null, true, true, new string[0], new[] { "Calculate_RecordSet_Subtract" }, out resources, new Guid[0], new[] { Guid.NewGuid() });
             ResourceCatalogTests.SaveResources(workspaceID, null, true, true, new string[0], new[] { "Calculate_RecordSet_Subtract" }, out resources, new Guid[0], new[] { Guid.NewGuid() });

            // Force reload of server workspace from _currentTestDir
            ResourceCatalog.Instance.LoadWorkspace(GlobalConstants.ServerWorkspaceID);
            return repo;
        }
    }
}
