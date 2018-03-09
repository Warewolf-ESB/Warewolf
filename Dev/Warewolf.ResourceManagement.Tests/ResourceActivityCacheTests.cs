using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
