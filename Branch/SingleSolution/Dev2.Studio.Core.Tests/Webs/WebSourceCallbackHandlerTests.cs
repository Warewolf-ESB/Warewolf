using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            Assert.AreEqual(0, handler.StartUriProcessHitCount);
        }

        [TestMethod]
        public void WebSourceCallbackHandlerNavigateToWithInvalidSchemesExpectedDoesNotInvokeStartUriProcess()
        {
            var schemes = new[] { "gopher", "mailto", "file", "ldap", "mailto", "net.pipe", "net.tcp", "news", "nntp", "telnet", "ldap", "uuid" };

            var env = new Mock<IEnvironmentRepository>();
            var handler = new WebSourceCallbackHandlerMock(env.Object);
            foreach(var scheme in schemes)
            {
                Assert.AreEqual(0, handler.StartUriProcessHitCount);
            }
        }

        #endregion



    }
}
