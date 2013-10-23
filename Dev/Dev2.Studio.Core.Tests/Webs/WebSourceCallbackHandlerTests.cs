using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Webs.Callbacks;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Moq;

namespace Dev2.Core.Tests.Webs
{
    [TestClass][ExcludeFromCodeCoverage]
    public class WebSourceCallbackHandlerTests
    {
        static ImportServiceContext _importContext;

        private static Mock<IEventAggregator> _eventAgrregator;

        #region Class/TestInitialize

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _importContext = new ImportServiceContext();
            ImportService.CurrentContext = _importContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullTestAggregateCatalog()
            });
            ImportService.AddExportedValueToContainer<IFrameworkSecurityContext>(new MockSecurityProvider(""));
            _eventAgrregator = new Mock<IEventAggregator>();
            ImportService.AddExportedValueToContainer(_eventAgrregator.Object);

        }

        [TestInitialize]
        public void TestInitialize()
        {
            ImportService.CurrentContext = _importContext;
        }

        #endregion

        #region NavigateTo

        [TestMethod]
        public void WebSourceCallbackHandlerNavigateToWithNullArgsExpectedDoesNothing()
        {
            var env = new Mock<IEnvironmentRepository>();
            var handler = new WebSourceCallbackHandlerMock(env.Object);
            handler.NavigateTo(null, null, null);
            Assert.AreEqual(0, handler.StartUriProcessHitCount);
        }

        [TestMethod]
        public void WebSourceCallbackHandlerNavigateToWithValidSchemesExpectedInvokesStartUriProcess()
        {
            var expectedCount = 0;

            var env = new Mock<IEnvironmentRepository>();
            var handler = new WebSourceCallbackHandlerMock(env.Object);
            foreach(var scheme in WebSourceCallbackHandler.ValidSchemes)
            {
                expectedCount++;
                handler.NavigateTo(string.Format("{0}://localhost/webservice", scheme), null, null);
                Assert.AreEqual(expectedCount, handler.StartUriProcessHitCount);
            }
        }

        [TestMethod]
        public void WebSourceCallbackHandlerNavigateToWithInvalidSchemesExpectedDoesNotInvokeStartUriProcess()
        {
            var schemes = new[] { "gopher", "mailto", "file", "ldap", "mailto", "net.pipe", "net.tcp", "news", "nntp", "telnet", "ldap", "uuid" };

            var env = new Mock<IEnvironmentRepository>();
            var handler = new WebSourceCallbackHandlerMock(env.Object);
            foreach(var scheme in schemes)
            {
                handler.NavigateTo(string.Format("{0}://localhost/webservice", scheme), null, null);
                Assert.AreEqual(0, handler.StartUriProcessHitCount);
            }
        }

        #endregion



    }
}
