using System;
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
            Assert.IsNull(dataSplitDTO.Errors);
        }
    }
}
