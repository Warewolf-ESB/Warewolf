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
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Common;
using Warewolf.Configuration;
using Warewolf.Data;
using Warewolf.Driver.Serilog;
using Warewolf.Logging;
using Warewolf.Security.Encryption;
using Warewolf.UnitTestAttributes;

namespace Warewolf.Logger.Tests
{
    [TestClass]
    public class WarewolfLoggerTests
    {
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
            Assert.IsNotNull(context.Source);
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

        private static ILoggerContext ConstructLoggerContext(IArgs args, out ElasticsearchSource elasticsearchSource)
        {
            var mockArgs = new Mock<IArgs>();
            mockArgs.Setup(o => o.ServerEndpoint).Returns(args.ServerEndpoint);
            mockArgs.Setup(o => o.Verbose).Returns(args.Verbose);
            var dependency = new Depends(Depends.ContainerType.AnonymousElasticsearch);
            var hostName = "http://" + dependency.Container.IP;

            elasticsearchSource = new ElasticsearchSource
            {
                ResourceID = Guid.Empty,
                ResourceType = "ElasticsearchSource",
                AuthenticationType = AuthenticationType.Anonymous,
                Port = dependency.Container.Port,
                HostName = hostName,
                SearchIndex = "warewolflogstests"
            };

            var serializer = new Dev2JsonSerializer();
            var payload = serializer.Serialize(elasticsearchSource );
            var encryptedPayload = DpapiWrapper.Encrypt(payload);
            var data = new AuditingSettingsData
            {
                LoggingDataSource = new NamedGuidWithEncryptedPayload
                {
                    Name = "Testing Elastic Data Source",
                    Value = Guid.Empty,
                    Payload = encryptedPayload
                }
            };
            Config.Auditing.LoggingDataSource = data.LoggingDataSource;
            Config.Server.Sink = nameof(AuditingSettingsData);
            return new LoggerContext(mockArgs.Object);
        }
    }
}