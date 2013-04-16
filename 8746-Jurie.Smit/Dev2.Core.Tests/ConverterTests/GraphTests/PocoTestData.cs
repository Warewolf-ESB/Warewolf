using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlimited.UnitTest.Framework.ConverterTests.GraphTests
{
    internal class PocoTestData
    {
        public string Name { get; set; }
        public int Age { get; set; }

        private string InternalData { get; set; }

        public PocoTestData NestedData { get; set; }

        public IList<PocoTestData> EnumerableData { get; set; }
        public IList<PocoTestData> EnumerableData1 { get; set; }
    }
}
