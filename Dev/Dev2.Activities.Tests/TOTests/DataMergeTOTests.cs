using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.TOTests
{
    /// <summary>
    /// Summary description for DataMergeTOTests
    /// </summary>
    [TestClass]
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
            Assert.IsNull(dataMergeDTO.Errors);
        }     
    }
}
