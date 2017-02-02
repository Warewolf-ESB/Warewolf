using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{    
    [TestClass]
    public class IsBinaryTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IsBinary_Invoke")]
        public void IsBinary_Invoke_DoesEndWith_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isBinary = new IsBinary();
            string[] cols = new string[2];
            cols[0] = "2";
            //------------Execute Test---------------------------
            bool result = isBinary.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            result = isBinary.Invoke(new []{string.Empty});
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IsBinary_Invoke")]
        public void IsBinary_Invoke_DoesntEndWith_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var isBinary = new IsBinary();
            string[] cols = new string[2];
            cols[0] = "1";
            //------------Execute Test---------------------------
            bool result = isBinary.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsBinary_HandlesType")]
        public void IsBinary_HandlesType_ReturnsIsBinaryType()
        {
            var expected = enDecisionType.IsBinary;
            //------------Setup for test--------------------------
            var isBinary = new IsBinary();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, isBinary.HandlesType());
        }
    }
}
