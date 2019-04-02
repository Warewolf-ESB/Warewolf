using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class IsDateTests
    {
        [TestInitialize]
        public void PreConditions()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-ZA");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-ZA");

            Assert.AreEqual("en-ZA", System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            Assert.AreEqual("en-ZA", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IsDate_Invoke")]
        public void GivenSomeString_IsDate_Invoke_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isDate = new IsDate();
            var cols = new string[2];
            cols[0] = "Yersteday";            
            //------------Execute Test---------------------------
            var result = isDate.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IsDate_Invoke")]
        public void IsDate_Invoke_IsDate_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new IsDate();
            var cols = new string[2];
            cols[0] = "01/12/2000";
            //------------Execute Test---------------------------
            var result = notStartsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsDate_HandlesType")]
        public void IsDate_HandlesType_ReturnsIsDateType()
        {
            var decisionType = enDecisionType.IsDate;
            //------------Setup for test--------------------------
            var isDate = new IsDate();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isDate.HandlesType());
        }
    }
}
