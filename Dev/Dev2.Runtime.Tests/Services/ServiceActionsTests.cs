using System;
using Dev2.Runtime.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class ServiceActionsTests
    {
        [TestMethod]
        public void TestMockOutput()
        {
            ServiceActions serviceActions = new ServiceActions();

            string expected = "[{\"Name\":\"Action1\",\"ActionType\":\"Unknown\",\"SourceName\":null,\"SourceMethod\":null,\"OutputDescription\":null,\"ServiceActionInputs\":[],\"ServiceActionOutputs\":[]},{\"Name\":\"Action2\",\"ActionType\":\"Unknown\",\"SourceName\":null,\"SourceMethod\":null,\"OutputDescription\":null,\"ServiceActionInputs\":[],\"ServiceActionOutputs\":[]},{\"Name\":\"Action3\",\"ActionType\":\"Unknown\",\"SourceName\":null,\"SourceMethod\":null,\"OutputDescription\":null,\"ServiceActionInputs\":[],\"ServiceActionOutputs\":[]}]";
            string actual = serviceActions.List("", Guid.Empty, Guid.Empty); ;

            Assert.AreEqual(expected, actual);
        }
    }
}
