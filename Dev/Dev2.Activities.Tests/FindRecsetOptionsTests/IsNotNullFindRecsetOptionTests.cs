using System.Collections.Generic;
using Dev2.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Tests.Activities.FindRecsetOptionsTests
{
    [TestClass]
    public class IsNotNullFindRecsetOptionTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("RsOpIsNotNull_CreateFunc")]
        public void RsOpIsNotNull_CreateFunc_WhenNull_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var rsOpIsNull = new RsOpIsNotNull();
            
            //------------Execute Test---------------------------
            var func = rsOpIsNull.CreateFunc(new List<DataStorage.WarewolfAtom>{DataStorage.WarewolfAtom.Nothing}, null,null, false);
            var isNull = func.Invoke(DataStorage.WarewolfAtom.Nothing);
            //------------Assert Results-------------------------
            Assert.IsFalse(isNull);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("RsOpIsNotNull_CreateFunc")]
        public void RsOpIsNotNull_CreateFunc_WhenNotNull_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var rsOpIsNull = new RsOpIsNotNull();

            //------------Execute Test---------------------------
            var func = rsOpIsNull.CreateFunc(new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.Nothing }, null, null, false);
            var isNull = func.Invoke(DataStorage.WarewolfAtom.NewDataString("bob"));
            //------------Assert Results-------------------------
            Assert.IsTrue(isNull);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("RsOpIsNotNull_HandlesType")]
        public void RsOpIsNotNull_HandlesType_ReturnsIsNotNULL()
        {
            //------------Setup for test--------------------------
            var rsOpIsNull = new RsOpIsNotNull();
            
            //------------Execute Test---------------------------
            var handlesType = rsOpIsNull.HandlesType();
            //------------Assert Results-------------------------
            Assert.AreEqual("Is Not NULL",handlesType);
        }
    }
}