using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

namespace Dev2.Tests.Activities
{
    [TestClass]
    public class TestMockStepTests
    {
        [TestMethod]
        [Timeout(60000)]
        public void TestMockStep_ServiceTestOutputs_HaveOutput()
        {
            var originalAct = new Mock<DsfActivityAbstract<string>>();
            var env = new ExecutionEnvironment();
            var dataMock = new Mock<IDSFDataObject>();
            dataMock.Setup(o => o.Environment).Returns(() => env);

            const string theValue = "theValue";
            var input = new Mock<IServiceTestOutput>();
            input.Setup(o => o.Variable).Returns(() => "[[list()]]");
            input.Setup(o => o.Value).Returns(() => theValue);
            var outputs = new List<IServiceTestOutput>
            {
                input.Object
            };
            var act = new TestMockStep(originalAct.Object, outputs);


            act.Execute(dataMock.Object, 0);

            var results = env.EvalAsListOfStrings("[[@list()]]", 0);
            Assert.AreEqual(theValue, results[0]);
        }

        [TestMethod]
        [Timeout(60000)]
        public void TestMockStep_Equality()
        {
            var act = new TestMockStep();
            Object act1 = null;

            Assert.IsFalse(act.Equals(act1));
            Assert.IsTrue(act.Equals(act));
            act1 = act;
            Assert.IsTrue(act.Equals(act1));

            act1 = new TestMockStep();
            Assert.IsFalse(act.Equals(act1));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestMockStep_GetState")]
        public void TestMockStep_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            //------------Setup for test--------------------------
            var act = new TestMockStep();
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, stateItems.Count());
            
        }
    }
}
