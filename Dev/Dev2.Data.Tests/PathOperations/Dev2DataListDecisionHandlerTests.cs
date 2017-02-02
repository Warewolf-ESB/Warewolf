using System;
using Dev2.Data.Decision;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class Dev2DataListDecisionHandlerTests
    {
        [TestMethod]
        public void Dev2DataListDecisionHandler_ShouldHaveInstance()
        {
            Assert.IsNotNull(Dev2DataListDecisionHandler.Instance);
        }

        [TestMethod]
        public void Dev2DataListDecisionHandler_AssEnvironment_ShouldIncreaseEnvironmentsCount()
        {
            var instance = Dev2DataListDecisionHandler.Instance;
            Assert.IsNotNull(instance);
            instance.AddEnvironment(Guid.NewGuid(), new ExecutionEnvironment());
        }
    }
}
