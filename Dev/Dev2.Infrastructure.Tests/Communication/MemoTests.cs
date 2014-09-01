using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces.Communication;
using Dev2.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Communication
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MemoTests
    {
        [TestMethod]
        [Description("Constructor must initialize Date to now.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void MemoConstructor_UnitTest_Date_Now()
        // ReSharper restore InconsistentNaming
        {
            var memo = new Memo();
            var diff = DateTime.Now - memo.Date;
            var expected = diff < new TimeSpan(0, 0, 0, 5);
            Assert.IsTrue(expected,string.Format("The date should be close to date time now as it is set in the constructor to DateTime.Now. But got{0}", diff));
        }

        [TestMethod]
        [Description("DateString must return the Date property formatted as yyyy-MM-dd.HH.mm.ss.ffff.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
// ReSharper disable InconsistentNaming
        public void MemoDateString_UnitTest_Format_Correct()
// ReSharper restore InconsistentNaming
        {
            var memo = new Memo();
            Assert.AreEqual(memo.Date.ToString("yyyy-MM-dd.HH.mm.ss.ffff"), memo.DateString);
        }

        [TestMethod]
        [Description("Equals to null returns false.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
// ReSharper disable InconsistentNaming
        public void MemoEquals_UnitTest_Null_False()
// ReSharper restore InconsistentNaming
        {
            var memo = new Memo();
            var result = memo.Equals(null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Description("Equals to another type returns false.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
// ReSharper disable InconsistentNaming
        public void MemoEquals_UnitTest_OtherType_False()
// ReSharper restore InconsistentNaming
        {
            var memo = new Memo();
            var result = memo.Equals(new object());
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Description("Equals to same type but different ID returns false.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
// ReSharper disable InconsistentNaming
        public void MemoEquals_UnitTest_SameTypeAndDifferentID_False()
// ReSharper restore InconsistentNaming
        {
            var memo1 = new Memo { InstanceID = Guid.NewGuid() };
            var memo2 = new Memo { InstanceID = Guid.NewGuid() };
            var result = memo1.Equals(memo2);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Description("Equals to same type and same ID returns true.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
// ReSharper disable InconsistentNaming
        public void MemoEquals_UnitTest_SameTypeAndSameID_True()
// ReSharper restore InconsistentNaming
        {
            var memo1 = new Memo { InstanceID = Guid.NewGuid() };
            var memo2 = new Memo { InstanceID = memo1.InstanceID };
            var result = memo1.Equals(memo2);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Description("GetHashCode returns hash code of ID property.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
// ReSharper disable InconsistentNaming
        public void MemoGetHashCode_UnitTest_Result_HashCodeOfID()
// ReSharper restore InconsistentNaming
        {
            var memo = new Memo { InstanceID = Guid.NewGuid() };
            var expected = memo.InstanceID.GetHashCode();
            var actual = memo.GetHashCode();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("ToString must serialize a Memo inside an Envelope.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
// ReSharper disable InconsistentNaming
        public void MemoToString_UnitTest_Serialization_EnvelopeWithMemoAsContent()
// ReSharper restore InconsistentNaming
        {
            var memo = new Memo { InstanceID = Guid.NewGuid() };

            var serializer = new Dev2JsonSerializer();

            var actual = memo.ToString(serializer);
            Assert.IsNotNull(actual);

            var actualEnvelope = serializer.Deserialize<Envelope>(actual);
            Assert.IsNotNull(actualEnvelope);

            var actualMemo = serializer.Deserialize<Memo>(actualEnvelope.Content);
            Assert.IsNotNull(actualMemo);
            Assert.AreEqual(memo.InstanceID, actualMemo.InstanceID);
            Assert.AreEqual(memo.Date, actualMemo.Date);
        }


        [TestMethod]
        [Description("Parse must deserialize the envelope and then deserialize the contained memo.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
// ReSharper disable InconsistentNaming
        public void MemoParse_UnitTest_Serialization_MemoFromEnvelopeContent()
// ReSharper restore InconsistentNaming
        {
            var memo = new Memo { InstanceID = Guid.NewGuid() };

            var serializer = new Dev2JsonSerializer();

            var envelopeStr = memo.ToString(serializer);
            var actual = Memo.Parse(serializer, envelopeStr) as IMemo;

            Assert.IsNotNull(actual);
            Assert.AreEqual(memo.InstanceID, actual.InstanceID);
            Assert.AreEqual(memo.Date, actual.Date);
        }
    }
}
