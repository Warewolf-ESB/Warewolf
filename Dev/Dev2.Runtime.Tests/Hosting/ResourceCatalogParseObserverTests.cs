/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Text;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Runtime.Hosting;
using Dev2.Tests.Runtime.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.ResourceManagement;

namespace Dev2.Tests.Runtime.Hosting
{
    /// <summary>
    /// WF removal T1.7: <see cref="ResourceCatalog.Parse(Guid, Guid)"/> tells a registered
    /// <see cref="IWorkflowParseObserver"/> about fresh parses only, and an observer can never break a parse.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class ResourceCatalogParseObserverTests
    {
        [TestCleanup]
        public void Cleanup() => CustomContainer.DeRegister<IWorkflowParseObserver>();

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ResourceCatalog))]
        public void ResourceCatalog_NotifyParsed_PassesResourceXamlAndResult()
        {
            var observer = new Mock<IWorkflowParseObserver>();
            var resourceId = Guid.NewGuid();
            var parsed = new Mock<IDev2Activity>().Object;

            ResourceCatalog.NotifyParsed(observer.Object, resourceId, "<Activity />", parsed);

            observer.Verify(o => o.Parsed(resourceId, It.Is<StringBuilder>(x => x.ToString() == "<Activity />"), parsed), Times.Once);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ResourceCatalog))]
        public void ResourceCatalog_NotifyParsed_ObserverThrows_IsSwallowed()
        {
            var observer = new Mock<IWorkflowParseObserver>();
            observer.Setup(o => o.Parsed(It.IsAny<Guid>(), It.IsAny<StringBuilder>(), It.IsAny<IDev2Activity>()))
                .Throws(new InvalidOperationException("observer down"));

            ResourceCatalog.NotifyParsed(observer.Object, Guid.NewGuid(), "<Activity />", new Mock<IDev2Activity>().Object);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ResourceCatalog))]
        public void ResourceCatalog_NotifyParsed_NoObserverOrNoResult_DoesNothing()
        {
            ResourceCatalog.NotifyParsed(null, Guid.NewGuid(), "<Activity />", new Mock<IDev2Activity>().Object);
            var observer = new Mock<IWorkflowParseObserver>();
            ResourceCatalog.NotifyParsed(observer.Object, Guid.NewGuid(), "<Activity />", null);
            ResourceCatalog.NotifyParsed(observer.Object, Guid.NewGuid(), null, new Mock<IDev2Activity>().Object);

            observer.Verify(o => o.Parsed(It.IsAny<Guid>(), It.IsAny<StringBuilder>(), It.IsAny<IDev2Activity>()), Times.Never);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ResourceCatalog))]
        public void ResourceCatalog_Parse_CacheHit_DoesNotNotifyTheObserver()
        {
            var observer = new Mock<IWorkflowParseObserver>();
            CustomContainer.Register(observer.Object);
            var resourceId = Guid.NewGuid();
            var cache = new Mock<IResourceActivityCache>();
            cache.Setup(o => o.HasActivityInCache(resourceId)).Returns(true);
            cache.Setup(o => o.GetActivity(resourceId)).Returns(new Mock<IDev2Activity>().Object);
            var sut = new ResourceCatalog { ResourceActivityCache = cache.Object };
            UseFreshParserCache();

            Assert.IsNotNull(sut.Parse(GlobalConstants.ServerWorkspaceID, resourceId));

            observer.Verify(o => o.Parsed(It.IsAny<Guid>(), It.IsAny<StringBuilder>(), It.IsAny<IDev2Activity>()), Times.Never);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ResourceCatalog))]
        [TestCategory("CannotParallelize")]
        public void ResourceCatalog_Parse_CacheMiss_NotifiesWithTheWorkflowXamlAndTheFreshGraph()
        {
            var workspaceId = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceId);
            var servicesPath = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(servicesPath);
            try
            {
                var resourceId = Guid.NewGuid();
                var xml = XmlResource.Fetch("Bug6619Dep");
                xml.Attribute("ID").Value = resourceId.ToString();
                File.WriteAllText(Path.Combine(servicesPath, "ParseObserverWorkflow.bite"), xml.ToString());

                var observer = new Mock<IWorkflowParseObserver>();
                CustomContainer.Register(observer.Object);
                var sut = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object)
                {
                    ResourceActivityCache = new ResourceActivityCache(new ActivityParser(), new ConcurrentDictionary<Guid, IDev2Activity>()),
                };
                // GetResources loads the workspace into the store that Parse's GetResource lookup reads.
                Assert.AreEqual(1, sut.GetResources(workspaceId).Count, "precondition: the workflow is in the workspace");
                UseFreshParserCache();

                var result = sut.Parse(workspaceId, resourceId);

                Assert.IsNotNull(result, "precondition: the workflow parses");
                observer.Verify(o => o.Parsed(resourceId, It.Is<StringBuilder>(x => x.ToString().Contains("<Activity")), result), Times.Once);
            }
            finally
            {
                try { Directory.Delete(workspacePath, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
        }

        // ResourceCatalog keeps its per-workspace parser caches in a static field; start each test from empty.
        static void UseFreshParserCache() =>
            typeof(ResourceCatalog).GetField("_parsers", BindingFlags.NonPublic | BindingFlags.Static)
                ?.SetValue(null, new ConcurrentDictionary<Guid, IResourceActivityCache>());
    }
}
