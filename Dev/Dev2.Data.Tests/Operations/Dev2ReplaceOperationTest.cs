using Dev2.Data.Interfaces;
using Dev2.Data.Operations;
using Dev2.Data.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Operations
{
    [TestClass]
    public class Dev2ReplaceOperationTests
    {
        private IDev2ReplaceOperation _dev2ReplaceOperation;

        [TestInitialize]
        public void Initialization()
        {
            _dev2ReplaceOperation = Dev2OperationsFactory.CreateReplaceOperation();
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Dev2ReplaceOperation_ShouldHaveInstance()
        {
            Assert.IsNotNull(_dev2ReplaceOperation);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenReplaceFolderWithC_Dev2ReplaceOperation_Replace_ShouldReturnColder()
        {
            Assert.IsNotNull(_dev2ReplaceOperation);
            ErrorResultTO errorResultTo;
            int replaceCount = 0;
            var result = _dev2ReplaceOperation.Replace("FOLDER", "F", "C", false, out errorResultTo, ref replaceCount);
            Assert.AreEqual("COLDER", result);
        }
    }
}
