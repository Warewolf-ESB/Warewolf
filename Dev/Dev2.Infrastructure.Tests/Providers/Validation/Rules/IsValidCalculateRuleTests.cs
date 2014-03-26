//using Dev2.Intellisense.Validation;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//
//namespace Dev2.Infrastructure.Tests.Providers.Validation.Rules
//{
//    [TestClass]
//    // ReSharper disable InconsistentNaming
//    public class IsValidCalculateRuleTests
//    {
//        [TestMethod]
//        [Owner("Tshepo Ntlhokoa")]
//        [TestCategory("IsValidCalculateRule_Check")]
//        public void IsValidCalculateRule_Check_CalculateExpressionIsValid_ReturnsError()
//        {
//            //------------Setup for test--------------------------
//            var rule = new IsValidCalculateRule(() => "pi(1)") { LabelText = "Calculate" };
//            //------------Execute Test---------------------------
//            var errorInfo = rule.Check();
//            //------------Assert Results-------------------------
//            Assert.IsNotNull(errorInfo);
//            Assert.AreEqual("Calculate An error occured while parsing { pi }, the function must be called without arguments", errorInfo.Message);
//        }
//        
//    }
//}
