using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Operations
{
    [TestClass]
    public class Dev2DecisionFactoryTests
    {
        [TestMethod]
        [Owner("Sanele Mthmembu")]        
        public void GivenIsLessThanDev2DecisionFactory_HandleType_ShouldReturnIsLessThan()
        {
            var decisionType = enDecisionType.IsLessThan;
            //------------Setup for test--------------------------
            var decisionFactory = new Dev2DecisionFactory();
            //------------Execute Test---------------------------
            var fetchDecisionFunction = decisionFactory.FetchDecisionFunction(decisionType);             
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, fetchDecisionFunction.HandlesType());
        }
        [TestMethod]
        [Owner("Sanele Mthmembu")]        
        public void Dev2DecisionFactory_Instance_ShouldHaveAStaticInstance()
        {            
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(Dev2DecisionFactory.Instance());            
        }
    }
}
