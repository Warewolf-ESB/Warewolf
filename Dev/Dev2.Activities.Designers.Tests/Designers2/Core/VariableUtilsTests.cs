using Dev2.Activities.Designers2.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Designers.Tests.Designers2.Core
{
    [TestClass]
    public class VariableUtilsTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("VariableUtils_TryParseVariables")]
        public void VariableUtils_TryParseVariables_InputValueIsNullOrEmpty_NoErrors()
        {
            //------------Setup for test--------------------------
            string outputValue;

            //------------Execute Test---------------------------
            var result = VariableUtils.TryParseVariables(null, out outputValue, () => { });

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);
        }
    }
}
