/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Runtime.ServiceModel.Data;
using Elasticsearch.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Newtonsoft.Json.Linq;
using Warewolf.Auditing.Drivers;
using Warewolf.Interfaces.Auditing;
using Warewolf.Triggers;
using Warewolf.UnitTestAttributes;
using LogLevel = Warewolf.Logging.LogLevel;
using System.Reflection;
using Elastic.Transport;
using System.DirectoryServices.Protocols;

namespace Warewolf.Auditing.Tests
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory("CannotParallelize")]
    public class AuditQueryableElasticTests
    {
        private IAuditQueryable GetAuditQueryablePasswordAuthentication()
        {
            var dependency = new Depends(Depends.ContainerType.Elasticsearch);
            var hostName = "http://" + dependency.Container.IP;
            const string searchIndex = "warewolftestlogs";
            const string username = "test";
            const string password = "test123";

            var mockElasticsearchSource = new Mock<IElasticsearchSource>();
            mockElasticsearchSource.Setup(o => o.HostName).Returns(hostName);
            mockElasticsearchSource.Setup(o => o.Port).Returns(dependency.Container.Port);
            mockElasticsearchSource.Setup(o => o.SearchIndex).Returns(searchIndex);
            mockElasticsearchSource.Setup(o => o.Username).Returns(username);
            mockElasticsearchSource.Setup(o => o.Password).Returns(password);
            mockElasticsearchSource.Setup(o => o.AuthenticationType).Returns(AuthenticationType.Password);

            return new AuditQueryableElastic(mockElasticsearchSource.Object, null);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        [ExpectedException(typeof(TransportException))]
        public void AuditQueryableElastic_Default_Constructor_Failed_InvalidSource()
        {
            var auditQueryable = new AuditQueryableElastic("http://invalid-elastic-source", string.Empty, string.Empty,
                AuthenticationType.Anonymous, string.Empty, string.Empty);
            var query = new Dictionary<string, StringBuilder>();

            _ = auditQueryable.QueryLogData(query);
            Assert.Fail("Invalid Elastic source successfully connected.");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_PasswordAuthentication()
        {
            var auditQueryable = GetAuditQueryablePasswordAuthentication();
            var query = new Dictionary<string, StringBuilder>();
            _ = auditQueryable.QueryLogData(query);
            var match_all = new JObject
            {
                ["$id"] = "1",
                ["$type"] = "Elastic.Clients.Elasticsearch.SearchRequestDescriptor`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]"
            };

            Assert.AreEqual(match_all.ToString(), auditQueryable.Query);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryTriggerData()
        {
            var dependency = new Depends(Depends.ContainerType.AnonymousElasticsearch);
            var hostName = "http://" + dependency.Container.IP;
            const string searchIndex = "warewolftestlogs";
            const string username = "test";
            const string password = "test123";

            var mockElasticsearchSource = new Mock<IElasticsearchSource>();
            mockElasticsearchSource.Setup(o => o.HostName).Returns(hostName);
            mockElasticsearchSource.Setup(o => o.Port).Returns(dependency.Container.Port);
            mockElasticsearchSource.Setup(o => o.SearchIndex).Returns(searchIndex);
            mockElasticsearchSource.Setup(o => o.Username).Returns(username);
            mockElasticsearchSource.Setup(o => o.Password).Returns(password);
            mockElasticsearchSource.Setup(o => o.AuthenticationType).Returns(AuthenticationType.Password);

            var resourceId = Guid.NewGuid();
            var executionId = Guid.NewGuid();
            var customTransactionId = Guid.NewGuid();
            var startDate = DateTime.Now;
            var endDate = startDate.AddMinutes(5);
            var queueRunStatus = QueueRunStatus.Success;

            var executionInfo = new Dictionary<string, object>
            {
                {"ExecutionId", executionId},
                {"CustomTransactionID", customTransactionId},
                {"Success", queueRunStatus},
                {"EndDate", endDate},
                {"Duration", "5"},
                {"StartDate", startDate},
                {"FailureReason", ""},
            };

            var hits = new Dictionary<string, object>
            {
                {"ResourceId", resourceId},
                {"ExecutionInfo", executionInfo},
                {"UserName", username},
                {"Exception", new Exception("This is an exception")},
                {"LogLevel", LogLevel.Debug.ToString()},
                {"AuditType", "LogAdditionalDetail"},
            };

            var values = new Dictionary<string, object>
            {
                {"values", hits}
            };

            var fields = new Dictionary<string, object>
            {
                {"fields", values}
            };

            var mockHit = new Mock<IHit<object>>();
            mockHit.Setup(o => o.Source).Returns(fields);

            var readOnlyCollection = new List<Hit<object>>
            {
                ConvertToHit(mockHit.Object)
            };

            var mockHitsMetadata = new Mock<IHitsMetadata<object>>();
            mockHitsMetadata.Setup(o => o.Hits).Returns(readOnlyCollection);

            var mockElasticClient = new Mock<IElasticsearchClientWrapper>();
            var mock = new Mock<TestElasticsearchClientSettings>();
            mockElasticClient.Setup(o => o.ElasticsearchClientSettings).Returns(mock.Object);

            mockElasticClient.Setup(o => o.Search(It.IsAny<SearchRequest<object>>()))
                .Returns(new SearchResponse<object> { HitsMetadata = ConvertToHitsMetadata(mockHitsMetadata.Object) });

            var auditQueryableElastic =
                new AuditQueryableElastic(mockElasticsearchSource.Object, mockElasticClient.Object.GetUnderlyingClient());

            var query = new Dictionary<string, StringBuilder>
            {
                {"ResourceId", resourceId.ToString().ToStringBuilder()}
            };

            var queryTriggerData = auditQueryableElastic.QueryTriggerData(query);

            var executionHistories = queryTriggerData.ToList();

            Assert.AreEqual(1, executionHistories.Count);
            Assert.AreEqual(resourceId, executionHistories[0].ResourceId);
            Assert.AreEqual(username, executionHistories[0].UserName);
            Assert.AreEqual(executionId, executionHistories[0].ExecutionInfo.ExecutionId);
            Assert.AreEqual(queueRunStatus, executionHistories[0].ExecutionInfo.Success);
            Assert.AreEqual(startDate.ToString(), executionHistories[0].ExecutionInfo.StartDate.ToString());
            Assert.AreEqual(endDate.ToString(), executionHistories[0].ExecutionInfo.EndDate.ToString());
            Assert.AreEqual("5.00:00:00", executionHistories[0].ExecutionInfo.Duration.ToString());

            auditQueryableElastic.Dispose();

            mockElasticsearchSource.Verify(o => o.Dispose(), Times.Once);
        }

        public static Hit<T> ConvertToHit<T>(IHit<T> iHit)
        {
            // Assuming Hit<T> has a constructor that accepts the necessary parameters
            return new Hit<T>
            {
                Source = iHit.Source
                // Set other properties as needed
            };
        }

        public static HitsMetadata<T> ConvertToHitsMetadata<T>(IHitsMetadata<T> iHitsMetadata)
        {
            // Create an instance of HitsMetadata<T> using reflection
            var hitsMetadata = (HitsMetadata<T>)Activator.CreateInstance(typeof(HitsMetadata<T>), true);
        
            // Set the Hits property using reflection
            var hitsProperty = typeof(HitsMetadata<T>).GetProperty("Hits", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            hitsProperty.SetValue(hitsMetadata, iHitsMetadata.Hits);
        
            return hitsMetadata;
        }

        public static IElasticsearchClientSettings ConvertToIElasticsearchClientSettings(TestElasticsearchClientSettings interfaceSettings)
        {
            // Create an instance of ElasticsearchClientSettings using reflection
            var settings = (IElasticsearchClientSettings)Activator.CreateInstance(typeof(IElasticsearchClientSettings), true);

            // Set the DefaultIndex property using reflection
            var defaultIndexProperty = typeof(IElasticsearchClientSettings).GetProperty("DefaultIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            defaultIndexProperty.SetValue(settings, interfaceSettings.DefaultIndex);

            return settings;
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AuditQueryableElastic))]
        public void AuditQueryableElastic_QueryLogData()
        {
            var dependency = new Depends(Depends.ContainerType.AnonymousElasticsearch);
            var hostName = "https://" + dependency.Container.IP;
            const string searchIndex = "warewolftestlogs";
            const string username = "test";
            const string password = "test123";

            var mockElasticsearchSource = new Mock<IElasticsearchSource>();
            mockElasticsearchSource.Setup(o => o.HostName).Returns(hostName);
            mockElasticsearchSource.Setup(o => o.Port).Returns(dependency.Container.Port);
            mockElasticsearchSource.Setup(o => o.SearchIndex).Returns(searchIndex);
            mockElasticsearchSource.Setup(o => o.Username).Returns(username);
            mockElasticsearchSource.Setup(o => o.Password).Returns(password);
            mockElasticsearchSource.Setup(o => o.AuthenticationType).Returns(AuthenticationType.Password);

            var executionId = Guid.NewGuid();
            var serverId = Guid.NewGuid();
            var parentId = Guid.NewGuid();
            var customTransactionId = Guid.NewGuid();
            var auditDate = DateTime.Now;
            var startDate = DateTime.Now;
            var endDate = startDate.AddMinutes(5);
            const string workflowName = "workflowName";
            const string executingUser = "executingUser";
            const string url = "http:localhost.net";
            const string environment = "environment";
            const string auditType = "LogResumeExecutionState";
            var logLevel = LogLevel.Debug.ToString();

            var hits = new Dictionary<string, object>
            {
                {"ExecutionID", executionId},
                {"CustomTransactionID", customTransactionId},
                {"WorkflowName", workflowName},
                {"ExecutingUser", executingUser},
                {"Url", url},
                {"Environment", environment},
                {"AuditDate", auditDate},
                {"Exception", new Exception("This is an exception")},
                {"AuditType", auditType},
                {"LogLevel", logLevel},
                {"IsSubExecution", false.ToString()},
                {"IsRemoteWorkflow", false.ToString()},
                {"ServerID", serverId},
                {"ParentID", parentId},
            };

            var values = new Dictionary<string, object>
            {
                {"values", hits}
            };

            var fields = new Dictionary<string, object>
            {
                {"fields", values}
            };

            var mockHit = new Mock<IHit<object>>();
            mockHit.Setup(o => o.Source).Returns(fields);

            var readOnlyCollection = new List<Hit<object>>
            {
                ConvertToHit(mockHit.Object)
            };

            var mockHitsMetadata = new Mock<IHitsMetadata<object>>();
            mockHitsMetadata.Setup(o => o.Hits).Returns(readOnlyCollection);

            var mockElasticClient = new Mock<ElasticsearchClient>();
            mockElasticClient.Setup(o => o.Search<object>(It.IsAny<SearchRequestDescriptor<object>>()))
                .Returns(new SearchResponse<object> { HitsMetadata = ConvertToHitsMetadata(mockHitsMetadata.Object) });

            var auditQueryableElastic =
                new AuditQueryableElastic(mockElasticsearchSource.Object, mockElasticClient.Object);

            var query = new Dictionary<string, StringBuilder>
            {
                {"StartDateTime", startDate.ToString().ToStringBuilder()},
                {"CompletedDateTime", endDate.ToString().ToStringBuilder()},
                {"EventLevel", "Debug".ToStringBuilder()},
                {"ExecutionID", executionId.ToString().ToStringBuilder()},
            };

            var queryTriggerData = auditQueryableElastic.QueryLogData(query);

            var audits = queryTriggerData.ToList();

            Assert.AreEqual(1, audits.Count);
            Assert.AreEqual(executionId.ToString(), audits[0].ExecutionID);
            Assert.AreEqual(customTransactionId.ToString(), audits[0].CustomTransactionID);
            Assert.AreEqual(workflowName, audits[0].WorkflowName);
            Assert.AreEqual(executingUser, audits[0].ExecutingUser);
            Assert.AreEqual(auditDate.ToString(), audits[0].AuditDate.ToString());
            Assert.AreEqual(auditType, audits[0].AuditType);
            Assert.AreEqual(logLevel, audits[0].LogLevel.ToString());
            Assert.IsFalse(audits[0].IsSubExecution);
            Assert.IsFalse(audits[0].IsRemoteWorkflow);
            Assert.AreEqual(serverId.ToString(), audits[0].ServerID);
            Assert.AreEqual(parentId.ToString(), audits[0].ParentID);
        }
    }

    public interface IHit<T>
    {
        T Source { get; }
    }

    public class HitWrapper<T> : IHit<T>
    {
        private readonly Hit<T> _hit;

        public HitWrapper(Hit<T> hit)
        {
            _hit = hit;
        }

        public T Source => _hit.Source;
    }

    public interface IHitsMetadata<T>
    {
        IReadOnlyCollection<Hit<T>> Hits { get; }
    }

    public class HitsMetadataWrapper<T> : IHitsMetadata<T>
    {
        private readonly HitsMetadata<T> _hitsMetadata;

        public HitsMetadataWrapper(HitsMetadata<T> hitsMetadata)
        {
            _hitsMetadata = hitsMetadata;
        }

        public IReadOnlyCollection<Hit<T>> Hits => _hitsMetadata.Hits;
    }

    public interface ISearchResponse<T>
    {
        IHitsMetadata<T> HitsMetadata { get; }
    }

    public class SearchResponseWrapper<T> : ISearchResponse<T>
    {
        private readonly SearchResponse<T> _searchResponse;

        public SearchResponseWrapper(SearchResponse<T> searchResponse)
        {
            _searchResponse = searchResponse;
        }

        public IHitsMetadata<T> HitsMetadata => new HitsMetadataWrapper<T>(_searchResponse.HitsMetadata);
    }

    public interface IElasticsearchClientWrapper
    {
        public object ElasticsearchClientSettings { get; set; }

        IElasticsearchClientSettings GetSettings();
        ElasticsearchClient GetUnderlyingClient();
        SearchResponse<T> Search<T>(SearchRequest<T> request);
    }

    public class ElasticsearchClientWrapper : IElasticsearchClientWrapper
    {
        private readonly ElasticsearchClient _client;

        public ElasticsearchClientWrapper(ElasticsearchClient client)
        {
            _client = client;
        }

        object IElasticsearchClientWrapper.ElasticsearchClientSettings { get => GetSettings(); set => throw new NotImplementedException(); }

        public IElasticsearchClientSettings GetSettings()
        {
            return _client.ElasticsearchClientSettings;
        }

        public SearchResponse<T> Search<T>(SearchRequest<T> request)
        {
            return _client.Search<T>(request);
        }

        public ElasticsearchClient GetUnderlyingClient()
        {
            return _client;
        }
    }

    public interface TestElasticsearchClientSettings
    {
        string DefaultIndex { get; }
    }

    public class ElasticsearchClientSettings : TestElasticsearchClientSettings
    {
        public string DefaultIndex { get; set; }
    }
}