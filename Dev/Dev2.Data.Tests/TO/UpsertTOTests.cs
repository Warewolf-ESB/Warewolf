using Dev2.Data.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Operations
{
    [TestClass]
    public class UpsertTOTests
    {                               
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenReplaceFolderWithC_Dev2ReplaceOperation_Replace_ShouldReturnColder()
        {
            const string someexpression = "SomeExpression";
            const string somepayload = "SomePayLoad";
            UpsertTO upsertTo = new UpsertTO(someexpression, somepayload);
            Assert.IsNotNull(upsertTo);
            Assert.AreEqual(someexpression, upsertTo.Expression);
            Assert.AreEqual(somepayload, upsertTo.Payload);
        }
    }
}
