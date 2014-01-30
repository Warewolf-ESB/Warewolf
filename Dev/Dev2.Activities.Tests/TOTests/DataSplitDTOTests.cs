using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.TOTests
{
    /// <summary>
    /// Summary description for DataSplitDTOTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataSplitDTOTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_Constructor")]
        public void DataSplitDTO_Constructor_FullConstructor_DefaultValues()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO(string.Empty,null,null,1);
            //------------Assert Results-------------------------
            Assert.AreEqual("Index", dataSplitDTO.SplitType);
            Assert.AreEqual(string.Empty, dataSplitDTO.At);
            Assert.AreEqual(1, dataSplitDTO.IndexNumber);
            Assert.IsNull(dataSplitDTO.EscapeChar);
            Assert.AreEqual(false, dataSplitDTO.Include);
            Assert.IsNotNull(dataSplitDTO.Errors);
        }

        #region CanAdd Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_CanAdd")]
        public void DataSplitDTO_CanAdd_WithNewLineSplitTypeAndNoOtherValues_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO(string.Empty, "NewLine", null, 1);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataSplitDTO.CanAdd());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_CanAdd")]
        public void DataSplitDTO_CanAdd_WithNoInputVarButValueForAt_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO(string.Empty, null, "|", 1);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataSplitDTO.CanAdd());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_CanAdd")]
        public void DataSplitDTO_CanAdd_WithNoInputVarAndNoAt_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO(string.Empty, null, null, 1);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataSplitDTO.CanAdd());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_CanAdd")]
        public void DataSplitDTO_CanAdd_WithIndexSplitTypeAndNoOtherValues_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO(string.Empty, "Index", null, 1);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataSplitDTO.CanAdd());
        }

        #endregion

        #region CanRemove Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_CanRemove")]
        public void DataSplitDTO_CanRemove_WithNoInputVarButValueForAt_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO(string.Empty, null, "|", 1);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataSplitDTO.CanRemove());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_CanRemove")]
        public void DataSplitDTO_CanRemove_WithNoInputVarAndNoAt_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO(string.Empty, null, null, 1);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataSplitDTO.CanRemove());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_CanRemove")]
        public void DataSplitDTO_CanRemove_WithNewLineTypeAndNoInputVar_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO(string.Empty, "NewLine", null, 1);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataSplitDTO.CanRemove());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataSplitDTO_CanRemove")]
        public void DataSplitDTO_CanRemove_WithNewLineInputTypeAndVar_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataSplitDTO = new DataSplitDTO("s", "NewLine", null, 1);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataSplitDTO.CanRemove());
        }

        #endregion
    }
}
