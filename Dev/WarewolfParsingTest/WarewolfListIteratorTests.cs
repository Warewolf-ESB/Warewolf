using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;

// ReSharper disable InconsistentNaming
namespace WarewolfParsingTest
{
    [TestClass]
    public class WarewolfListIteratorTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WarewolfListIterator_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WarewolfListIterator_Constructor_NullEnvironment_NullException()
        {
            //------------Setup for test--------------------------
            
                
            //------------Execute Test---------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new WarewolfListIterator(null);
            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WarewolfListIterator_FetchNextValue")]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void WarewolfListIterator_FetchNextValue_NoValuesToIterateOn_ReturnsException()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            var warewolfListIterator = new WarewolfListIterator(env);
            //------------Execute Test---------------------------
            var value = warewolfListIterator.FetchNextValue("[[rec().a]]");
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
            env.Assign("[[rec().a]]", "Test");
            env.Assign("[[rec().a]]", "Test2");
            env.Assign("[[rec().a]]", "Test4");
            env.Assign("[[rec().a]]", "Test5");
            env.CommitAssign();
            var warewolfListIterator = new WarewolfListIterator(env);
            warewolfListIterator.AddVariableToIterateOn("[[rec().a]]");
            //------------Execute Test---------------------------
            var value = warewolfListIterator.FetchNextValue("[[rec().a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual("Test5",value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WarewolfListIterator_FetchNextValue")]
        public void WarewolfListIterator_FetchNextValue_WithStar_HasValues_ShouldReturnValues()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            env.Assign("[[rec().a]]", "Test");
            env.Assign("[[rec().a]]", "Test2");
            env.Assign("[[rec().a]]", "Test4");
            env.Assign("[[rec().a]]", "Test5");
            env.CommitAssign();
            var warewolfListIterator = new WarewolfListIterator(env);
            warewolfListIterator.AddVariableToIterateOn("[[rec(*).a]]");
            //------------Execute Test---------------------------
            var value = warewolfListIterator.FetchNextValue("[[rec(*).a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual("Test",value);
            //------------Execute Test---------------------------
            value = warewolfListIterator.FetchNextValue("[[rec(*).a]]");
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
            env.Assign("[[rec().a]]", "Test");
            env.Assign("[[rec().a]]", "Test2");
            env.Assign("[[rec().a]]", "Test4");
            env.Assign("[[rec().a]]", "Test5");
            env.CommitAssign();
            var warewolfListIterator = new WarewolfListIterator(env);
            warewolfListIterator.AddVariableToIterateOn("[[rec(3).a]]");
            //------------Execute Test---------------------------
            var value = warewolfListIterator.FetchNextValue("[[rec(3).a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual("Test4",value);
            //------------Execute Test---------------------------
            value = warewolfListIterator.FetchNextValue("[[rec(3).a]]");
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
            env.Assign("[[rec().a]]", "Test");
            env.CommitAssign();
            var warewolfListIterator = new WarewolfListIterator(env);
            warewolfListIterator.AddVariableToIterateOn("[[rec(*).a]]");
            //------------Execute Test---------------------------
            var value = warewolfListIterator.FetchNextValue("[[rec(*).a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual("Test",value);
            //------------Execute Test---------------------------
            value = warewolfListIterator.FetchNextValue("[[rec(*).a]]");
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
            env.Assign("[[a]]", "Test");
            env.CommitAssign();
            var warewolfListIterator = new WarewolfListIterator(env);
            warewolfListIterator.AddVariableToIterateOn("[[a]]");
            //------------Execute Test---------------------------
            var value = warewolfListIterator.FetchNextValue("[[a]]");
            //------------Assert Results-------------------------
            Assert.AreEqual("Test",value);
            //------------Execute Test---------------------------
            value = warewolfListIterator.FetchNextValue("[[a]]");
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
            env.Assign("[[rec().a]]", "Test");
            env.Assign("[[rec().a]]", "Test2");
            env.Assign("[[rec().a]]", "Test4");
            env.Assign("[[rec().a]]", "Test5");
            env.CommitAssign();
            var warewolfListIterator = new WarewolfListIterator(env);
            warewolfListIterator.AddVariableToIterateOn("[[rec(3).a]]");
            //------------Execute Test---------------------------
            var hasMoreData = warewolfListIterator.HasMoreData();
            //------------Assert Results-------------------------
            Assert.IsTrue(hasMoreData);            
        } 
    }
}
