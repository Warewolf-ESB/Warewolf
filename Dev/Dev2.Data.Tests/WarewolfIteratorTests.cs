using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarewolfParserInterop;


namespace Dev2.Data.Tests
{
    [TestClass]
    public class WarewolfIteratorTests
    {
        [TestMethod]
        public void WarewolfIterator_Should()
        {
            CommonFunctions.WarewolfEvalResult listResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing);
            var atomIterator = new WarewolfIterator(listResult);
            Assert.IsNotNull(atomIterator);
            var privateObj = new PrivateObject(atomIterator);
            var maxVal = (int) privateObj.GetField("_maxValue");
            Assert.IsNotNull(maxVal);
            var length = atomIterator.GetLength();
            Assert.AreEqual(maxVal, length);
        }
        [TestMethod]
        public void WarewolfIterator_SetupForWarewolfRecordSetResult_Should()
        {
            CommonFunctions.WarewolfEvalResult listResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing);
            var atomIterator = new WarewolfIterator(listResult);
            Assert.IsNotNull(atomIterator);
            var privateObj = new PrivateObject(atomIterator);
            var assigns = new List<IAssignValue>
             {
                 new AssignValue("[[rec(25).a]]", "25"),
                 new AssignValue("[[rec(27).b]]", "33"),
                 new AssignValue("[[rec(29).b]]", "26")
             };
            var testEnv = WarewolfTestData.CreateTestEnvEmpty("");
            var testEnv3 = PublicFunctions.EvalMultiAssign(assigns, 0, testEnv);
            var res = PublicFunctions.EvalEnvExpression("[[rec(27)]]", 0, false, testEnv3);
            Assert.IsTrue(res.IsWarewolfRecordSetResult);
            object[] arg = { res };
            privateObj.Invoke("SetupForWarewolfRecordSetResult", arg);
            var scalarRes = privateObj.GetField("_scalarResult") as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsNotNull(scalarRes);
        }
    }
}
