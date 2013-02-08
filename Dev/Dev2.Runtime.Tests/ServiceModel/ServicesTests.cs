using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    public class ServicesTests
    {
        [TestMethod]
        public void TestMockOutput()
        {
            var services = new Dev2.Runtime.ServiceModel.Services();

            string expected = "[{\"Name\":\"Action1\",\"ActionType\":\"Unknown\",\"SourceName\":null,\"SourceMethod\":null,\"OutputDescription\":null,\"ServiceActionInputs\":[],\"ServiceActionOutputs\":[]},{\"Name\":\"Action2\",\"ActionType\":\"Unknown\",\"SourceName\":null,\"SourceMethod\":null,\"OutputDescription\":null,\"ServiceActionInputs\":[],\"ServiceActionOutputs\":[]},{\"Name\":\"Action3\",\"ActionType\":\"Unknown\",\"SourceName\":null,\"SourceMethod\":null,\"OutputDescription\":null,\"ServiceActionInputs\":[],\"ServiceActionOutputs\":[]}]";
            string actual = services.Actions("", Guid.Empty, Guid.Empty); ;

            Assert.AreEqual(expected, actual);
        }

        #region Sources

        [TestMethod]
        public void SourcesWith()
        {
        }

        #endregion

        #region Save

        [TestMethod]
        public void SaveWith()
        {
            var services = new Dev2.Runtime.ServiceModel.Services();
        }

        #endregion
    }
}
