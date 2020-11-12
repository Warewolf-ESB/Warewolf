using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Operations
{
    [TestClass]
    public class Dev2PutOperationTOTests
    {
        [TestMethod]
        public void Dev2PutOperationTO_Should()
        {
            var operationToFactory = new Dev2PutOperationTOFactory();
            var dev2PutOperationTo = operationToFactory.CreateDev2PutOperationTO(true, "SomeContent", true, true);
            Assert.IsNotNull(dev2PutOperationTo);
            Assert.IsTrue(dev2PutOperationTo.Append);
            Assert.IsFalse(string.IsNullOrEmpty(dev2PutOperationTo.FileContents));
            Assert.IsTrue(dev2PutOperationTo.FileContentsAsBase64);
        }
    }
}
