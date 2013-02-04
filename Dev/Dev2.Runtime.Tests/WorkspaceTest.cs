using Dev2.DynamicServices.Test.XML;
using Dev2.Runtime.Security;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Dev2.DynamicServices.Test
{
    /// <summary>
    /// Summary description for WorkspaceTest
    /// </summary>
    [TestClass]
    public class WorkspaceTest
    {
        public static readonly Guid TestWorkspaceID = new Guid("B1890C86-95D8-4612-A7C3-953250ED237A");
        public static XElement TestWorkspaceItemXml = XmlResource.Fetch("WorkspaceItem");

        const string ServiceName = "Calculate_RecordSet_Subtract";
        const enDynamicServiceObjectType ServiceType = enDynamicServiceObjectType.DynamicService;

        static string _workspacesPath;
        static string _servicesPath;
        static object l = new object();

        static WorkspaceRepository _testInstance;


        #region Class Initialization/Cleanup

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            // HACK: shorten test directory path by removing auto generated folder....
            var parentDir = Directory.GetParent(testContext.TestDir);
            var tempDir = testContext.TestDir.Remove(0, parentDir.FullName.Length);
            var keys = testContext.Properties.Keys.OfType<string>().ToList();
            foreach(var key in keys)
            {
                var value = testContext.Properties[key] as string;
                if(value != null)
                {
                    testContext.Properties[key] = value.Replace(tempDir, string.Empty);
                }
            }


            Directory.SetCurrentDirectory(testContext.TestDir);
            _workspacesPath = Path.Combine(testContext.TestDir, "Workspaces");
            _servicesPath = Path.Combine(testContext.TestDir, "Services");
            Directory.CreateDirectory(_servicesPath);

            var xml = XmlResource.Fetch(ServiceName);
            xml.Save(Path.Combine(_servicesPath, ServiceName + ".xml"));
        }

        [ClassCleanup]
        public static void MyClassCleanup()
        {
            if(Directory.Exists(_workspacesPath))
            {
                Directory.Delete(_workspacesPath, true);
            }
            if(Directory.Exists(_servicesPath))
            {
                Directory.Delete(_servicesPath, true);
            }
        }

        #endregion

        #region Update

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateWithNull()
        {
            lock(l)
            {
                var workspace = new Workspace(TestWorkspaceID);
                workspace.Update(null);
            }
        }

        [TestMethod]
        public void UpdateWorkItemWithEditAction()
        {
            lock(l)
            {
                var workspaceItem = new Mock<IWorkspaceItem>();
                workspaceItem.Setup(m => m.Action).Returns(WorkspaceItemAction.Edit);
                workspaceItem.Setup(m => m.ServiceName).Returns(ServiceName);
                workspaceItem.Setup(m => m.ServiceType).Returns(ServiceType.ToString);

                var workspace = WorkspaceRepository.Instance.Get(TestWorkspaceID);
                var previous = workspace.Host.Find(ServiceName, ServiceType);
                workspace.Update(workspaceItem.Object, previous.AuthorRoles);
                var next = workspace.Host.Find(ServiceName, ServiceType);
                Assert.AreNotSame(previous, next);
            }
        }


        [TestMethod]
        public void UpdateWorkItemWithDiscardAction()
        {
            lock(l)
            {
                UpdateWorkItemWithEditAction();
            }
        }

        [TestMethod]
        public void UpdateWorkItemWithCommitAction()
        {
            lock(l)
            {
                var workspaceItem = new Mock<IWorkspaceItem>();
                workspaceItem.Setup(m => m.Action).Returns(WorkspaceItemAction.Commit);
                workspaceItem.Setup(m => m.ServiceName).Returns(ServiceName);
                workspaceItem.Setup(m => m.ServiceType).Returns(ServiceType.ToString);

                var previous = WorkspaceRepository.Instance.ServerWorkspace.Host.Find(ServiceName, ServiceType);

                var workspace = WorkspaceRepository.Instance.Get(TestWorkspaceID);
                workspace.Update(workspaceItem.Object, previous.AuthorRoles);
                var next = WorkspaceRepository.Instance.ServerWorkspace.Host.Find(ServiceName, ServiceType);
                Assert.AreNotSame(previous, next);
            }
        }


        [TestMethod]
        public void UpdateWorkItemWithCommitActionOnSameWorkspace()
        {
            lock(l)
            {
                var workspaceItem = new Mock<IWorkspaceItem>();
                workspaceItem.Setup(m => m.Action).Returns(WorkspaceItemAction.Commit);
                workspaceItem.Setup(m => m.ServiceName).Returns(ServiceName);
                workspaceItem.Setup(m => m.ServiceType).Returns(ServiceType.ToString);

                var previous = WorkspaceRepository.Instance.ServerWorkspace.Host.Find(ServiceName, ServiceType);

                WorkspaceRepository.Instance.ServerWorkspace.Update(workspaceItem.Object, previous.AuthorRoles);
                var next = WorkspaceRepository.Instance.ServerWorkspace.Host.Find(ServiceName, ServiceType);
                Assert.AreSame(previous, next);
            }
        }

        [TestMethod]
        public void VerifyXMLSpeedTest()
        {

            lock(l)
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
        }

        #endregion

        #region Delete

        [TestMethod]
        public void DeleteDecreasesItemCountByOne()
        {
            lock (l)
            {
                // this will add
                var workspace = _testInstance.Get(Guid.NewGuid());
                var expected = _testInstance.Count - 1;
                _testInstance.Delete(workspace);
                Assert.AreEqual(expected, _testInstance.Count);
            }

        }

        [TestMethod]
        public void DeleteNullItem_Expected_NoOperationPerformed()
        {
            // this will add
            lock (l)
            {
                var expected = _testInstance.Count;
                _testInstance.Delete(null);
                Assert.AreEqual(expected, _testInstance.Count);
            }
        }

        #endregion Delete

        #region Save

        [TestMethod]
        public void SaveWithNewWorkspaceIncreasesItemCountByOne()
        {
            lock (l)
            {
                var expected = _testInstance.Count + 1;
                var workspace = new Workspace(Guid.NewGuid());
                _testInstance.Save(workspace);
                Assert.AreEqual(expected, _testInstance.Count);
            }
        }

        [TestMethod]
        public void SaveWithNullWorkspaceIncreasesItemCountByOne()
        {
            lock (l)
            {
                var expected = _testInstance.Count;
                _testInstance.Save(null);
                Assert.AreEqual(expected, _testInstance.Count);
            }
        }

        #endregion Save

        #region CTOR Tests

        [TestMethod]
        public void ServerWorkspaceCreatedAfterInstantiation()
        {
            Assert.IsNotNull(_testInstance.ServerWorkspace);
        }

        [TestMethod]
        public void ServerWorkspaceCreatedWithServerWorkspaceID()
        {
            Assert.AreEqual(WorkspaceRepository.ServerWorkspaceID, _testInstance.ServerWorkspace.ID);
        }

        #endregion CTOR Tests

        #region Get Tests

        [TestMethod]
        public void GetWithEmptyGuidReturnsServerWorkspace()
        {
            var result = _testInstance.Get(Guid.Empty);
            Assert.AreSame(_testInstance.ServerWorkspace, result);
        }

        [TestMethod]
        public void GetWithServerWorkspaceIDReturnsServerWorkspace()
        {
            var result = _testInstance.Get(WorkspaceRepository.ServerWorkspaceID);
            Assert.AreSame(_testInstance.ServerWorkspace, result);
        }

        [TestMethod]
        public void GetWithNewWorkspaceIDIncreasesItemCountByOne()
        {
            var expected = _testInstance.Count + 1;
            var workspace = _testInstance.Get(Guid.NewGuid());
            Assert.AreEqual(expected, _testInstance.Count);
        }

        [TestMethod]
        public void GetWithForceReloadsWorkspace()
        {
            var previous = _testInstance.ServerWorkspace;
            var result = _testInstance.Get(WorkspaceRepository.ServerWorkspaceID, true);
            Assert.AreNotSame(previous, result);
        }

        #endregion Get Tests

    }
}
