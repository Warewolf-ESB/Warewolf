using Dev2.Data.Enums;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests
{
    [TestClass]
    public class GatherSystemInformationTOTests
    {
        [TestMethod]
        public void GatherSystemInformationTOShouldImplementIDev2TOFn()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            GatherSystemInformationTO informationTO = new GatherSystemInformationTO();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(informationTO,typeof(IDev2TOFn));
        }

        [TestMethod]
        public void ConstructorWhereParametersExpectSetsProperties()
        {
            //------------Setup for test--------------------------
            const string ResultVariable = "[[Output]]";
            var toGather = enTypeOfSystemInformationToGather.OperatingSystem;
            const int IndexNumber = 0;
            //------------Execute Test---------------------------
            var gatherSystemInformationTO = new GatherSystemInformationTO(toGather,ResultVariable,IndexNumber);
            //------------Assert Results-------------------------
            Assert.IsNotNull(gatherSystemInformationTO);
            Assert.AreEqual(ResultVariable,gatherSystemInformationTO.Result);
            Assert.AreEqual(toGather,gatherSystemInformationTO.EnTypeOfSystemInformation);
            Assert.AreEqual(IndexNumber,gatherSystemInformationTO.IndexNumber);

        }
    }

  
}