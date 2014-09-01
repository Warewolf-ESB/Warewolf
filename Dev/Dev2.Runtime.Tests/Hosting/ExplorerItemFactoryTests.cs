using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Runtime.Hosting;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Hosting
{
    static class MoqUtil
    {
        public static IEnumerable<Mock<T>> GenerateMockEnumerable<T>(int count) where T : class
        {
            for(int i = 0; i < count; i++)
                yield return new Mock<T>();
        }


        public static IEnumerable<T> ProxiesFromMockEnumerable<T>(IEnumerable<Mock<T>> values) where T : class
        {
            return values.Select(a => a.Object);
        }
    }

    [TestClass]
    public class ExplorerItemFactoryTests
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemFactory_Constructor")]
        public void ExplorerItemFactory_Constructor_AssertCatalogueSetup()
        {

            //------------Setup for test--------------------------
            var catalogue = new Mock<IResourceCatalog>();
            var directory = new Mock<IDirectory>();
            var auth = new Mock<IAuthorizationService>();
            var explorerItemFactory = new ExplorerItemFactory(catalogue.Object, directory.Object, auth.Object);


            //------------Assert Results-------------------------
            Assert.AreEqual(catalogue.Object, explorerItemFactory.Catalogue);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemFactory_Constructor")]
        public void ExplorerItemFactory_IsChild_AssertStringEqualitySameAsIsChild()
        {

            //------------Setup for test--------------------------
            var catalogue = new Mock<IResourceCatalog>();
            var directory = new Mock<IDirectory>();
            var auth = new Mock<IAuthorizationService>();
            var explorerItemFactory = new ExplorerItemFactory(catalogue.Object, directory.Object, auth.Object);
            Assert.IsTrue(explorerItemFactory.IsChild("a", "a"));
            Assert.IsFalse(explorerItemFactory.IsChild("cd", "cde"));


        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemFactory_BuildRootNode")]
        public void ExplorerItemFactory_BuildRootNode_AssertRootNode_Correct()
        {

            //------------Setup for test--------------------------
            var catalogue = new Mock<IResourceCatalog>();
            var directory = new Mock<IDirectory>();
            var auth = new Mock<IAuthorizationService>();
            auth.Setup(c => c.GetResourcePermissions(It.IsAny<Guid>())).Returns(Permissions.View);
            catalogue.Setup(a => a.GetResourceList(It.IsAny<Guid>())).Returns(new List<IResource>());

            var explorerItemFactory = new ExplorerItemFactory(catalogue.Object, directory.Object, auth.Object);
            //------------Execute Test---------------------------
            var root = explorerItemFactory.BuildRoot();

            //------------Assert Results-------------------------
            Assert.AreEqual(Environment.MachineName, root.DisplayName);
            Assert.AreEqual(0, root.Children.Count);
            Assert.AreEqual(Guid.Empty, root.ResourceId);
            Assert.AreEqual(Permissions.View, root.Permissions);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemFactory_CreateExplorerItem")]
        public void ExplorerItemFactory_CreateExplorerItem_NoResources_ExpectARoot()
        {

            //------------Setup for test--------------------------
            var catalogue = new Mock<IResourceCatalog>();
            var directory = new Mock<IDirectory>();
            var auth = new Mock<IAuthorizationService>();
            catalogue.Setup(a => a.GetResourceList(It.IsAny<Guid>())).Returns(new List<IResource>());

            var explorerItemFactory = new ExplorerItemFactory(catalogue.Object, directory.Object, auth.Object);

            //------------Execute Test---------------------------
            var item = explorerItemFactory.CreateRootExplorerItem(@"b:\bob", Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.AreEqual(Environment.MachineName, item.DisplayName);
            Assert.AreEqual(0, item.Children.Count);
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemFactory_CreateExplorerItem")]
        public void ExplorerItemFactory_CreateExplorerItem_FirstGen_ExpectFirstGenChildren()
        {

            //------------Setup for test--------------------------
            var catalogue = new Mock<IResourceCatalog>();
            var directory = new Mock<IDirectory>();
            var resources = MoqUtil.GenerateMockEnumerable<IResource>(4).ToList();
            var auth = new Mock<IAuthorizationService>();
            for(int i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];
                resource.Setup(a => a.ResourcePath).Returns("");
                resource.Setup(a => a.ResourceName).Returns(i.ToString);
                resource.Setup(a => a.ResourceID).Returns(Guid.NewGuid);

            }
            catalogue.Setup(a => a.GetResourceList(It.IsAny<Guid>())).Returns(MoqUtil.ProxiesFromMockEnumerable(resources).ToList());
            var explorerItemFactory = new ExplorerItemFactory(catalogue.Object, directory.Object, auth.Object);

            //------------Execute Test---------------------------
            var item = explorerItemFactory.CreateRootExplorerItem(@"b:\bob", Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.AreEqual(Environment.MachineName, item.DisplayName);
            Assert.AreEqual(4, resources.Count);
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemFactory_CreateExplorerItem")]
        public void ExplorerItemFactory_CreateExplorerItem_SecondtGen_ExpectSecondGenChildren()
        {

            //------------Setup for test--------------------------
            var catalogue = new Mock<IResourceCatalog>();
            var directory = new Mock<IDirectory>();
            var auth = new Mock<IAuthorizationService>();
            var resources = MoqUtil.GenerateMockEnumerable<IResource>(4).ToList();
            for(int i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];
                resource.Setup(a => a.ResourcePath).Returns("1\\" + i);
                resource.Setup(a => a.ResourceName).Returns(i.ToString);
                resource.Setup(a => a.ResourceID).Returns(Guid.NewGuid);

            }
            directory.Setup(a => a.GetDirectories(@"b:\bob")).Returns(new[] { @"b:\bob\1" });
            catalogue.Setup(a => a.GetResourceList(It.IsAny<Guid>())).Returns(MoqUtil.ProxiesFromMockEnumerable(resources).ToList());
            var explorerItemFactory = new ExplorerItemFactory(catalogue.Object, directory.Object, auth.Object);

            //------------Execute Test---------------------------
            var item = explorerItemFactory.CreateRootExplorerItem(@"b:\bob", Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.AreEqual(Environment.MachineName, item.DisplayName);
            Assert.AreEqual(1, item.Children.Count);
            Assert.AreEqual(4, item.Children[0].Children.Count);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemFactory_CreateExplorerItem")]
        public void ExplorerItemFactory_CreateExplorerItem_MultipleGenerations_ExpectSecondAndFirstGenChildren()
        {

            //------------Setup for test--------------------------
            var catalogue = new Mock<IResourceCatalog>();
            var directory = new Mock<IDirectory>();
            var auth = new Mock<IAuthorizationService>();
            var resources = MoqUtil.GenerateMockEnumerable<IResource>(4).ToList();
            for(int i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];
                resource.Setup(a => a.ResourcePath).Returns(i % 2 == 0 ? i.ToString(CultureInfo.InvariantCulture) : "1\\" + i);
                resource.Setup(a => a.ResourceName).Returns(i.ToString);
                resource.Setup(a => a.ResourceID).Returns(Guid.NewGuid);

            }
            directory.Setup(a => a.GetDirectories(@"b:\bob")).Returns(new[] { @"b:\bob\1" });
            catalogue.Setup(a => a.GetResourceList(It.IsAny<Guid>())).Returns(MoqUtil.ProxiesFromMockEnumerable(resources).ToList());
            var explorerItemFactory = new ExplorerItemFactory(catalogue.Object, directory.Object, auth.Object);

            //------------Execute Test---------------------------
            var item = explorerItemFactory.CreateRootExplorerItem(@"b:\bob", Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.AreEqual(Environment.MachineName, item.DisplayName);
            Assert.AreEqual(3, item.Children.Count);
            Assert.AreEqual(2, item.Children[0].Children.Count);
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemFactory_CreateExplorerItem")]
        public void ExplorerItemFactory_CreateExplorerItemWithType_FirstGen_ExpectFirstGenChildren()
        {

            //------------Setup for test--------------------------
            var catalogue = new Mock<IResourceCatalog>();
            var directory = new Mock<IDirectory>();
            var auth = new Mock<IAuthorizationService>();
            var resources = MoqUtil.GenerateMockEnumerable<IResource>(4).ToList();
            for(int i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];
                resource.Setup(a => a.ResourcePath).Returns("" + i);
                resource.Setup(a => a.ResourceName).Returns(i.ToString);
                resource.Setup(a => a.ResourceID).Returns(Guid.NewGuid);

            }
            resources[0].Setup(a => a.ResourceType).Returns(ResourceType.EmailSource);
            catalogue.Setup(a => a.GetResourceList(It.IsAny<Guid>())).Returns(MoqUtil.ProxiesFromMockEnumerable(resources).ToList());
            var explorerItemFactory = new ExplorerItemFactory(catalogue.Object, directory.Object, auth.Object);

            //------------Execute Test---------------------------
            var item = explorerItemFactory.CreateRootExplorerItem(ResourceType.EmailSource, @"b:\bob", Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.AreEqual(Environment.MachineName, item.DisplayName);
            Assert.AreEqual(1, item.Children.Count);
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemFactory_CreateExplorerItem")]
        public void ExplorerItemFactory_CreateExplorerItemWithType_SecondtGen_ExpectSecondGenChildren()
        {

            //------------Setup for test--------------------------
            var auth = new Mock<IAuthorizationService>();
            var catalogue = new Mock<IResourceCatalog>();
            var directory = new Mock<IDirectory>();
            var resources = MoqUtil.GenerateMockEnumerable<IResource>(4).ToList();
            for(int i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];
                resource.Setup(a => a.ResourcePath).Returns("1\\" + i);
                resource.Setup(a => a.ResourceName).Returns(i.ToString);
                resource.Setup(a => a.ResourceID).Returns(Guid.NewGuid);
                resource.Setup(a => a.ResourceType).Returns(ResourceType.EmailSource);
            }
            directory.Setup(a => a.GetDirectories(@"b:\bob")).Returns(new[] { @"b:\bob\1" });
            catalogue.Setup(a => a.GetResourceList(It.IsAny<Guid>())).Returns(MoqUtil.ProxiesFromMockEnumerable(resources).ToList());
            var explorerItemFactory = new ExplorerItemFactory(catalogue.Object, directory.Object, auth.Object);

            //------------Execute Test---------------------------
            var item = explorerItemFactory.CreateRootExplorerItem(ResourceType.EmailSource, @"b:\bob", Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.AreEqual(Environment.MachineName, item.DisplayName);
            Assert.AreEqual(1, item.Children.Count);
            Assert.AreEqual(4, item.Children[0].Children.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemFactory_CreateExplorerItem")]
        public void ExplorerItemFactory_CreateExplorerItemWithType_ReservedServicesNotReturned_SecondtGen_ExpectSecondGenChildren()
        {

            //------------Setup for test--------------------------
            var auth = new Mock<IAuthorizationService>();
            var catalogue = new Mock<IResourceCatalog>();
            var directory = new Mock<IDirectory>();
            var resources = MoqUtil.GenerateMockEnumerable<IResource>(4).ToList();
            for(int i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];
                resource.Setup(a => a.ResourcePath).Returns("1\\" + i);
                resource.Setup(a => a.ResourceName).Returns(i.ToString);
                resource.Setup(a => a.ResourceID).Returns(Guid.NewGuid);
                resource.Setup(a => a.ResourceType).Returns(ResourceType.EmailSource);
            }
            var mockReserverService = new Mock<IResource>();
            mockReserverService.Setup(a => a.ResourcePath).Returns("1");
            mockReserverService.Setup(a => a.ResourceName).Returns("TestReservedService");
            mockReserverService.Setup(a => a.ResourceID).Returns(Guid.NewGuid);
            mockReserverService.Setup(a => a.ResourceType).Returns(ResourceType.ReservedService);
            resources.Add(mockReserverService);
            directory.Setup(a => a.GetDirectories(@"b:\bob")).Returns(new[] { @"b:\bob\1" });
            catalogue.Setup(a => a.GetResourceList(It.IsAny<Guid>())).Returns(MoqUtil.ProxiesFromMockEnumerable(resources).ToList());
            var explorerItemFactory = new ExplorerItemFactory(catalogue.Object, directory.Object, auth.Object);

            //------------Execute Test---------------------------
            var item = explorerItemFactory.CreateRootExplorerItem(@"b:\bob", Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.AreEqual(1, item.Children.Count);
            Assert.AreEqual(4, item.Children[0].Children.Count);
            var found = item.Children[0].Children.FirstOrDefault(explorerItem => explorerItem.DisplayName == "TestReservedService");
            Assert.IsNull(found);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemFactory_CreateExplorerItem")]
        public void ExplorerItemFactory_CreateExplorerItemWithType_MultipleGenerations_ExpectSecondAndFirstGenChildren()
        {

            //------------Setup for test--------------------------
            var catalogue = new Mock<IResourceCatalog>();
            var directory = new Mock<IDirectory>();
            var auth = new Mock<IAuthorizationService>();
            var resources = MoqUtil.GenerateMockEnumerable<IResource>(5).ToList();
            for(int i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];
                resource.Setup(a => a.ResourcePath).Returns(i % 2 == 0 ? "" + i : "1\\" + i);
                resource.Setup(a => a.ResourceName).Returns(i.ToString);
                resource.Setup(a => a.ResourceID).Returns(Guid.NewGuid);
                resource.Setup(a => a.ResourceType).Returns(i % 2 == 0 ? ResourceType.EmailSource : ResourceType.DbSource);
            }
            resources[3].Setup(a => a.ResourceType).Returns(ResourceType.EmailSource);
            directory.Setup(a => a.GetDirectories(@"b:\bob")).Returns(new[] { @"b:\bob\1" });
            catalogue.Setup(a => a.GetResourceList(It.IsAny<Guid>())).Returns(MoqUtil.ProxiesFromMockEnumerable(resources).ToList());
            var explorerItemFactory = new ExplorerItemFactory(catalogue.Object, directory.Object, auth.Object);

            //------------Execute Test---------------------------
            var item = explorerItemFactory.CreateRootExplorerItem(ResourceType.EmailSource, @"b:\bob", Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.AreEqual(Environment.MachineName, item.DisplayName);
            Assert.AreEqual(4, item.Children.Count);
            Assert.AreEqual(1, item.Children[0].Children.Count);
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemFactory_CreateExplorerItem")]
        public void ExplorerItemFactory_CreateExplorerItemWithType_Folders_ExpectTwoChildren()
        {

            //------------Setup for test--------------------------
            var catalogue = new Mock<IResourceCatalog>();
            var directory = new Mock<IDirectory>();
            var resources = MoqUtil.GenerateMockEnumerable<IResource>(4).ToList();
            var auth = new Mock<IAuthorizationService>();
            auth.Setup(c => c.GetResourcePermissions(It.IsAny<Guid>())).Returns(Permissions.Contribute);
            for(int i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];
                resource.Setup(a => a.ResourcePath).Returns("1");
                resource.Setup(a => a.ResourceName).Returns(i.ToString);
                resource.Setup(a => a.ResourceID).Returns(Guid.NewGuid);
                resource.Setup(a => a.ResourceType).Returns(ResourceType.EmailSource);
            }
            directory.Setup(a => a.GetDirectories(@"b:\bob")).Returns(new[] { @"b:\bob\1", @"b:\bob\2" });
            catalogue.Setup(a => a.GetResourceList(It.IsAny<Guid>())).Returns(MoqUtil.ProxiesFromMockEnumerable(resources).ToList());
            var explorerItemFactory = new ExplorerItemFactory(catalogue.Object, directory.Object, auth.Object);

            //------------Execute Test---------------------------
            var item = explorerItemFactory.CreateRootExplorerItem(ResourceType.Folder, @"b:\bob", Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.AreEqual(Environment.MachineName, item.DisplayName);
            Assert.AreEqual(2, item.Children.Count);
            Assert.IsTrue(item.Children.All(a => a.Permissions == Permissions.Contribute));
            Assert.IsTrue(item.Permissions == Permissions.Contribute);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemFactory_CreateExplorerItem")]
        public void ExplorerItemFactory_CreateExplorerItemWithType_Authorisation_ExpectFirstGenChildrenWithPermissions()
        {

            //------------Setup for test--------------------------
            var catalogue = new Mock<IResourceCatalog>();
            var directory = new Mock<IDirectory>();
            var auth = new Mock<IAuthorizationService>();
            var resources = MoqUtil.GenerateMockEnumerable<IResource>(4).ToList();
            var guid = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            for(int i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];
                resource.Setup(a => a.ResourcePath).Returns("" + i);
                resource.Setup(a => a.ResourceName).Returns(i.ToString);
                resource.Setup(a => a.ResourceID).Returns(guid[i]);

            }
            resources[0].Setup(a => a.ResourceType).Returns(ResourceType.EmailSource);
            auth.Setup(a => a.GetResourcePermissions(guid[0])).Returns(Permissions.Contribute);
            auth.Setup(a => a.GetResourcePermissions(guid[1])).Returns(Permissions.Administrator);
            auth.Setup(a => a.GetResourcePermissions(guid[2])).Returns(Permissions.DeployFrom);
            auth.Setup(a => a.GetResourcePermissions(guid[3])).Returns(Permissions.DeployTo);
            catalogue.Setup(a => a.GetResourceList(It.IsAny<Guid>())).Returns(MoqUtil.ProxiesFromMockEnumerable(resources).ToList());
            var explorerItemFactory = new ExplorerItemFactory(catalogue.Object, directory.Object, auth.Object);

            //------------Execute Test---------------------------
            var item = explorerItemFactory.CreateRootExplorerItem(@"b:\bob", Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.AreEqual(Environment.MachineName, item.DisplayName);
            Assert.AreEqual(4, item.Children.Count);
            Assert.AreEqual(item.Children[0].Permissions, Permissions.Contribute);
            Assert.AreEqual(item.Children[1].Permissions, Permissions.Administrator);
            Assert.AreEqual(item.Children[2].Permissions, Permissions.DeployFrom);
            Assert.AreEqual(item.Children[3].Permissions, Permissions.DeployTo);
        }


    }
}
