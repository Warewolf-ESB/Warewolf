using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;


namespace Dev2.Data.Tests
{
    [TestClass]
    public class WarewolfListIteratorTests
    {

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WarewolfListIterator_FetchNextValue")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WarewolfListIterator_FetchNextValue_NoValuesToIterateOn_ReturnsException()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            var warewolfListIterator = new WarewolfListIterator();
            var warewolfIterator = new WarewolfIterator(env.Eval("[[rec().a]]", 0));
            //------------Execute Test---------------------------
            var value = warewolfListIterator.FetchNextValue(warewolfIterator);
            //------------Assert Results-------------------------
            Assert.IsNull(value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WarewolfListIterator_FetchNextValue")]
        public void WarewolfListIterator_FetchNextValue_HasValues_ShouldReturnValue()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            env.Assign("[[rec().a]]", "Test", 0);
            env.Assign("[[rec().a]]", "Test2", 0);
            env.Assign("[[rec().a]]", "Test4", 0);
            env.Assign("[[rec().a]]", "Test5", 0);
            env.CommitAssign();
            var warewolfListIterator = new WarewolfListIterator();
            var warewolfIterator = new WarewolfIterator(env.Eval("[[rec().a]]", 0));
            warewolfListIterator.AddVariableToIterateOn(warewolfIterator);
            //------------Execute Test---------------------------
            var value = warewolfListIterator.FetchNextValue(warewolfIterator);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test5", value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WarewolfListIterator_FetchNextValue")]
        public void WarewolfListIterator_FetchNextValue_WithStar_HasValues_ShouldReturnValues()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            env.Assign("[[rec().a]]", "Test", 0);
            env.Assign("[[rec().a]]", "Test2", 0);
            env.Assign("[[rec().a]]", "Test4", 0);
            env.Assign("[[rec().a]]", "Test5", 0);
            env.CommitAssign();
            var warewolfListIterator = new WarewolfListIterator();
            var warewolfIterator = new WarewolfIterator(env.Eval("[[rec(*).a]]", 0));
            warewolfListIterator.AddVariableToIterateOn(warewolfIterator);
            //------------Execute Test---------------------------
            var value = warewolfListIterator.FetchNextValue(warewolfIterator);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", value);
            //------------Execute Test---------------------------
            value = warewolfListIterator.FetchNextValue(warewolfIterator);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WarewolfListIterator_FetchNextValue")]
        public void WarewolfListIterator_FetchNextValue_WithIndex_HasValues_ShouldReturnValues()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            env.Assign("[[rec().a]]", "Test", 0);
            env.Assign("[[rec().a]]", "Test2", 0);
            env.Assign("[[rec().a]]", "Test4", 0);
            env.Assign("[[rec().a]]", "Test5", 0);
            env.CommitAssign();
            var warewolfListIterator = new WarewolfListIterator();
            var warewolfIterator = new WarewolfIterator(env.Eval("[[rec(3).a]]", 0));
            warewolfListIterator.AddVariableToIterateOn(warewolfIterator);
            //------------Execute Test---------------------------
            var value = warewolfListIterator.FetchNextValue(warewolfIterator);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test4", value);
            //------------Execute Test---------------------------
            value = warewolfListIterator.FetchNextValue(warewolfIterator);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test4", value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WarewolfListIterator_FetchNextValue")]
        public void WarewolfListIterator_FetchNextValue_PassLastValue_HasValues_ShouldReturnValues()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            env.Assign("[[rec().a]]", "Test", 0);
            env.CommitAssign();
            var warewolfListIterator = new WarewolfListIterator();
            var warewolfIterator = new WarewolfIterator(env.Eval("[[rec(*).a]]", 0));
            warewolfListIterator.AddVariableToIterateOn(warewolfIterator);
            //------------Execute Test---------------------------
            var value = warewolfListIterator.FetchNextValue(warewolfIterator);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", value);
            //------------Execute Test---------------------------
            value = warewolfListIterator.FetchNextValue(warewolfIterator);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WarewolfListIterator_FetchNextValue")]
        public void WarewolfListIterator_FetchNextValue_WithScalar_HasValues_ShouldReturnValues()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            env.Assign("[[a]]", "Test", 0);
            env.CommitAssign();
            var warewolfListIterator = new WarewolfListIterator();
            var warewolfIterator = new WarewolfIterator(env.Eval("[[a]]", 0));
            warewolfListIterator.AddVariableToIterateOn(warewolfIterator);
            //------------Execute Test---------------------------
            var value = warewolfListIterator.FetchNextValue(warewolfIterator);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", value);
            //------------Execute Test---------------------------
            value = warewolfListIterator.FetchNextValue(warewolfIterator);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", value);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WarewolfListIterator_FetchNextValue")]
        public void WarewolfListIterator_FetchNextValue_WithIndex_HasMoreData_ShouldReturnTrue_WhenCounterSmallerThanLargestIndex()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            env.Assign("[[rec().a]]", "Test", 0);
            env.Assign("[[rec().a]]", "Test2", 0);
            env.Assign("[[rec().a]]", "Test4", 0);
            env.Assign("[[rec().a]]", "Test5", 0);
            env.CommitAssign();
            var warewolfListIterator = new WarewolfListIterator();
            var warewolfIterator = new WarewolfIterator(env.Eval("[[rec().a]]", 0));
            warewolfListIterator.AddVariableToIterateOn(warewolfIterator);
            //------------Execute Test---------------------------
            var hasMoreData = warewolfListIterator.HasMoreData();
            //------------Assert Results-------------------------
            Assert.IsTrue(hasMoreData);
        }
    }
}
