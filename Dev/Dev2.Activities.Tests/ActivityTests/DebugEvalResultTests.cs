using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class DebugEvalResultTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DebugEvalResult_GivenIsNew_ShouldSetValues()
        {
            //---------------Set up test pack-------------------
            var env = new Mock<IExecutionEnvironment>();
            env.Setup(environment => environment.EvalToExpression(It.IsAny<string>(), It.IsAny<int>())).Returns("[[scalar]]");
            var warewolfAtomResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("{\"PolicyNo\":\"A0003\",\"DateId\":32,\"SomeVal\":\"Bob\"}"));
            env.Setup(environment => environment.Eval(It.IsAny<string>(), It.IsAny<int>())).Returns(warewolfAtomResult);
            var debugEvalResult = new DebugEvalResult("[[scalar]]", "", env.Object, 0);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(debugEvalResult);
            //---------------Execute Test ----------------------
            Assert.AreEqual("", debugEvalResult.LabelText);
            var debugItemResults = debugEvalResult.GetDebugItemResult();
            //---------------Test Result -----------------------
            var variable = debugItemResults[0].Variable;
            Assert.AreEqual("[[scalar]]", variable);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DebugEvalResult_GivenIsNewAndIsJSonArray_ShouldSetObjectArray()
        {
            //---------------Set up test pack-------------------
            var env = new Mock<IExecutionEnvironment>();
            env.Setup(environment => environment.EvalToExpression(It.IsAny<string>(), It.IsAny<int>())).Returns("[[@scalar()]]");
            var warewolfAtomResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("{\"PolicyNo\":\"A0003\",\"DateId\":32,\"SomeVal\":\"Bob\"}"));
            env.Setup(environment => environment.Eval(It.IsAny<string>(), It.IsAny<int>())).Returns(warewolfAtomResult);
            var debugEvalResult = new DebugEvalResult("[[@scalar()]]", "", env.Object, 0);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(debugEvalResult);
            //---------------Execute Test ----------------------
            Assert.AreEqual("", debugEvalResult.LabelText);
            var debugItemResults = debugEvalResult.GetDebugItemResult();
            //---------------Test Result -----------------------
            var variable = debugItemResults[0].Variable;
            Assert.AreEqual("[[@scalar()]]", variable);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DebugEvalResult_GivenObjectIsJSonArray_ShouldSetObjectArray()
        {
            //---------------Set up test pack-------------------
            var env = new Mock<IExecutionEnvironment>();
            env.Setup(environment => environment.EvalToExpression(It.IsAny<string>(), It.IsAny<int>())).Returns("[[@scalar]]");
            var warewolfAtomResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("{\"PolicyNo\":\"A0003\",\"DateId\":32,\"SomeVal\":\"Bob\"}"));
            env.Setup(environment => environment.Eval(It.IsAny<string>(), It.IsAny<int>())).Returns(warewolfAtomResult);
            var debugEvalResult = new DebugEvalResult("[[@scalar]]", "", env.Object, 0);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(debugEvalResult);
            //---------------Execute Test ----------------------
            Assert.AreEqual("", debugEvalResult.LabelText);
            var debugItemResults = debugEvalResult.GetDebugItemResult();
            //---------------Test Result -----------------------
            var variable = debugItemResults[0].Variable;
            Assert.AreEqual("[[@scalar]]", variable);
        }
    }
}
