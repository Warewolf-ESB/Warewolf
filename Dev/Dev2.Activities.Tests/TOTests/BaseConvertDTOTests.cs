using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.TOTests
{
    /// <summary>
    /// Summary description for BaseConvertDTOTests
    /// </summary>
    [TestClass]
    public class BaseConvertDTOTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("BaseConvertDTO_Constructor")]
        public void DataSplitDTO_Constructor_FullConstructor_DefaultValues()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var baseConvertDTO = new BaseConvertTO(string.Empty,null,null,null,1);
            //------------Assert Results-------------------------
            Assert.AreEqual("Text", baseConvertDTO.FromType);
            Assert.AreEqual("Base 64", baseConvertDTO.ToType);
            Assert.AreEqual(string.Empty, baseConvertDTO.ToExpression);
            Assert.AreEqual(1, baseConvertDTO.IndexNumber);
            Assert.IsNull(baseConvertDTO.Errors);
        }
    }
}
