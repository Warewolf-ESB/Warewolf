using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Storage.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class WarewolfListIteratorTests
    {
        IWarewolfIterator _expr1;
        IWarewolfIterator _expr2;
        IWarewolfIterator _expr3;
        private IExecutionEnvironment _environment;
        private IWarewolfListIterator _warewolfListIterator;

        const string Result = "[[Result]]";

        [TestInitialize]
        public void TestInitialize()
        {
            _environment = new ExecutionEnvironment();
            _warewolfListIterator = new WarewolfListIterator();
            
            _environment.Assign(Result, "Success", 0);
            _expr1 = new WarewolfIterator(_environment.Eval(Result, 0));
            _expr2 = new WarewolfIterator(_environment.Eval("[[@Parent()]]", 0));

            _warewolfListIterator.AddVariableToIterateOn(_expr1);
            _warewolfListIterator.AddVariableToIterateOn(_expr2);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void WarewolfListIteratorInstance_ShouldHaveConstructor()
        {            
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn");
            var count = privateObj.GetField("_count");
            Assert.IsNotNull(variablesToIterateOn);
            Assert.AreEqual(-1, count);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenResultExpression_WarewolfListIterator_FetchNextValue_Should()
        {
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);

            var result = variablesToIterateOn.FirstOrDefault();
            var fetchNextValue = _warewolfListIterator.FetchNextValue(result);
            Assert.AreEqual("Success", fetchNextValue);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenPosition_WarewolfListIterator_FetchNextValue_Should()
        {
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);
            var listIterator = _warewolfListIterator as WarewolfListIterator;
            if (listIterator == null) return;
            var fetchNextValue = listIterator.FetchNextValue(0);
            Assert.AreEqual("Success", fetchNextValue);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void WarewolfListIterator_AddVariableToIterateOn_Should()
        {
            _expr3 = new WarewolfIterator(_environment.Eval("[[RecSet()]]", 0));            
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);
            Assert.AreEqual(2, variablesToIterateOn.Count);
            _warewolfListIterator.AddVariableToIterateOn(_expr3);
            Assert.AreEqual(3, variablesToIterateOn.Count);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void WarewolfListIterator_GetMax_ShouldReturn1()
        {
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);
            var max = _warewolfListIterator.GetMax();
            Assert.AreEqual(1, max);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void WarewolfListIterator_HasMoreData_ShouldReturnTrue()
        {
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);
            var hasMoreData = _warewolfListIterator.HasMoreData();
            Assert.IsTrue(hasMoreData);
        }
    }
}