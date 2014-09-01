using System.Diagnostics.CodeAnalysis;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Webs
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WebSourceCallbackHandlerTests
    {

        #region Class/TestInitialize

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
        }

        [TestInitialize]
        public void TestInitialize()
        {
        }

        #endregion

        #region NavigateTo

        [TestMethod]
        public void WebSourceCallbackHandlerNavigateToWithNullArgsExpectedDoesNothing()
        {
            var env = new Mock<IEnvironmentRepository>();
            var handler = new WebSourceCallbackHandlerMock(env.Object);
            Assert.AreEqual(0, handler.StartUriProcessHitCount);
        }

        [TestMethod]
        public void WebSourceCallbackHandlerNavigateToWithInvalidSchemesExpectedDoesNotInvokeStartUriProcess()
        {
            var schemes = new[] { "gopher", "mailto", "file", "ldap", "mailto", "net.pipe", "net.tcp", "news", "nntp", "telnet", "ldap", "uuid" };

            var env = new Mock<IEnvironmentRepository>();
            var handler = new WebSourceCallbackHandlerMock(env.Object);
#pragma warning disable 168
            foreach(var scheme in schemes)
#pragma warning restore 168
            {
                Assert.AreEqual(0, handler.StartUriProcessHitCount);
            }
        }

        #endregion



    }
}
