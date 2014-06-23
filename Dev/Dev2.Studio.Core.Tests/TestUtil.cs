using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Core.Tests
{
   public static class TestUtil
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
}
