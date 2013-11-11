using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.TOTests
{
    /// <summary>
    /// Summary description for CaseConvertDTOTests
    /// </summary>
    [TestClass]
    public class CaseConvertDTOTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("CaseConvertDTO_Constructor")]
        public void DataSplitDTO_Constructor_FullConstructor_DefaultValues()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var caseConvertDTO = new CaseConvertTO(string.Empty,null,null,1);
            //------------Assert Results-------------------------
            Assert.AreEqual("UPPER", caseConvertDTO.ConvertType);            
            Assert.AreEqual(string.Empty, caseConvertDTO.Result);
            Assert.AreEqual(1, caseConvertDTO.IndexNumber);
            Assert.IsNull(caseConvertDTO.Errors);
        }
    }
}
