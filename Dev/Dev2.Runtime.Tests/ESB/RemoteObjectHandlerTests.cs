using System.Diagnostics.CodeAnalysis;
using Dev2.Services.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ESB
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RemoteObjectHandlerTests
    {

        [TestMethod]
        public void RemoteObjectHandlerFormatResultWithNullArgumentExpectedReturnsEmptyFormat()
        {
            var actual = RemoteObjectHandler.FormatResult(null, "<z:anyType xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:d1p1=\"http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Ouput\" i:type=\"d1p1:OutputDescription\" xmlns:z=\"http://schemas.microsoft.com/2003/10/Serialization/\"><d1p1:DataSourceShapes xmlns:d2p1=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\"><d2p1:anyType i:type=\"d1p1:DataSourceShape\"><d1p1:Paths /></d2p1:anyType></d1p1:DataSourceShapes><d1p1:Format>ShapedXML</d1p1:Format></z:anyType>");

            Assert.AreEqual("<ADL />", actual);
        }
    }
}
