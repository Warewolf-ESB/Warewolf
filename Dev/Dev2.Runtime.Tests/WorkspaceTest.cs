using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Dev2.Common;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.Hosting;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.DynamicServices.Test
{
    /// <summary>
    /// Summary description for WorkspaceTest
    /// </summary>
    [TestClass]
    [Ignore]
    public class WorkspaceTest
    {
        const string ServiceName = "Calculate_RecordSet_Subtract";

        const enDynamicServiceObjectType ServiceType = enDynamicServiceObjectType.DynamicService;

        static string _testDir;

        static Guid _workspaceID;
        static int _currentTestNum;
        static string _currentTestDir;

        #region ClassInitialize

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            //// HACK: shorten test directory path by removing auto generated folder....
            //var parentDir = Directory.GetParent(testContext.TestDir);
            //var tempDir = testContext.TestDir.Remove(0, parentDir.FullName.Length);
            //var keys = testContext.Properties.Keys.OfType<string>().ToList();
            //foreach(var key in keys)
            //{
            //    var value = testContext.Properties[key] as string;
            //    if(value != null)
            //    {
            //        testContext.Properties[key] = value.Replace(tempDir, string.Empty);
            //    }
            //}

            _testDir = testContext.TestDir;
            _currentTestNum = 0;
        }

        #endregion

        #region TestInitialize/Cleanup

        static readonly object TestLock = new object();

        [TestInitialize]
        public void TestInitialize()
        {
            Monitor.Enter(TestLock);

            // User int's to keep dir name short!
            _currentTestNum++;
            _currentTestDir = Path.Combine(_testDir, _currentTestNum.ToString(CultureInfo.InvariantCulture));
            Directory.CreateDirectory(_currentTestDir);

            // Set current directory to new one each time so that tests do not clash
            Directory.SetCurrentDirectory(_currentTestDir);

            _workspaceID = Guid.NewGuid();
            List<IResource> resources;
            ResourceCatalogTests.SaveResources(Guid.Empty, _workspaceID, null, true, true, new string[0], new[] { ServiceName }, out resources);

            // Force reload of server workspace from _currentTestDir
            ResourceCatalog.Instance.LoadWorkspace(GlobalConstants.ServerWorkspaceID);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            var repositoryInstance = WorkspaceRepository.Instance;
            repositoryInstance.RefreshWorkspaces();

            Monitor.Exit(TestLock);
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
        public void UpdateWorkItemWithEditAction()
        {
            var workspaceItem = new Mock<IWorkspaceItem>();
            workspaceItem.Setup(m => m.Action).Returns(WorkspaceItemAction.Edit);
            workspaceItem.Setup(m => m.ServiceName).Returns(ServiceName);
            workspaceItem.Setup(m => m.ServiceType).Returns(ServiceType.ToString);

            var workspace = WorkspaceRepository.Instance.Get(_workspaceID);

            var previous = ResourceCatalog.Instance.GetResource(_workspaceID, ServiceName);
            workspace.Update(workspaceItem.Object, previous.AuthorRoles);
            var next = ResourceCatalog.Instance.GetResource(_workspaceID, ServiceName);
            Assert.AreNotSame(previous, next);
        }


        [TestMethod]
        public void UpdateWorkItemWithDiscardAction()
        {
            UpdateWorkItemWithEditAction();
        }

        [TestMethod]
        public void UpdateWorkItemWithCommitAction()
        {
            var workspaceItem = new Mock<IWorkspaceItem>();
            workspaceItem.Setup(m => m.Action).Returns(WorkspaceItemAction.Commit);
            workspaceItem.Setup(m => m.ServiceName).Returns(ServiceName);
            workspaceItem.Setup(m => m.ServiceType).Returns(ServiceType.ToString);

            var workspace = WorkspaceRepository.Instance.Get(_workspaceID);

            var previous = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, ServiceName);
            workspace.Update(workspaceItem.Object, previous.AuthorRoles);
            var next = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, ServiceName);
            Assert.AreNotSame(previous, next);
        }

        [TestMethod]
        public void UpdateWorkItemWithCommitActionOnSameWorkspace()
        {
            var workspaceItem = new Mock<IWorkspaceItem>();
            workspaceItem.Setup(m => m.Action).Returns(WorkspaceItemAction.Commit);
            workspaceItem.Setup(m => m.ServiceName).Returns(ServiceName);
            workspaceItem.Setup(m => m.ServiceType).Returns(ServiceType.ToString);

            var workspace = WorkspaceRepository.Instance.Get(GlobalConstants.ServerWorkspaceID);

            var previous = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, ServiceName);
            workspace.Update(workspaceItem.Object, previous.AuthorRoles);
            var next = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, ServiceName);
            Assert.AreSame(previous, next);
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
            theHostProvider.VerifyXml(xmlToVerify);
            TimeSpan duration = DateTime.Now - theTime;
            Assert.IsTrue(duration.TotalMilliseconds < 20, "Duration: " + duration.TotalMilliseconds + "ms");
        }

        #endregion

        #region Delete

        [TestMethod]
        public void DeleteDecreasesItemCountByOne()
        {
            // this will add           
            var repositoryInstance = WorkspaceRepository.Instance;

            var workspace = repositoryInstance.Get(_workspaceID);
            var expected = repositoryInstance.Count - 1;
            repositoryInstance.Delete(workspace);
            Assert.AreEqual(expected, repositoryInstance.Count);

        }

        [TestMethod]
        public void DeleteNullItemExpectedNoOperationPerformed()
        {
            var repositoryInstance = WorkspaceRepository.Instance;
            var expected = repositoryInstance.Count;
            repositoryInstance.Delete(null);
            Assert.AreEqual(expected, repositoryInstance.Count);
        }

        #endregion Delete

        #region Save

        [TestMethod]
        public void SaveWithNewWorkspaceIncreasesItemCountByOne()
        {
            var repositoryInstance = WorkspaceRepository.Instance;
            var expected = repositoryInstance.Count + 1;
            Guid myGuid = Guid.NewGuid();
            var workspace = new Workspace(myGuid);
            repositoryInstance.Save(workspace);
            Assert.AreEqual(expected, repositoryInstance.Count);
        }

        [TestMethod]
        public void SaveWithNullWorkspaceIncreasesItemCountByOne()
        {
            var repositoryInstance = WorkspaceRepository.Instance;
            var expected = repositoryInstance.Count;
            repositoryInstance.Save(null);
            Assert.AreEqual(expected, repositoryInstance.Count);
        }

        #endregion Save

        #region CTOR Tests

        [TestMethod]
        public void ServerWorkspaceCreatedAfterInstantiation()
        {
            var repositoryInstance = WorkspaceRepository.Instance;
            Assert.IsNotNull(repositoryInstance.ServerWorkspace);
        }

        [TestMethod]
        public void ServerWorkspaceCreatedWithServerWorkspaceID()
        {
            var repositoryInstance = WorkspaceRepository.Instance;
            Assert.AreEqual(WorkspaceRepository.ServerWorkspaceID, repositoryInstance.ServerWorkspace.ID);
        }

        #endregion CTOR Tests

        #region Get Tests

        [TestMethod]
        public void GetWithEmptyGuidReturnsServerWorkspace()
        {
            var repositoryInstance = WorkspaceRepository.Instance;
            var result = repositoryInstance.Get(Guid.Empty);
            Assert.AreSame(repositoryInstance.ServerWorkspace, result);
        }

        [TestMethod]
        public void GetWithServerWorkspaceIDReturnsServerWorkspace()
        {
            var repositoryInstance = WorkspaceRepository.Instance;
            var result = repositoryInstance.Get(WorkspaceRepository.ServerWorkspaceID);
            Assert.AreSame(repositoryInstance.ServerWorkspace, result);
        }

        [TestMethod]
        public void GetWithNewWorkspaceIDIncreasesItemCountByOne()
        {

            var repositoryInstance = WorkspaceRepository.Instance;
            var expected = repositoryInstance.Count + 1;
            repositoryInstance.Get(Guid.NewGuid());
            Assert.AreEqual(expected, repositoryInstance.Count);
        }

        [TestMethod]
        public void GetWithForceReloadsWorkspace()
        {
            var repositoryInstance = WorkspaceRepository.Instance;
            var previous = repositoryInstance.ServerWorkspace;
            var result = repositoryInstance.Get(WorkspaceRepository.ServerWorkspaceID, true);
            Assert.AreNotSame(previous, result);
        }

        #endregion Get Tests
    }
}
