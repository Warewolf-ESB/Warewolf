/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.DB;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Common;
using Warewolf.Driver.Serilog;
using Warewolf.Logging;
using Warewolf.UnitTestAttributes;

namespace Warewolf.Logger.Tests
{
    [TestClass]
    public class WarewolfLoggerTests
    {
        private static readonly Guid _sourceId = Guid.Parse("24e12ae4-58b6-4fec-b521-48493230fef7");

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfLogger))]
        public void WorkerContext_GivenValidConstruct_ExpectValidSource()
        {
            var args = new Args
            {
                Verbose = true,
                ServerEndpoint = new Uri("http://somehost:1234")
            };
            var context = ConstructLoggerContext(args, out var source);
            Assert.AreEqual(source, context.Source);
            Assert.IsTrue(context.Verbose);
        }


        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LoggerContext))]
        public void LoggerContext_Contructor_Verbose_IsFalse()
        {
            var args = new Args
            {
                Verbose = false,
                ServerEndpoint = new Uri("http://somehost:1234")
            };
            var context = ConstructLoggerContext(args, out var source);
            Assert.IsNotNull(source);
            Assert.IsFalse(context.Verbose);
        }

        private static ILoggerContext ConstructLoggerContext(IArgs args, out SerilogElasticsearchSource elasticsearchSource)
        {
            var mockArgs = new Mock<IArgs>();
            mockArgs.Setup(o => o.ServerEndpoint).Returns(args.ServerEndpoint);
            mockArgs.Setup(o => o.Verbose).Returns(args.Verbose);
            var dependency = new Depends(Depends.ContainerType.Elasticsearch);
            var hostName = "http://" + dependency.Container.IP;
            elasticsearchSource = new SerilogElasticsearchSource
            {
                ResourceID = _sourceId,
                HostName = hostName,
                Port = dependency.Container.Port,
                ResourceName = "TestSource",
                SearchIndex = "warewolftestlogs"
            };
            var mockResourceCatalogProxy = new Mock<IResourceCatalogProxy>();
            mockResourceCatalogProxy.Setup(o => o.GetResourceById<SerilogElasticsearchSource>(GlobalConstants.ServerWorkspaceID, _sourceId)).Returns(elasticsearchSource);
            return new LoggerContext(mockArgs.Object, mockResourceCatalogProxy.Object);
        }
    }
}