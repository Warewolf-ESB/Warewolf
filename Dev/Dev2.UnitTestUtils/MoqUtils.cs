using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Moq;

namespace Dev2.UnitTestUtils
{
    public static class MoqUtils
    {
        public static IEnumerable<Mock<T>> GenerateMockEnumerable<T>(int count) where T : class
        {
            for (int i = 0; i < count; i++)
                yield return new Mock<T>();
        }

        public static IEnumerable<T> ProxiesFromMockEnumerable<T>(this IEnumerable<Mock<T>> values) where T : class
        {
            return values.Select(a => a.Object);
        }

       public static IEnumerable<Mock<T>> SetupExpectationsOnEnumerableWithReturnValues<T,TA>(this IEnumerable<Mock<T>> values,IEnumerable<TA> returns, Expression<Func<T,TA>> expectation ) where T : class
        {
            return values.Zip(returns, ((x, y) => new Tuple<Mock<T>, TA>(x, y))).Select(v =>
                {
                    v.Item1.Setup(expectation).Returns(v.Item2); return v.Item1;
                }).ToList();
            
        }
    }
}
