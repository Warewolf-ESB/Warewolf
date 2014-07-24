using System;
using System.Collections.Generic;
using Dev2.Data.Storage;
using Dev2.Data.Storage.ProtocolBuffers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Data.Tests.Persistence
{
    [TestClass]
    public class BinaryDataListRowEqualityComparerTest
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BinaryDataListRowEqualityComparer_Constructor_NullListOfColumns_ExceptionThrown()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
// ReSharper disable ObjectCreationAsStatement
            new BinaryDataListRowEqualityComparer(null);
// ReSharper restore ObjectCreationAsStatement
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_GetHashCode")]
        public void BinaryDataListRowEqualityComparer_GetHashCode_AlwaysReturnsZero()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 1 });

            //------------Execute Test---------------------------
            var hashCode = binaryDataListRowEqualityComparer.GetHashCode(new IndexBasedBinaryDataListRow());
            //------------Assert Results-------------------------
            Assert.AreEqual(0, hashCode);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Equals")]
        public void BinaryDataListRowEqualityComparer_Equals_StringValues_EqualReturnsTrue()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 0 });

            var xValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var yValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var binaryDataListRow1 = new BinaryDataListRow(1);
            var binaryDataListRow2 = new BinaryDataListRow(1);
            binaryDataListRow1.UpdateValue("r1.f1.value", 0, 0);
            binaryDataListRow2.UpdateValue("r1.f1.value", 0, 0);
            xValueRow.Row = binaryDataListRow1;
            yValueRow.Row = binaryDataListRow2;
            //------------Execute Test---------------------------
            var isEqual = binaryDataListRowEqualityComparer.Equals(xValueRow, yValueRow);
            //------------Assert Results-------------------------
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Equals")]
        public void BinaryDataListRowEqualityComparer_Equals_StringValues_NotEqualReturnsFalse()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 0 });

            var xValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var yValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var binaryDataListRow1 = new BinaryDataListRow(1);
            var binaryDataListRow2 = new BinaryDataListRow(1);
            binaryDataListRow1.UpdateValue("r1.f1.value", 0, 0);
            binaryDataListRow2.UpdateValue("r1.f2.value", 0, 0);
            xValueRow.Row = binaryDataListRow1;
            yValueRow.Row = binaryDataListRow2;
            //------------Execute Test---------------------------
            var isEqual = binaryDataListRowEqualityComparer.Equals(xValueRow, yValueRow);
            //------------Assert Results-------------------------
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Equals")]
        public void BinaryDataListRowEqualityComparer_Equals_IntValues_EqualReturnsTrue()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 0 });

            var xValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var yValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var binaryDataListRow1 = new BinaryDataListRow(1);
            var binaryDataListRow2 = new BinaryDataListRow(1);
            binaryDataListRow1.UpdateValue("100", 0, 0);
            binaryDataListRow2.UpdateValue("100", 0, 0);
            xValueRow.Row = binaryDataListRow1;
            yValueRow.Row = binaryDataListRow2;
            //------------Execute Test---------------------------
            var isEqual = binaryDataListRowEqualityComparer.Equals(xValueRow, yValueRow);
            //------------Assert Results-------------------------
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Equals")]
        public void BinaryDataListRowEqualityComparer_Equals_BothNotIntValues_EqualReturnsFalse()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 0 });

            var xValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var yValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var binaryDataListRow1 = new BinaryDataListRow(1);
            var binaryDataListRow2 = new BinaryDataListRow(1);
            binaryDataListRow1.UpdateValue("100", 0, 0);
            binaryDataListRow2.UpdateValue("test", 0, 0);
            xValueRow.Row = binaryDataListRow1;
            yValueRow.Row = binaryDataListRow2;
            //------------Execute Test---------------------------
            var isEqual = binaryDataListRowEqualityComparer.Equals(xValueRow, yValueRow);
            //------------Assert Results-------------------------
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Equals")]
        public void BinaryDataListRowEqualityComparer_Equals_IntValues_NotEqualReturnsFalse()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 0 });

            var xValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var yValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var binaryDataListRow1 = new BinaryDataListRow(1);
            var binaryDataListRow2 = new BinaryDataListRow(1);
            binaryDataListRow1.UpdateValue("10", 0, 0);
            binaryDataListRow2.UpdateValue("1", 0, 0);
            xValueRow.Row = binaryDataListRow1;
            yValueRow.Row = binaryDataListRow2;
            //------------Execute Test---------------------------
            var isEqual = binaryDataListRowEqualityComparer.Equals(xValueRow, yValueRow);
            //------------Assert Results-------------------------
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Equals")]
        public void BinaryDataListRowEqualityComparer_Equals_LongValues_EqualReturnsTrue()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 0 });

            var xValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var yValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var binaryDataListRow1 = new BinaryDataListRow(1);
            var binaryDataListRow2 = new BinaryDataListRow(1);
            binaryDataListRow1.UpdateValue("9223372036854775707", 0, 0);
            binaryDataListRow2.UpdateValue("9223372036854775707", 0, 0);
            xValueRow.Row = binaryDataListRow1;
            yValueRow.Row = binaryDataListRow2;
            //------------Execute Test---------------------------
            var isEqual = binaryDataListRowEqualityComparer.Equals(xValueRow, yValueRow);
            //------------Assert Results-------------------------
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Equals")]
        public void BinaryDataListRowEqualityComparer_Equals_LongValues_NotEqualReturnsFalse()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 0 });

            var xValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var yValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var binaryDataListRow1 = new BinaryDataListRow(1);
            var binaryDataListRow2 = new BinaryDataListRow(1);
            binaryDataListRow1.UpdateValue("9223372036854775707", 0, 0);
            binaryDataListRow2.UpdateValue("9223372036854775708", 0, 0);
            xValueRow.Row = binaryDataListRow1;
            yValueRow.Row = binaryDataListRow2;
            //------------Execute Test---------------------------
            var isEqual = binaryDataListRowEqualityComparer.Equals(xValueRow, yValueRow);
            //------------Assert Results-------------------------
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Equals")]
        public void BinaryDataListRowEqualityComparer_Equals_BothLongValues_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 0 });

            var xValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var yValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var binaryDataListRow1 = new BinaryDataListRow(1);
            var binaryDataListRow2 = new BinaryDataListRow(1);
            binaryDataListRow1.UpdateValue("9223372036854775707", 0, 0);
            binaryDataListRow2.UpdateValue("val", 0, 0);
            xValueRow.Row = binaryDataListRow1;
            yValueRow.Row = binaryDataListRow2;
            //------------Execute Test---------------------------
            var isEqual = binaryDataListRowEqualityComparer.Equals(xValueRow, yValueRow);
            //------------Assert Results-------------------------
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Equals")]
        public void BinaryDataListRowEqualityComparer_Equals_FloatValues_EqualReturnsTrue()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 0 });

            var xValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var yValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var binaryDataListRow1 = new BinaryDataListRow(1);
            var binaryDataListRow2 = new BinaryDataListRow(1);
            binaryDataListRow1.UpdateValue("100.01", 0, 0);
            binaryDataListRow2.UpdateValue("100.01", 0, 0);
            xValueRow.Row = binaryDataListRow1;
            yValueRow.Row = binaryDataListRow2;
            //------------Execute Test---------------------------
            var isEqual = binaryDataListRowEqualityComparer.Equals(xValueRow, yValueRow);
            //------------Assert Results-------------------------
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Equals")]
        public void BinaryDataListRowEqualityComparer_Equals_FloatValues_NotEqualReturnsFalse()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 0 });

            var xValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var yValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var binaryDataListRow1 = new BinaryDataListRow(1);
            var binaryDataListRow2 = new BinaryDataListRow(1);
            binaryDataListRow1.UpdateValue("100.01", 0, 0);
            binaryDataListRow2.UpdateValue("100.02", 0, 0);
            xValueRow.Row = binaryDataListRow1;
            yValueRow.Row = binaryDataListRow2;
            //------------Execute Test---------------------------
            var isEqual = binaryDataListRowEqualityComparer.Equals(xValueRow, yValueRow);
            //------------Assert Results-------------------------
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Equals")]
        public void BinaryDataListRowEqualityComparer_Equals_BothNotFloatValuesReturnsFalse()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 0 });

            var xValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var yValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var binaryDataListRow1 = new BinaryDataListRow(1);
            var binaryDataListRow2 = new BinaryDataListRow(1);
            binaryDataListRow1.UpdateValue("100.01", 0, 0);
            binaryDataListRow2.UpdateValue("some val", 0, 0);
            xValueRow.Row = binaryDataListRow1;
            yValueRow.Row = binaryDataListRow2;
            //------------Execute Test---------------------------
            var isEqual = binaryDataListRowEqualityComparer.Equals(xValueRow, yValueRow);
            //------------Assert Results-------------------------
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Equals")]
        public void BinaryDataListRowEqualityComparer_Equals_DateTimeValues_EqualReturnsTrue()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 0 });

            var xValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var yValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var binaryDataListRow1 = new BinaryDataListRow(1);
            var binaryDataListRow2 = new BinaryDataListRow(1);
            binaryDataListRow1.UpdateValue("2014/07/21", 0, 0);
            binaryDataListRow2.UpdateValue("2014/07/21", 0, 0);
            xValueRow.Row = binaryDataListRow1;
            yValueRow.Row = binaryDataListRow2;
            //------------Execute Test---------------------------
            var isEqual = binaryDataListRowEqualityComparer.Equals(xValueRow, yValueRow);
            //------------Assert Results-------------------------
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Equals")]
        public void BinaryDataListRowEqualityComparer_Equals_DateTimeValues_NotEqualReturnsFalse()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 0 });

            var xValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var yValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var binaryDataListRow1 = new BinaryDataListRow(1);
            var binaryDataListRow2 = new BinaryDataListRow(1);
            binaryDataListRow1.UpdateValue("2014/07/21", 0, 0);
            binaryDataListRow2.UpdateValue("2014/07/26", 0, 0);
            xValueRow.Row = binaryDataListRow1;
            yValueRow.Row = binaryDataListRow2;
            //------------Execute Test---------------------------
            var isEqual = binaryDataListRowEqualityComparer.Equals(xValueRow, yValueRow);
            //------------Assert Results-------------------------
            Assert.IsFalse(isEqual);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Equals")]
        public void BinaryDataListRowEqualityComparer_Equals_BothNotDateTimeValues_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 0 });

            var xValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var yValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var binaryDataListRow1 = new BinaryDataListRow(1);
            var binaryDataListRow2 = new BinaryDataListRow(1);
            binaryDataListRow1.UpdateValue("2014/07/21", 0, 0);
            binaryDataListRow2.UpdateValue("another value", 0, 0);
            xValueRow.Row = binaryDataListRow1;
            yValueRow.Row = binaryDataListRow2;
            //------------Execute Test---------------------------
            var isEqual = binaryDataListRowEqualityComparer.Equals(xValueRow, yValueRow);
            //------------Assert Results-------------------------
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Equals")]
        public void BinaryDataListRowEqualityComparer_Equals_MultipleColumns_EqualReturnsTrue()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 0, 1 });

            var xValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var yValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var binaryDataListRow1 = new BinaryDataListRow(2);
            var binaryDataListRow2 = new BinaryDataListRow(2);
            binaryDataListRow1.UpdateValue("r1.f1.value", 0, 0);
            binaryDataListRow1.UpdateValue("test", 1, 0);
            binaryDataListRow2.UpdateValue("r1.f1.value", 0, 0);
            binaryDataListRow2.UpdateValue("test", 1, 0);
            xValueRow.Row = binaryDataListRow1;
            yValueRow.Row = binaryDataListRow2;
            //------------Execute Test---------------------------
            var isEqual = binaryDataListRowEqualityComparer.Equals(xValueRow, yValueRow);
            //------------Assert Results-------------------------
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataListRowEqualityComparer_Equals")]
        public void BinaryDataListRowEqualityComparer_Equals_MultipleColumns_NotMatchColumn_EqualReturnsFalse()
        {
            //------------Setup for test--------------------------
            var binaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(new List<int> { 0, 1 });

            var xValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var yValueRow = new IndexBasedBinaryDataListRow { Index = 1 };
            var binaryDataListRow1 = new BinaryDataListRow(2);
            var binaryDataListRow2 = new BinaryDataListRow(2);
            binaryDataListRow1.UpdateValue("r1.f1.value", 0, 0);
            binaryDataListRow1.UpdateValue("test", 1, 0);
            binaryDataListRow2.UpdateValue("r1.f1.value", 0, 0);
            binaryDataListRow2.UpdateValue("test1", 1, 0);
            xValueRow.Row = binaryDataListRow1;
            yValueRow.Row = binaryDataListRow2;
            //------------Execute Test---------------------------
            var isEqual = binaryDataListRowEqualityComparer.Equals(xValueRow, yValueRow);
            //------------Assert Results-------------------------
            Assert.IsFalse(isEqual);
        }
    }
}
