using System;
using System.Activities;
using System.Collections.Generic;
using Dev2;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace Warewolf.ResourceManagement.Tests
{
    [TestClass]
    public class ResourceActivityCacheTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullParser_ShouldThrowException()
        {
            var resourceActivityCache = new ResourceActivityCache(null, new System.Collections.Concurrent.ConcurrentDictionary<Guid, Dev2.IDev2Activity>());
            Assert.IsNull(resourceActivityCache);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullCache_ShouldThrowException()
        {
            var resourceActivityCache = new ResourceActivityCache(new ActivityParser(), null);
            Assert.IsNull(resourceActivityCache);
        }

        [TestMethod]
        public void Parse_WhenNotInCache_ShouldAdd()
        {
            var parser = new Mock<IActivityParser>();
            parser.Setup(a => a.Parse(It.IsAny<DynamicActivity>())).Returns(new Mock<IDev2Activity>().Object);
            var resourceActivityCache = new ResourceActivityCache(parser.Object, new System.Collections.Concurrent.ConcurrentDictionary<Guid, Dev2.IDev2Activity>());
            var resourceId = Guid.NewGuid();
            resourceActivityCache.Parse(new DynamicActivity(), resourceId);

            Assert.IsNotNull(resourceActivityCache);
            Assert.AreEqual(1, resourceActivityCache.Cache.Count);
            Assert.IsTrue(resourceActivityCache.Cache.ContainsKey(resourceId));
        }

        [TestMethod]
        public void HasInCache_WhenInCache_ShouldReturnTrue()
        {
            var parser = new Mock<IActivityParser>();
            parser.Setup(a => a.Parse(It.IsAny<DynamicActivity>())).Returns(new Mock<IDev2Activity>().Object);
            var resourceActivityCache = new ResourceActivityCache(parser.Object, new System.Collections.Concurrent.ConcurrentDictionary<Guid, Dev2.IDev2Activity>());
            var resourceId = Guid.NewGuid();
            resourceActivityCache.Parse(new DynamicActivity(), resourceId);

            Assert.IsTrue(resourceActivityCache.HasActivityInCache(resourceId));            
        }

        [TestMethod]
        public void HasInCache_WhenNotInCache_ShouldReturnFalse()
        {
            var parser = new Mock<IActivityParser>();
            parser.Setup(a => a.Parse(It.IsAny<DynamicActivity>())).Returns(new Mock<IDev2Activity>().Object);
            var resourceActivityCache = new ResourceActivityCache(parser.Object, new System.Collections.Concurrent.ConcurrentDictionary<Guid, Dev2.IDev2Activity>());
            var resourceId = Guid.NewGuid();
            resourceActivityCache.Parse(new DynamicActivity(), resourceId);

            Assert.IsFalse(resourceActivityCache.HasActivityInCache(Guid.NewGuid()));
        }

        [TestMethod]
        public void Parse_WhenInCache_ShouldNotAdd()
        {
            var parser = new Mock<IActivityParser>();
            parser.Setup(a => a.Parse(It.IsAny<DynamicActivity>())).Returns(new Mock<IDev2Activity>().Object);
            var resourceActivityCache = new ResourceActivityCache(parser.Object, new System.Collections.Concurrent.ConcurrentDictionary<Guid, Dev2.IDev2Activity>());
            var resourceId = Guid.NewGuid();
            resourceActivityCache.Parse(new DynamicActivity(), resourceId);

            Assert.IsNotNull(resourceActivityCache);
            Assert.AreEqual(1, resourceActivityCache.Cache.Count);
            Assert.IsTrue(resourceActivityCache.Cache.ContainsKey(resourceId));

            resourceActivityCache.Parse(new DynamicActivity(), resourceId);
            Assert.AreEqual(1, resourceActivityCache.Cache.Count);
            Assert.IsTrue(resourceActivityCache.Cache.ContainsKey(resourceId));
        }

        [TestMethod]
        public void Remove_WhenInCache_ShouldRemove()
        {
            var parser = new Mock<IActivityParser>();
            parser.Setup(a => a.Parse(It.IsAny<DynamicActivity>())).Returns(new Mock<IDev2Activity>().Object);
            var resourceActivityCache = new ResourceActivityCache(parser.Object, new System.Collections.Concurrent.ConcurrentDictionary<Guid, Dev2.IDev2Activity>());
            var resourceId = Guid.NewGuid();
            resourceActivityCache.Parse(new DynamicActivity(), resourceId);

            Assert.AreEqual(1, resourceActivityCache.Cache.Count);
            Assert.IsTrue(resourceActivityCache.Cache.ContainsKey(resourceId));

            resourceActivityCache.RemoveFromCache(resourceId);
            Assert.AreEqual(0, resourceActivityCache.Cache.Count);
            Assert.IsFalse(resourceActivityCache.Cache.ContainsKey(resourceId));
        }

        // --------------------------------------------------------------
        // 8431- additional coverage for ResourceActivityCache
        // --------------------------------------------------------------

        static ResourceActivityCache NewCache(Mock<IActivityParser> parser = null)
        {
            return new ResourceActivityCache(
                (parser ?? new Mock<IActivityParser>()).Object,
                new System.Collections.Concurrent.ConcurrentDictionary<Guid, IDev2Activity>());
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        public void Parse_NullActivity_ReturnsNull()
        {
            var sut = NewCache();

            var result = sut.Parse(null, Guid.NewGuid());

            Assert.IsNull(result);
            Assert.AreEqual(0, sut.Cache.Count);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        public void Parse_WhenInCache_ReturnsCachedActivity()
        {
            var cached = new Mock<IDev2Activity>().Object;
            var parser = new Mock<IActivityParser>();
            parser.Setup(p => p.Parse(It.IsAny<DynamicActivity>())).Returns(new Mock<IDev2Activity>().Object);
            var sut = new ResourceActivityCache(parser.Object,
                new System.Collections.Concurrent.ConcurrentDictionary<Guid, IDev2Activity>());
            var id = Guid.NewGuid();
            sut.Cache.TryAdd(id, cached);

            var result = sut.Parse(new DynamicActivity(), id);

            Assert.AreSame(cached, result);
            parser.Verify(p => p.Parse(It.IsAny<DynamicActivity>()), Times.Never);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void Parse_FailOnErrorTrue_InvalidWorkflow_Rethrows()
        {
            var parser = new Mock<IActivityParser>();
            parser.Setup(p => p.Parse(It.IsAny<DynamicActivity>())).Throws(new InvalidWorkflowException("bad"));
            var sut = NewCache(parser);

            sut.Parse(new DynamicActivity(), Guid.NewGuid(), true);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Parse_FailOnErrorTrue_GeneralException_Rethrows()
        {
            var parser = new Mock<IActivityParser>();
            parser.Setup(p => p.Parse(It.IsAny<DynamicActivity>())).Throws(new InvalidOperationException("boom"));
            var sut = NewCache(parser);

            sut.Parse(new DynamicActivity(), Guid.NewGuid(), true);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        public void Parse_FailOnErrorFalse_InvalidWorkflow_ReturnsNull()
        {
            var parser = new Mock<IActivityParser>();
            parser.Setup(p => p.Parse(It.IsAny<DynamicActivity>())).Throws(new InvalidWorkflowException("bad"));
            var sut = NewCache(parser);

            var result = sut.Parse(new DynamicActivity(), Guid.NewGuid(), false);

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        public void Parse_FailOnErrorFalse_GeneralException_ReturnsNull()
        {
            var parser = new Mock<IActivityParser>();
            parser.Setup(p => p.Parse(It.IsAny<DynamicActivity>())).Throws(new InvalidOperationException("boom"));
            var sut = NewCache(parser);

            var result = sut.Parse(new DynamicActivity(), Guid.NewGuid(), false);

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        public void ParseWithoutCache_Success_ReturnsParsedNoCacheMutation()
        {
            var parsed = new Mock<IDev2Activity>().Object;
            var parser = new Mock<IActivityParser>();
            parser.Setup(p => p.Parse(It.IsAny<DynamicActivity>())).Returns(parsed);
            var sut = NewCache(parser);

            var result = sut.ParseWithoutCache(new DynamicActivity(), Guid.NewGuid(), false);

            Assert.AreSame(parsed, result);
            Assert.AreEqual(0, sut.Cache.Count);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        public void ParseWithoutCache_NullActivity_ReturnsNull()
        {
            var sut = NewCache();

            var result = sut.ParseWithoutCache(null, Guid.NewGuid(), true);

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void ParseWithoutCache_FailOnErrorTrue_InvalidWorkflow_Rethrows()
        {
            var parser = new Mock<IActivityParser>();
            parser.Setup(p => p.Parse(It.IsAny<DynamicActivity>())).Throws(new InvalidWorkflowException("bad"));
            var sut = NewCache(parser);

            sut.ParseWithoutCache(new DynamicActivity(), Guid.NewGuid(), true);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ParseWithoutCache_FailOnErrorTrue_GeneralException_Rethrows()
        {
            var parser = new Mock<IActivityParser>();
            parser.Setup(p => p.Parse(It.IsAny<DynamicActivity>())).Throws(new InvalidOperationException("boom"));
            var sut = NewCache(parser);

            sut.ParseWithoutCache(new DynamicActivity(), Guid.NewGuid(), true);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        public void ParseWithoutCache_FailOnErrorFalse_Swallows_ReturnsNull()
        {
            var parser = new Mock<IActivityParser>();
            parser.Setup(p => p.Parse(It.IsAny<DynamicActivity>())).Throws(new InvalidWorkflowException("bad"));
            var sut = NewCache(parser);

            Assert.IsNull(sut.ParseWithoutCache(new DynamicActivity(), Guid.NewGuid(), false));

            parser.Setup(p => p.Parse(It.IsAny<DynamicActivity>())).Throws(new InvalidOperationException("boom"));
            Assert.IsNull(sut.ParseWithoutCache(new DynamicActivity(), Guid.NewGuid(), false));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        public void ParseWithCache_Success_DelegatesToParser()
        {
            var parsed = new Mock<IDev2Activity>().Object;
            var workspaceId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var parser = new Mock<IActivityParser>();
            parser.Setup(p => p.ParseWithCache(It.IsAny<DynamicActivity>(), workspaceId, resourceId)).Returns(parsed);
            var sut = NewCache(parser);
            var parameters = new ResourceActivityParseWithCacheParameters(new DynamicActivity(), workspaceId, resourceId, false);

            var result = sut.ParseWithCache(parameters);

            Assert.AreSame(parsed, result);
            parser.Verify(p => p.ParseWithCache(It.IsAny<DynamicActivity>(), workspaceId, resourceId), Times.Once);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        public void ParseWithCache_NullActivity_ReturnsNull()
        {
            var sut = NewCache();
            var parameters = new ResourceActivityParseWithCacheParameters(null, Guid.NewGuid(), Guid.NewGuid(), true);

            var result = sut.ParseWithCache(parameters);

            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        [ExpectedException(typeof(InvalidWorkflowException))]
        public void ParseWithCache_FailOnErrorTrue_InvalidWorkflow_Rethrows()
        {
            var parser = new Mock<IActivityParser>();
            parser.Setup(p => p.ParseWithCache(It.IsAny<DynamicActivity>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                  .Throws(new InvalidWorkflowException("bad"));
            var sut = NewCache(parser);
            var parameters = new ResourceActivityParseWithCacheParameters(new DynamicActivity(), Guid.NewGuid(), Guid.NewGuid(), true);

            sut.ParseWithCache(parameters);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ParseWithCache_FailOnErrorTrue_GeneralException_Rethrows()
        {
            var parser = new Mock<IActivityParser>();
            parser.Setup(p => p.ParseWithCache(It.IsAny<DynamicActivity>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                  .Throws(new InvalidOperationException("boom"));
            var sut = NewCache(parser);
            var parameters = new ResourceActivityParseWithCacheParameters(new DynamicActivity(), Guid.NewGuid(), Guid.NewGuid(), true);

            sut.ParseWithCache(parameters);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        public void ParseWithCache_FailOnErrorFalse_Swallows_ReturnsNull()
        {
            var parser = new Mock<IActivityParser>();
            parser.Setup(p => p.ParseWithCache(It.IsAny<DynamicActivity>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                  .Throws(new InvalidWorkflowException("bad"));
            var sut = NewCache(parser);

            Assert.IsNull(sut.ParseWithCache(
                new ResourceActivityParseWithCacheParameters(new DynamicActivity(), Guid.NewGuid(), Guid.NewGuid(), false)));

            parser.Setup(p => p.ParseWithCache(It.IsAny<DynamicActivity>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                  .Throws(new InvalidOperationException("boom"));
            Assert.IsNull(sut.ParseWithCache(
                new ResourceActivityParseWithCacheParameters(new DynamicActivity(), Guid.NewGuid(), Guid.NewGuid(), false)));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        public void GetActivity_ReturnsCachedInstance()
        {
            var cached = new Mock<IDev2Activity>().Object;
            var sut = NewCache();
            var id = Guid.NewGuid();
            sut.Cache.TryAdd(id, cached);

            Assert.AreSame(cached, sut.GetActivity(id));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        public void GetActivityFromCache_DelegatesToParser()
        {
            var parsed = new Mock<IDev2Activity>().Object;
            var workspaceId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var parser = new Mock<IActivityParser>();
            parser.Setup(p => p.GetActivityFromCache(workspaceId, resourceId)).Returns(parsed);
            var sut = NewCache(parser);

            var result = sut.GetActivityFromCache(workspaceId, resourceId);

            Assert.AreSame(parsed, result);
            parser.Verify(p => p.GetActivityFromCache(workspaceId, resourceId), Times.Once);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        public void ClearSerializedActivityCache_DelegatesToParser()
        {
            var parser = new Mock<IActivityParser>();
            var sut = NewCache(parser);

            sut.ClearSerializedActivityCache();

            parser.Verify(p => p.ClearSerializedActivityCache(), Times.Once);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        public void RemoveFromSerializedActivityCache_DelegatesToParser_ReturnsResult()
        {
            var workspaceId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var parser = new Mock<IActivityParser>();
            parser.Setup(p => p.RemoveFromSerializedActivityCache(workspaceId, resourceId)).Returns(true);
            var sut = NewCache(parser);

            Assert.IsTrue(sut.RemoveFromSerializedActivityCache(workspaceId, resourceId));
            parser.Verify(p => p.RemoveFromSerializedActivityCache(workspaceId, resourceId), Times.Once);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ResourceActivityCache))]
        public void Parse_HasNullEntryInCache_FallsBackToAddOrUpdate()
        {
            // HasActivityInCache returns true but the stored value is null, so GetActivity returns
            // null and Parse falls through; TryAdd then fails (key already present) and the
            // AddOrUpdate branch executes, replacing the null with the freshly parsed activity.
            var parsed = new Mock<IDev2Activity>().Object;
            var parser = new Mock<IActivityParser>();
            parser.Setup(p => p.Parse(It.IsAny<DynamicActivity>())).Returns(parsed);
            var cache = new System.Collections.Concurrent.ConcurrentDictionary<Guid, IDev2Activity>();
            var id = Guid.NewGuid();
            cache[id] = null;
            var sut = new ResourceActivityCache(parser.Object, cache);

            var result = sut.Parse(new DynamicActivity(), id);

            Assert.AreSame(parsed, result);
            Assert.AreSame(parsed, sut.Cache[id]);
        }
    }
}
