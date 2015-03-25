

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WarewolfParsingTest
{
    [TestClass]
    public class TestDataStructure
    {

     
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CreateDataSet_ExpectColumnsIncludePositionAndEmpty")]
        public void CreateDataSet_ExpectColumnsIncludePositionAndEmpty_Blank_ValidRecset()
        {
            //------------Setup for test--------------------------
            var createDataSet = PublicFunctions.CreateDataSet("a");

            Assert.IsTrue(createDataSet.Data.ContainsKey("WarewolfPositionColumn#"));
            Assert.IsTrue(createDataSet.Optimisations.IsOrdinal);
            Assert.AreEqual(createDataSet.LastIndex, 0);
            Assert.AreEqual(createDataSet.Count,0);
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CreateDataSet_ExpectColumnsIncludePositionAndEmpty")]
        public void CreateDataSet_EvalRecsetWithAnExpression_ExpectData()
        {
            //------------Setup for test--------------------------
            var createDataSet = WarewolfTestData.CreateTestEnvWithData;
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
    
    }
}
