
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Data;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    public class ServerVersionRepositoryTests
    {


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void ServerVersionRepostory_Ctor_Null_strategy()
        {
#pragma warning disable 168
            var strat = new Mock<IVersionStrategy>();
#pragma warning restore 168
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var file = new Mock<IFile>();
            var dir = new Mock<IDirectory>();
            const string rootPath = "bob";
            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(null, cat.Object, dir.Object, rootPath, file.Object);


            //------------Execute Test---------------------------
            var items = serverVersionRepostory.GetVersions(resourceId);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, items.Count);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServerVersionRepostory_Ctor_Null_Cataloguey()
        {
#pragma warning disable 168
            var strat = new Mock<IVersionStrategy>();
#pragma warning restore 168

            var resourceId = Guid.NewGuid();
            var file = new Mock<IFile>();
            var dir = new Mock<IDirectory>();
            const string rootPath = "bob";
            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, null, dir.Object, rootPath, file.Object);


            //------------Execute Test---------------------------
            var items = serverVersionRepostory.GetVersions(resourceId);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, items.Count);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServerVersionRepostory_Ctor_Null_dir()
        {
#pragma warning disable 168
            var strat = new Mock<IVersionStrategy>();
#pragma warning restore 168
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var file = new Mock<IFile>();

            const string rootPath = "bob";
            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat.Object, null, rootPath, file.Object);


            //------------Execute Test---------------------------
            var items = serverVersionRepostory.GetVersions(resourceId);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, items.Count);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServerVersionRepostory_Ctor_Null_Path()
        {
#pragma warning disable 168
            var strat = new Mock<IVersionStrategy>();
#pragma warning restore 168
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var file = new Mock<IFile>();
            var dir = new Mock<IDirectory>();
            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat.Object, dir.Object, null, file.Object);


            //------------Execute Test---------------------------
            var items = serverVersionRepostory.GetVersions(resourceId);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, items.Count);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServerVersionRepostory_Ctor_Null_File()
        {
#pragma warning disable 168
            var strat = new Mock<IVersionStrategy>();
#pragma warning restore 168
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();

            var dir = new Mock<IDirectory>();
            const string rootPath = "bob";
            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat.Object, dir.Object, rootPath, null);


            //------------Execute Test---------------------------
            var items = serverVersionRepostory.GetVersions(resourceId);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, items.Count);
        }


        static ServerVersionRepository CreateServerVersionRepository(IVersionStrategy strat, IResourceCatalog cat, IDirectory dir, string rootPath, IFile file)
        {

            var serverVersionRepostory = new ServerVersionRepository(strat, cat, dir, rootPath, file);
            return serverVersionRepostory;
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_GetVersions")]
        public void ServerVersionRepostory_GetVersions_CatologueHasNoResource_ReturnEmpty()
        {
#pragma warning disable 168
            var strat = new Mock<IVersionStrategy>();
#pragma warning restore 168
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var file = new Mock<IFile>();
            var dir = new Mock<IDirectory>();
            const string rootPath = "bob";
            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat.Object, dir.Object, rootPath, file.Object);

            //------------Execute Test---------------------------
            var items = serverVersionRepostory.GetVersions(resourceId);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, items.Count);


        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_GetVersions")]
        public void ServerVersionRepostory_GetVersions_CatologueResource_ResourceIsNotVersioned()
        {

            var strat = new Mock<IVersionStrategy>();
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var file = new Mock<IFile>();
            var dir = new Mock<IDirectory>();
            const string rootPath = "bob";

            var resource = new Mock<IResource>();
            cat.Setup(a => a.GetResource(Guid.Empty, resourceId)).Returns(resource.Object);
            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat.Object, dir.Object, rootPath, file.Object);

            //------------Execute Test---------------------------
            var items = serverVersionRepostory.GetVersions(resourceId);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, items.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_GetVersions")]
        public void ServerVersionRepostory_GetVersions_CatologueResource_ResourceIsVersioned()
        {

            var strat = new Mock<IVersionStrategy>();
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var file = new Mock<IFile>();
            var dir = new Mock<IDirectory>();
            const string rootPath = "bob";

            var resource = new Mock<IResource>();
            cat.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resource.Object);
            resource.Setup(a => a.VersionInfo).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "1", resourceId, versionId));
            resource.Setup(a => a.ResourcePath).Returns("moot\\boot");
            dir.Setup(a => a.GetFiles(It.IsAny<string>())).Returns(new[] { CreateFileName(versionId, 1), CreateFileName(versionId, 2) });
            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat.Object, dir.Object, rootPath, file.Object);

            //------------Execute Test---------------------------
            var items = serverVersionRepostory.GetVersions(resourceId);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, items.Count);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_GetVersions")]
        public void ServerVersionRepostory_StoreVersion_CatologueResource_ResourceIsVersioned()
        {
            //------------Setup for test--------------------------
            var strat = new Mock<IVersionStrategy>();
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var file = new Mock<IFile>();
            var dir = new Mock<IDirectory>();
            const string rootPath = "bob";

            var resource = new Mock<IResource>();
            resource.Setup(a => a.ResourceName).Returns("moon");
            resource.Setup(a => a.ToStringBuilder()).Returns(new StringBuilder("bob"));
            cat.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resource.Object);
            resource.Setup(a => a.VersionInfo).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "12345", resourceId, versionId));
            resource.Setup(a => a.ResourcePath).Returns("moot\\boot");
            dir.Setup(a => a.GetFiles(It.IsAny<string>())).Returns(new[] { CreateFileName(versionId, 1), CreateFileName(versionId, 2) });
            strat.Setup(a => a.GetNextVersion(resource.Object, resource.Object, "usr", "mook")).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "123456", resourceId, versionId));
            strat.Setup(a => a.GetCurrentVersion(resource.Object, resource.Object, "usr", "mook")).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "654321", resourceId, versionId));
            file.Setup(a => a.Copy(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            file.Setup(a => a.Exists(It.IsAny<string>())).Returns(false).Callback(() => file.Setup(a => a.Exists(It.IsAny<string>())).Returns(true));
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat.Object, dir.Object, rootPath, file.Object);
            //------------Execute Test---------------------------
            serverVersionRepostory.StoreVersion(resource.Object, "userName", "password", Guid.Empty);

            //------------Assert Results-------------------------
            file.Verify(a => a.Copy(It.IsAny<string>(), It.IsAny<string>()));
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_GetVersions")]
        public void ServerVersionRepostory_StoreVersion_CatologueResource_ResourceIsNotVersioned()
        {

            var strat = new Mock<IVersionStrategy>();
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var file = new Mock<IFile>();
            var dir = new Mock<IDirectory>();
            const string rootPath = "bob";

            var resource = new Mock<IResource>();
            resource.Setup(a => a.ResourceName).Returns("moon");
            resource.Setup(a => a.ToStringBuilder()).Returns(new StringBuilder("bob"));
            cat.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resource.Object);
            resource.Setup(a => a.VersionInfo).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "12345", resourceId, versionId));
            resource.Setup(a => a.ResourcePath).Returns("moot\\boot");
            dir.Setup(a => a.GetFiles(It.IsAny<string>())).Returns(new[] { CreateFileName(versionId, 1), CreateFileName(versionId, 2) });
            strat.Setup(a => a.GetNextVersion(resource.Object, resource.Object, "usr", "mook")).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "123456", resourceId, versionId));
            strat.Setup(a => a.GetCurrentVersion(resource.Object, resource.Object, "usr", "mook")).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "654321", resourceId, versionId));
            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat.Object, dir.Object, rootPath, file.Object);
            file.Setup(a => a.Exists(It.IsAny<string>())).Returns(true);
            file.Setup(a => a.Copy(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            //------------Execute Test---------------------------
            serverVersionRepostory.StoreVersion(resource.Object, "userName", "password", Guid.Empty);

            //------------Assert Results-------------------------
            file.Verify(a => a.Copy(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_Rollback")]
        [ExpectedException(typeof(VersionNotFoundException))]
        public void ServerVersionRepostory_Rollback_VersionDoesNotExist()
        {

            var strat = new Mock<IVersionStrategy>();
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var file = new Mock<IFile>();
            var dir = new Mock<IDirectory>();
            const string rootPath = "bob";

            var resource = new Mock<IResource>();
            resource.Setup(a => a.ResourceName).Returns("moon");
            resource.Setup(a => a.ToStringBuilder()).Returns(new StringBuilder("bob"));
            cat.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resource.Object);
            resource.Setup(a => a.VersionInfo).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "12345", resourceId, versionId));
            resource.Setup(a => a.ResourcePath).Returns("moot\\boot");
            dir.Setup(a => a.GetFiles(It.IsAny<string>())).Returns(new string[0]);
            strat.Setup(a => a.GetNextVersion(resource.Object, resource.Object, "usr", "mook")).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "123456", resourceId, versionId));
            strat.Setup(a => a.GetCurrentVersion(resource.Object, resource.Object, "usr", "mook")).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "654321", resourceId, versionId));
            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat.Object, dir.Object, rootPath, file.Object);

            file.Setup(a => a.Copy(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            //------------Execute Test---------------------------
            serverVersionRepostory.RollbackTo(resourceId, "2");

            //------------Assert Results-------------------------

        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_Rollback")]
        public void ServerVersionRepostory_Rollback_VersionDoesExist_DifferentNames()
        {
            var strat = new Mock<IVersionStrategy>();
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var file = new Mock<IFile>();
            var dir = new Mock<IDirectory>();
            const string rootPath = "bob";

            var resource = new Mock<IResource>();
            resource.Setup(a => a.ResourceName).Returns("moon");
            resource.Setup(a => a.ToStringBuilder()).Returns(new StringBuilder(ResourceOne));
            file.Setup(a => a.ReadAllText(It.IsAny<string>())).Returns(ResourceOne);
            cat.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resource.Object).Verifiable();

            resource.Setup(a => a.VersionInfo).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "12345", resourceId, versionId));
            resource.Setup(a => a.ResourcePath).Returns("moot\\boot");
            dir.Setup(a => a.GetFiles(It.IsAny<string>())).Returns(new[] { versionId + "_2_" + DateTime.Now.Ticks + "_jjj" });
            strat.Setup(a => a.GetNextVersion(resource.Object, resource.Object, "usr", "mook")).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "123456", resourceId, versionId));
            strat.Setup(a => a.GetCurrentVersion(resource.Object, resource.Object, "usr", "mook")).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "654321", resourceId, versionId));
            cat.Setup(a => a.SaveResource(Guid.Empty, It.IsAny<StringBuilder>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat.Object, dir.Object, rootPath, file.Object);
            //------------Execute Test---------------------------
            var res = serverVersionRepostory.RollbackTo(resourceId, "2");

            //------------Assert Results-------------------------
            Assert.AreEqual(res.VersionHistory.Count, 1);
            cat.Verify(a => a.SaveResource(Guid.Empty, It.IsAny<StringBuilder>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            cat.Verify(a => a.DeleteResource(Guid.Empty, "moon", "Unknown", null, false));
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_Rollback")]
        public void ServerVersionRepostory_Rollback_VersionDoesExist_SameName()
        {
            var strat = new Mock<IVersionStrategy>();
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var file = new Mock<IFile>();
            var dir = new Mock<IDirectory>();
            const string rootPath = "bob";

            var resource = new Mock<IResource>();
            resource.Setup(a => a.ResourceName).Returns("UnitTestResource");
            resource.Setup(a => a.ToStringBuilder()).Returns(new StringBuilder(ResourceOne));
            file.Setup(a => a.ReadAllText(It.IsAny<string>())).Returns(ResourceOne);
            cat.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resource.Object).Verifiable();

            resource.Setup(a => a.VersionInfo).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "12345", resourceId, versionId));
            resource.Setup(a => a.ResourcePath).Returns("UnitTestResource");
            dir.Setup(a => a.GetFiles(It.IsAny<string>())).Returns(new[] { versionId + "_2_" + DateTime.Now.Ticks + "_jjj" });
            strat.Setup(a => a.GetNextVersion(resource.Object, resource.Object, "usr", "mook")).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "123456", resourceId, versionId));
            strat.Setup(a => a.GetCurrentVersion(resource.Object, resource.Object, "usr", "mook")).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "654321", resourceId, versionId));
            cat.Setup(a => a.SaveResource(Guid.Empty, It.IsAny<StringBuilder>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat.Object, dir.Object, rootPath, file.Object);
            //------------Execute Test---------------------------
            var res = serverVersionRepostory.RollbackTo(resourceId, "2");

            //------------Assert Results-------------------------
            Assert.AreEqual(res.VersionHistory.Count, 1);
            cat.Verify(a => a.SaveResource(Guid.Empty, It.IsAny<StringBuilder>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            cat.Verify(a => a.DeleteResource(Guid.Empty, "moon", "Unknown", null, false), Times.Never());
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_Rollback")]
        public void ServerVersionRepostory_Delete_VersionDoesExist()
        {

            var strat = new Mock<IVersionStrategy>();
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var file = new Mock<IFile>();
            var dir = new Mock<IDirectory>();
            const string rootPath = "bob";
            var dt = DateTime.Now;
            var resource = new Mock<IResource>();
            resource.Setup(a => a.ResourceName).Returns("moon");
            resource.Setup(a => a.ToStringBuilder()).Returns(new StringBuilder("bob"));
            cat.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resource.Object).Verifiable();
            resource.Setup(a => a.VersionInfo).Returns(new VersionInfo(dt, "mook", "usr", "12345", resourceId, versionId));
            resource.Setup(a => a.ResourcePath).Returns("moot\\boot");
            dir.Setup(a => a.GetFiles(It.IsAny<string>())).Returns(new[] { versionId + "_2_" + dt.Ticks + "_jjj" });


            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat.Object, dir.Object, rootPath, file.Object);
            //------------Execute Test---------------------------
            var res = serverVersionRepostory.DeleteVersion(resourceId, "2");
            var filedel = versionId.ToString() + "_2_" + dt.Ticks + "_jjj";

            //------------Assert Results-------------------------
            Assert.AreEqual(res.Count, 1);
            file.Verify(a => a.Delete(filedel));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_Move")]
        public void ServerVersionRepostory_Move_VersionDoesExist()
        {

            var strat = new Mock<IVersionStrategy>();
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var file = new Mock<IFile>();
            var dir = new Mock<IDirectory>();
            const string rootPath = "bob";
            var dt = DateTime.Now;
            bool moov = false;
            var resource = new Mock<IResource>();
            resource.Setup(a => a.ResourceName).Returns("moon");
            resource.Setup(a => a.ToStringBuilder()).Returns(new StringBuilder("bob"));
            cat.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resource.Object).Verifiable();
            resource.Setup(a => a.VersionInfo).Returns(new VersionInfo(dt, "mook", "usr", "12345", resourceId, versionId));
            resource.Setup(a => a.ResourcePath).Returns("moot\\boot");
            dir.Setup(a => a.GetFiles(It.IsAny<string>())).Returns(new[] { versionId + "_2_" + dt.Ticks + "_jjj" });
            file.Setup(a => a.Move(It.IsAny<string>(), It.IsAny<string>())).Callback((string a, string b)=>
            {
                moov = a.Contains(versionId.ToString()) && b.Contains(versionId.ToString()) && b.Contains("222aaa"); ;
                    
            });

            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat.Object, dir.Object, rootPath, file.Object);
            //------------Execute Test---------------------------
            serverVersionRepostory.MoveVersions(resourceId, "222aaa");

            file.Verify(a => a.Move(It.IsAny<string>(),It.IsAny<string>()));
            Assert.IsTrue(moov);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_Move")]
        public void ServerVersionRepostory_Move_VersionInfoDoesNotExist()
        {

            var strat = new Mock<IVersionStrategy>();
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var file = new Mock<IFile>();
            var dir = new Mock<IDirectory>();
            const string rootPath = "bob";
            var dt = DateTime.Now;
            bool moov = false;
            var resource = new Mock<IResource>();
            resource.Setup(a => a.ResourceName).Returns("moon");
            resource.Setup(a => a.ToStringBuilder()).Returns(new StringBuilder("bob"));
            cat.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resource.Object).Verifiable();
          
            resource.Setup(a => a.ResourcePath).Returns("moot\\boot");
            dir.Setup(a => a.GetFiles(It.IsAny<string>())).Returns(new[] { versionId + "_2_" + dt.Ticks + "_jjj" });
            file.Setup(a => a.Move(It.IsAny<string>(), It.IsAny<string>())).Callback((string a, string b) =>
            {
                moov = a.Contains(versionId.ToString()) && b.Contains(versionId.ToString()) && b.Contains("222aaa"); ;

            });

            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat.Object, dir.Object, rootPath, file.Object);
            //------------Execute Test---------------------------
            serverVersionRepostory.MoveVersions(resourceId, "222aaa");


            Assert.IsFalse(moov);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_Move")]
        public void ServerVersionRepostory_Move_VersionDoesExistDirectoryDoesNotExist()
        {

            var strat = new Mock<IVersionStrategy>();
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var file = new Mock<IFile>();
            var dir = new Mock<IDirectory>();
            const string rootPath = "bob";
            var dt = DateTime.Now;
            bool moov = false;
            var resource = new Mock<IResource>();
            resource.Setup(a => a.ResourceName).Returns("moon");
            resource.Setup(a => a.ToStringBuilder()).Returns(new StringBuilder("bob"));
            cat.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resource.Object).Verifiable();
            resource.Setup(a => a.VersionInfo).Returns(new VersionInfo(dt, "mook", "usr", "12345", resourceId, versionId));
            resource.Setup(a => a.ResourcePath).Returns("moot\\boot");
            dir.Setup(a => a.GetFiles(It.IsAny<string>())).Returns(new[] { versionId + "_2_" + dt.Ticks + "_jjj" });
            file.Setup(a => a.Move(It.IsAny<string>(), It.IsAny<string>())).Callback((string a, string b) =>
            {
                moov = a.Contains(versionId.ToString()) && b.Contains(versionId.ToString()) && b.Contains("222aaa"); ;

            });
            dir.Setup(a => a.Exists(It.IsAny<string>())).Returns(true);
            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat.Object, dir.Object, rootPath, file.Object);
            //------------Execute Test---------------------------
            serverVersionRepostory.MoveVersions(resourceId, "222aaa");

            file.Verify(a => a.Move(It.IsAny<string>(), It.IsAny<string>()));
            Assert.IsTrue(moov);
            dir.Verify(a=>a.CreateIfNotExists(It.IsAny<string>()));
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerVersionRepostory_Delete")]
        public void ServerVersionRepostory_Delete_VersionDoesNotExist()
        {

            var strat = new Mock<IVersionStrategy>();
            var cat = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var file = new Mock<IFile>();
            var dir = new Mock<IDirectory>();
            const string rootPath = "bob";
            var dt = DateTime.Now;
            var resource = new Mock<IResource>();
            resource.Setup(a => a.ResourceName).Returns("moon");
            resource.Setup(a => a.ToStringBuilder()).Returns(new StringBuilder("bob"));
            cat.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resource.Object).Verifiable();
            resource.Setup(a => a.VersionInfo).Returns(new VersionInfo(dt, "mook", "usr", "12345", resourceId, versionId));
            resource.Setup(a => a.ResourcePath).Returns("moot\\boot");
            dir.Setup(a => a.GetFiles(It.IsAny<string>())).Returns(new string[0]);


            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat.Object, dir.Object, rootPath, file.Object);
            //------------Execute Test---------------------------
            var res = serverVersionRepostory.DeleteVersion(resourceId, "2");
            var filedel = versionId.ToString() + "_2_" + dt.Ticks + "_jjj";

            //------------Assert Results-------------------------
            Assert.AreEqual(res.Count, 0);
            file.Verify(a => a.Delete(filedel), Times.Never());

        }

        string CreateFileName(Guid versionId, int version)
        {
            return string.Format("{0}_{1}_{2}_{3}", versionId, version, DateTime.Now.Ticks, "bob");

        }

        // ReSharper restore InconsistentNaming

        const string ResourceOne = @"<Service ID=""fef087f1-18ba-406d-a9da-44b6aa2dd1bf"" Version=""1.0"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" Name=""UnitTestResource"" ResourceType=""WorkflowService"" IsValid=""false"">
  <DisplayName>UnitTestResource</DisplayName>
  <Category>UnitTestResource</Category>
  <IsNewWorkflow>false</IsNewWorkflow>
  <AuthorRoles>
  </AuthorRoles>
  <Comment>
  </Comment>
  <Tags>
  </Tags>
  <IconPath>
  </IconPath>
  <HelpLink>
  </HelpLink>
  <UnitTestTargetWorkflowService>
  </UnitTestTargetWorkflowService>
  <DataList>
  </DataList>
  <Action Name=""InvokeWorkflow"" Type=""Workflow"">
    <XamlDefinition>&lt;Activity x:Class=""UnitTestResource"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:dc=""clr-namespace:Dev2.Common;assembly=Dev2.Common"" xmlns:ddc=""clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data"" xmlns:ddcb=""clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data"" xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data"" xmlns:dddo=""clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data"" xmlns:ddsm=""clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data"" xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:sco=""clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""&gt;&lt;x:Members&gt;&lt;x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" /&gt;&lt;x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" /&gt;&lt;x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;654,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""6""&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=""AssemblyReference""&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=""UnitTestResource"" sap:VirtualizedContainerService.HintSize=""614,636""&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""LastResult"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" /&gt;&lt;Variable x:TypeArguments=""uaba:Util"" Name=""t"" /&gt;&lt;Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;60,75&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Null /&gt;&lt;/Flowchart.StartNode&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>
  </Action>
  <ErrorMessages />
  <VersionInfo DateTimeStamp=""2014-08-19T11:18:11.5677239+02:00"" Reason=""Save"" User=""Unknown"" VersionNumber=""1"" ResourceId=""fef087f1-18ba-406d-a9da-44b6aa2dd1bf"" VersionId=""ee694a65-37ca-4e0c-9741-a6d39dd5c12a"" />
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>AIWhvyy9UufXE9STU6Q0MIV4T+0=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>Sxuo1EtJA0TzLFcLOYK4VC9j3I+/FOMLbal3PtTukuUcwOPp/bP3PSrthsSclpPD3+nWyw+yIDReKTXxiqn67k0CEq4wtETI/YGJlcRiDAenkSvEv51YfAsABwWG9baJw42FEJdOf3oAQnRh1pPYX897+Yr2a1D2L32JwT8gxQ8=</SignatureValue>
  </Signature>
</Service>";
    }
}
