using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for CaseConvertActivityTests
    /// </summary>
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DecisionActivityTests : BaseActivityUnitTest
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDecision_SerializeDeserialize")]
        public void DsfDecision_SerializeDeserialize_WhenAndSetTrue_ShouldHaveAndAsTrueWhenDeserialized()
        {
            //------------Setup for test--------------------------
            var dsfDecision = new DsfDecision { And = true };
            var serializer = new Dev2JsonSerializer();
            var serDecision = serializer.Serialize(dsfDecision);
            //------------Execute Test---------------------------
            var deSerDecision = serializer.Deserialize<DsfDecision>(serDecision);
            //------------Assert Results-------------------------
            Assert.IsNotNull(deSerDecision);
            Assert.IsTrue(deSerDecision.And);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDecision_SerializeDeserialize")]
        public void DsfDecision_SerializeDeserialize_WhenAndSetFalse_ShouldHaveAndAsFalseWhenDeserialized()
        {
            //------------Setup for test--------------------------
            var dsfDecision = new DsfDecision { And = false };
            var serializer = new Dev2JsonSerializer();
            var serDecision = serializer.Serialize(dsfDecision);
            //------------Execute Test---------------------------
            var deSerDecision = serializer.Deserialize<DsfDecision>(serDecision);
            //------------Assert Results-------------------------
            Assert.IsNotNull(deSerDecision);
            Assert.IsFalse(deSerDecision.And);
        }
    }
}