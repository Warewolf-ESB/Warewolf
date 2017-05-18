using Dev2.Activities.Debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class DebugItemWarewolfAtomListResultTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DebugItemWarewolfAtomListResult_GivenIsNew_GetDebugItemResult()
        {
            //---------------Set up test pack-------------------
            var env = new Mock<IExecutionEnvironment>();
            env.Setup(environment => environment.EvalToExpression(It.IsAny<string>(), It.IsAny<int>())).Returns("[[scalar]]");
            var warewolfAtomResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("{\"PolicyNo\":\"A0003\",\"DateId\":32,\"SomeVal\":\"Bob\"}"));
            env.Setup(environment => environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(warewolfAtomResult);
            WarewolfAtomList<DataStorage.WarewolfAtom> atomList = new WarewolfAtomList<DataStorage.WarewolfAtom>(DataStorage.WarewolfAtom.NewDataString("Home"));
            CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult newWarewolfAtomListresult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(atomList) as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            Assert.IsNotNull(newWarewolfAtomListresult);
            newWarewolfAtomListresult.Item.AddSomething(DataStorage.WarewolfAtom.NewDataString("KingDom Of The Zulu"));
            var debugEvalResult = new DebugItemWarewolfAtomListResult(newWarewolfAtomListresult, warewolfAtomResult, "newValue", "Variable", "", "", "");
            //---------------Assert Precondition----------------
            Assert.IsNotNull(debugEvalResult);
            //---------------Execute Test ----------------------
            Assert.AreEqual("", debugEvalResult.LabelText);
            var debugItemResults = debugEvalResult.GetDebugItemResult();
            //---------------Test Result -----------------------
            Assert.AreEqual(1, debugItemResults.Count);
            Assert.AreEqual("KingDom Of The Zulu", debugItemResults[0].Value);
            Assert.IsNotNull(debugEvalResult);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DebugItemWarewolfAtomListResult_GivenJsonArrayResult_GetDebugItemResult()
        {
            //---------------Set up test pack-------------------
            var env = new Mock<IExecutionEnvironment>();
            env.Setup(environment => environment.EvalToExpression(It.IsAny<string>(), It.IsAny<int>())).Returns("[[scalar]]");
            var warewolfAtomResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("{\"PolicyNo\":\"A0003\",\"DateId\":32,\"SomeVal\":\"Bob\"}"));
            env.Setup(environment => environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(warewolfAtomResult);
            WarewolfAtomList<DataStorage.WarewolfAtom> atomList = new WarewolfAtomList<DataStorage.WarewolfAtom>(DataStorage.WarewolfAtom.NewDataString("Home"));
            CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult newWarewolfAtomListresult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(atomList) as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            Assert.IsNotNull(newWarewolfAtomListresult);
            newWarewolfAtomListresult.Item.AddSomething(DataStorage.WarewolfAtom.NewDataString("KingDom Of The Zulu"));
            var debugEvalResult = new DebugItemWarewolfAtomListResult(newWarewolfAtomListresult, warewolfAtomResult, "newValue", "[[@Variable()]]", "", "", "");
            //---------------Assert Precondition----------------
            Assert.IsNotNull(debugEvalResult);
            //---------------Execute Test ----------------------
            Assert.AreEqual("", debugEvalResult.LabelText);
            var debugItemResults = debugEvalResult.GetDebugItemResult();
            //---------------Test Result -----------------------
            Assert.AreEqual(1, debugItemResults.Count);
            Assert.AreEqual("KingDom Of The Zulu", debugItemResults[0].Value);
            Assert.AreEqual("[[@Variable(1)]]", debugItemResults[0].Variable);
            Assert.AreEqual("[[@Variable()]]", debugItemResults[0].GroupName);
            Assert.IsNotNull(debugEvalResult);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DebugItemWarewolfAtomListResult_GivenRecordsetResult_GetDebugItemResult()
        {
            //---------------Set up test pack-------------------
            var env = new Mock<IExecutionEnvironment>();
            env.Setup(environment => environment.EvalToExpression(It.IsAny<string>(), It.IsAny<int>())).Returns("[[scalar]]");
            var warewolfAtomResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("{\"PolicyNo\":\"A0003\",\"DateId\":32,\"SomeVal\":\"Bob\"}"));
            env.Setup(environment => environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(warewolfAtomResult);
            WarewolfAtomList<DataStorage.WarewolfAtom> atomList = new WarewolfAtomList<DataStorage.WarewolfAtom>(DataStorage.WarewolfAtom.NewDataString("Home"));
            CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult newWarewolfAtomListresult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(atomList) as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            Assert.IsNotNull(newWarewolfAtomListresult);
            newWarewolfAtomListresult.Item.AddSomething(DataStorage.WarewolfAtom.NewDataString("KingDom Of The Zulu"));
            var debugEvalResult = new DebugItemWarewolfAtomListResult(newWarewolfAtomListresult, warewolfAtomResult, "newValue", "[[Variable().Name]]", "", "", "");
            //---------------Assert Precondition----------------
            Assert.IsNotNull(debugEvalResult);
            //---------------Execute Test ----------------------
            Assert.AreEqual("", debugEvalResult.LabelText);
            var debugItemResults = debugEvalResult.GetDebugItemResult();
            //---------------Test Result -----------------------
            Assert.AreEqual(1, debugItemResults.Count);
            Assert.AreEqual("KingDom Of The Zulu", debugItemResults[0].Value);
            Assert.AreEqual("[[Variable(1).Name]]", debugItemResults[0].Variable);
            Assert.AreEqual("[[Variable().Name]]", debugItemResults[0].GroupName);
            Assert.IsNotNull(debugEvalResult);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DebugItemWarewolfAtomListResult_GetDebugItemResult()
        {
            //---------------Set up test pack-------------------
            var env = new Mock<IExecutionEnvironment>();
            env.Setup(environment => environment.EvalToExpression(It.IsAny<string>(), It.IsAny<int>())).Returns("[[@scalar()]]");
            var warewolfAtomResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("{\"PolicyNo\":\"A0003\",\"DateId\":32,\"SomeVal\":\"Bob\"}"));
            env.Setup(environment => environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(warewolfAtomResult);
            WarewolfAtomList<DataStorage.WarewolfAtom> atomList = new WarewolfAtomList<DataStorage.WarewolfAtom>(DataStorage.WarewolfAtom.NewDataString("Home"));
            CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult newWarewolfAtomListresult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(atomList) as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            var debugEvalResult = new DebugItemWarewolfAtomListResult(newWarewolfAtomListresult, "newValue", "[[@home]]", "a", "b", "b", "");
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            var debugItemResults = debugEvalResult.GetDebugItemResult();
            //---------------Test Result -----------------------
            var value = debugItemResults[0].Value;
            var operato = debugItemResults[0].Operator;
            Assert.AreEqual("newValue", value);
            Assert.AreEqual("", debugEvalResult.LabelText);
            Assert.AreEqual("=", operato);
        }
    }
}