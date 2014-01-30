using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.TOTests
{
    /// <summary>
    /// Summary description for DataMergeTOTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataMergeTOTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_Constructor")]
        public void DataMergeDTO_Constructor_FullConstructor_DefaultValues()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO(string.Empty, null, null, 1, null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Index", dataMergeDTO.MergeType);
            Assert.AreEqual(string.Empty, dataMergeDTO.At);
            Assert.AreEqual(1, dataMergeDTO.IndexNumber);
            Assert.AreEqual(string.Empty, dataMergeDTO.Padding);
            Assert.AreEqual("Left", dataMergeDTO.Alignment);
            Assert.IsNotNull(dataMergeDTO.Errors);
        }

        #region CanAdd Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_CanAdd")]
        public void DataMergeDTO_CanAdd_WithNewLineMergeTypeAndNoOtherValues_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO(string.Empty, "NewLine", null, 1, null, null);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataMergeDTO.CanAdd());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_CanAdd")]
        public void DataMergeDTO_CanAdd_WithNoInputVarButValueForAt_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO(string.Empty, null, "|", 1, null, null);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataMergeDTO.CanAdd());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_CanAdd")]
        public void DataMergeDTO_CanAdd_WithNoInputVarAndNoAt_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO(string.Empty, null, null, 1, null, null);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataMergeDTO.CanAdd());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_CanAdd")]
        public void DataMergeDTO_CanAdd_WithIndexMergeTypeAndNoOtherValues_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO(string.Empty, "Index", null, 1, null, null);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataMergeDTO.CanAdd());
        }

        #endregion

        #region CanRemove Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_CanRemove")]
        public void DataMergeDTO_CanRemove_WithNoInputVarButValueForAt_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO(string.Empty, null, "|", 1, null, null);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataMergeDTO.CanRemove());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_CanRemove")]
        public void DataMergeDTO_CanRemove_WithNoInputVarAndNoAt_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO(string.Empty, null, null, 1, null, null);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataMergeDTO.CanRemove());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_CanRemove")]
        public void DataMergeDTO_CanRemove_WithNewLineTypeAndNoInputVar_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO(string.Empty, "NewLine", null, 1, null, null);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataMergeDTO.CanRemove());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDTO_CanRemove")]
        public void DataMergeDTO_CanRemove_WithNewLineInputTypeAndVar_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataMergeDTO = new DataMergeDTO("s", "NewLine", null, 1, null, null);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataMergeDTO.CanRemove());
        }

        #endregion
    }
}
