
using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Dev2.Activities.Redis;
using Dev2.Activities.RedisDelete;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Serializers;
using Dev2.Infrastructure.Tests;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Threading;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Driver.Redis;
using Warewolf.Storage;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;

namespace Warewolf.Tools.Specs.Toolbox.Utility.Redis
{
    [Binding]
    public class RedisGetSetSteps
    {
        readonly ScenarioContext _scenarioContext;
        public static Stopwatch Stoptime { get; set; }

        private void SetUpRedisSourceViewModel(string hostName)
        {
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>("view");
            var mockStudioUpdateManager = new Mock<IRedisSourceModel>();
            mockStudioUpdateManager.Setup(model => model.ServerName).Returns(hostName);
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();

            var username = @"dev2\IntegrationTester";
            var password = TestEnvironmentVariables.GetVar(username);
            var redisSourceDefinition = new RedisSourceDefinition
            {
                Name = "Test-Redis",
                HostName = "http://RSAKLFSVRTFSBLD/IntegrationTestSite",
                Password = password,
                Port = "6379"
            };
            //Test Locally
            //var redisSourceDefinition = new RedisSourceDefinition
            //{
            //    Name = "localhost",
            //    HostName = "127.0.0.1",
            //    Password = "",
            //    Port = "6379"
            //};
            mockStudioUpdateManager.Setup(model => model.FetchSource(It.IsAny<Guid>()))
                .Returns(redisSourceDefinition);
            var redisSourceViewModel = new RedisSourceViewModel(mockStudioUpdateManager.Object, mockEventAggregator.Object, redisSourceDefinition, new SynchronousAsyncWorker(), mockExecutor.Object);
            redisSourceControl.DataContext = redisSourceViewModel;

            _scenarioContext.Remove("viewModel");
            _scenarioContext.Add("viewModel", redisSourceViewModel);

            _scenarioContext.Add("hostName", hostName);
        }

        private IDictionary<string, string> GetCachedData(RedisCacheImpl impl, string key)
        {
            var actualCacheData = impl.Get(key);

            if (actualCacheData != null)
            {
                var serializer = new Dev2JsonSerializer();
                return serializer.Deserialize<IDictionary<string, string>>(actualCacheData);
            }
            return null;
        }

        private static void GetExpectedTableData(Table table, int rowNumber, out string expectedKey, out string expectedValue)
        {
            var expectedRow = table.Rows[rowNumber].Values.ToList();

            expectedKey = expectedRow[0];
            expectedValue = expectedRow[1];
        }

        [Given(@"Redis source ""(.*)""")]
        public void GivenRedisSource(string hostName)
        {
            SetUpRedisSourceViewModel(hostName);
        }

        [Then(@"I have a key ""(.*)""")]
        [Given(@"I have a key ""(.*)""")]
        public void GivenIHaveAKey(string key)
        {
            var hostName = _scenarioContext.Get<string>("hostName");
            var redisImpl = GetRedisCacheImpl(hostName);
            GenResourceAndDataobject(key, hostName, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataobject, out ExecutionEnvironment environment);

            var assignActivity = new DsfMultiAssignActivity();
            var ttl = 3000;
            var redisActivityNew = GetRedisActivity(mockResourceCatalog.Object, key, ttl, hostName, redisImpl, assignActivity);

            _scenarioContext.Add(nameof(RedisActivity), redisActivityNew);
            _scenarioContext.Add(nameof(RedisCacheImpl), redisImpl);
            _scenarioContext.Add(nameof(ttl), ttl);

            Assert.IsNotNull(redisActivityNew.Key);
        }

        [Given(@"No data in the cache")]
        public void GivenNoDataInTheCache()
        {
            var redisActivityOld = _scenarioContext.Get<SpecRedisActivity>(nameof(RedisActivity));
            var impl = _scenarioContext.Get<RedisCacheImpl>(nameof(RedisCacheImpl));
            var ttl = _scenarioContext.Get<int>("ttl");

            var environment = new ExecutionEnvironment();

            var key = environment.EvalToExpression(redisActivityOld.Key, 0);

            do
            {
                Thread.Sleep(1000);
            } while (Stoptime.ElapsedMilliseconds < ttl);

            var actualCachedData = GetCachedData(impl, key);
            Assert.IsNull(actualCachedData);
        }

        [Given(@"an assign ""(.*)"" as")]
        [Then(@"an assign ""(.*)"" as")]
        public void GivenAnAssignAs(string data, Table table)
        {
            var redisActivity = _scenarioContext.Get<SpecRedisActivity>(nameof(RedisActivity));

            var assignActivity = GetDsfMultiAssignActivity("[[Var1]]", "Test1");

            redisActivity.ActivityFunc = new ActivityFunc<string, bool> { Handler = assignActivity };

            var assignOutputs = assignActivity.GetForEachOutputs();

            GetExpectedTableData(table, 0, out string expectedKey, out string expectedValue);

            Assert.AreEqual(expectedKey, assignOutputs[0].Value);
            Assert.IsTrue(expectedValue.Contains(assignOutputs[0].Name));

            var dic = new Dictionary<string, string> { { assignOutputs[0].Value, assignOutputs[0].Name } };

            _scenarioContext.Remove(nameof(RedisActivity));
            _scenarioContext.Add(nameof(RedisActivity), redisActivity);
            _scenarioContext.Add(data, dic);
            _scenarioContext.Add(nameof(DsfMultiAssignActivity), assignActivity);

        }

        [Given(@"data exists \(TTL not hit\) for key ""(.*)"" as")]
        public void GivenDataExistsTTLNotHitForKeyAs(string key, Table table)
        {
            var hostName = _scenarioContext.Get<string>("hostName");
            var redisImpl = GetRedisCacheImpl(hostName);

            GenResourceAndDataobject(key, hostName, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataobject, out ExecutionEnvironment environment);

            var dataStored = new Dictionary<string, string> { { "[[Var1]]", "Data in cache" } };

            var assignActivity = GetDsfMultiAssignActivity(dataStored.Keys.ToArray()[0], dataStored.Values.ToArray()[0]);

            var ttl = 3000;
            var redisActivityNew = GetRedisActivity(mockResourceCatalog.Object, key, ttl, hostName, redisImpl, assignActivity);

            ExecuteGetSetTool(redisActivityNew, mockDataobject);

            var sdfsf = redisActivityNew.SpecPerformExecution(dataStored);

            var actualDataStored = GetCachedData(redisImpl, key);

            GetExpectedTableData(table, 0, out string expectedKey, out string expectedValue);

            Assert.AreEqual(expectedKey, key);
            Assert.IsTrue(expectedValue.Contains(actualDataStored.Keys.ToArray()[0]));
            Assert.IsTrue(expectedValue.Contains(actualDataStored.Values.ToArray()[0]));

            _scenarioContext.Add(redisActivityNew.Key, actualDataStored);
            _scenarioContext.Remove(nameof(RedisActivity));
            _scenarioContext.Add(nameof(RedisActivity), redisActivityNew);
        }

        [Given(@"data does not exist \(TTL exceeded\) for key ""(.*)"" as")]
        public void GivenDataDoesNotExistTTLExceededForKeyAs(string p0, Table table)
        {
            var ttl = _scenarioContext.Get<int>("ttl");
            var redisActivity = _scenarioContext.Get<RedisActivity>(nameof(RedisActivity));
            var impl = _scenarioContext.Get<RedisCacheImpl>(nameof(RedisCacheImpl));

            do
            {
                Thread.Sleep(1000);
            } while (Stoptime.ElapsedMilliseconds < ttl);

            var actualCachedData = GetCachedData(impl, redisActivity.Key);

            Assert.IsNull(actualCachedData);
        }

        [Then(@"I execute the get/set tool")]
        [When(@"I execute the get/set tool")]
        public void WhenIExecuteTheTool()
        {
            var redisActivityOld = _scenarioContext.Get<SpecRedisActivity>(nameof(RedisActivity));
            var dataToStore = _scenarioContext.Get<Dictionary<string, string>>("dataToStore");
            var assignActivity = _scenarioContext.Get<DsfMultiAssignActivity>(nameof(DsfMultiAssignActivity));
            var hostName = _scenarioContext.Get<string>("hostName");
            var impl = _scenarioContext.Get<RedisCacheImpl>(nameof(RedisCacheImpl));

            GenResourceAndDataobject(redisActivityOld.Key, hostName, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataobject, out ExecutionEnvironment environment);

            ExecuteGetSetTool(redisActivityOld, mockDataobject);
        }

        [Then(@"the cache will contain")]
        public void ThenTheCacheWillContain(Table table)
        {
            var redisActivity = _scenarioContext.Get<SpecRedisActivity>(nameof(RedisActivity));
            var impl = _scenarioContext.Get<RedisCacheImpl>(nameof(RedisCacheImpl));

            var actualCacheData = GetCachedData(impl, redisActivity.Key);

            GetExpectedTableData(table, 0, out string expectedKey, out string expectedValue);

            Assert.IsTrue(expectedValue.Contains(actualCacheData.Keys.ToList()[0]));
            Assert.IsTrue(expectedValue.Contains(actualCacheData.Values.ToList()[0]));

            _scenarioContext.Add(redisActivity.Key, actualCacheData);
        }

        [Then(@"output variables have the following values")]
        public void ThenOutputVariablesHaveTheFollowingValues(Table table)
        {
            var redisActivity = _scenarioContext.Get<RedisActivity>(nameof(RedisActivity));
            var impl = _scenarioContext.Get<RedisCacheImpl>(nameof(RedisCacheImpl));
            var actualCacheData = GetCachedData(impl, redisActivity.Key);

            GetExpectedTableData(table, 0, out string expectedKey, out string expectedValue);

            Assert.AreEqual(expected: expectedKey, actual: actualCacheData.Keys.ToArray()[0]);
            Assert.IsTrue(expectedValue.Contains(actualCacheData.Values.ToArray()[0]));

        }

        [Then(@"the assign ""(.*)"" is not executed")]
        public void ThenTheAssignIsNotExecuted(string key)
        {
            var dataToStore = _scenarioContext.Get<IDictionary<string, string>>(key);

            Assert.IsNotNull(dataToStore);
        }

        [Then(@"the assign ""(.*)"" is executed")]
        public void ThenTheAssignIsExecuted(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        private static void GenResourceAndDataobject(string key, string hostName, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataobject, out ExecutionEnvironment environment)
        {
            mockResourceCatalog = new Mock<IResourceCatalog>();
            mockDataobject = new Mock<IDSFDataObject>();
            var redisSource = new Dev2.Data.ServiceModel.RedisSource { HostName = hostName };
            mockResourceCatalog.Setup(o => o.GetResource<Dev2.Data.ServiceModel.RedisSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(redisSource);

            environment = new ExecutionEnvironment();
            environment.Assign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>());
            environment.EvalToExpression(key, 0);

            mockDataobject.Setup(o => o.IsDebugMode()).Returns(true);
            mockDataobject.Setup(o => o.Environment).Returns(environment);
        }

        private DsfMultiAssignActivity GetDsfMultiAssignActivity(string fieldName, string fieldValue)
        {
            return new DsfMultiAssignActivity() { FieldsCollection = new List<ActivityDTO> { new ActivityDTO(fieldName, fieldValue, 1) } };
        }

        private RedisCacheImpl GetRedisCacheImpl(string hostName)
        {
            return new RedisCacheImpl(hostName, 6379, "");
        }

        private static SpecRedisActivity GetRedisActivity(IResourceCatalog resourceCatalog, string key, int ttl, string hostName, RedisCacheImpl impl, DsfMultiAssignActivity assignActivity)
        {
            Stoptime = Stopwatch.StartNew();
            return new SpecRedisActivity(resourceCatalog, impl)
            {
                Key = key,
                ActivityFunc = new ActivityFunc<string, bool> { Handler = assignActivity },
                TTL = ttl,
                RedisSource = new Dev2.Data.ServiceModel.RedisSource { HostName = hostName },
            };
        }
        private static SpecRedisDeleteActivity GetRedisDeleteActivity(IResourceCatalog resourceCatalog, string key, string hostName, RedisCacheImpl impl)
        {
            return new SpecRedisDeleteActivity(resourceCatalog, impl)
            {
                Key = key,
                RedisSource = new Dev2.Data.ServiceModel.RedisSource { HostName = hostName },
            };
        }


        private static void ExecuteGetSetTool(SpecRedisActivity redisActivity, Mock<IDSFDataObject> mockDataobject)
        {
            redisActivity.SpecExecuteTool(mockDataobject.Object);
        }
        private static void ExecuteDeleteTool(SpecRedisDeleteActivity redisDeleteActivity, Mock<IDSFDataObject> mockDataobject)
        {
            redisDeleteActivity.SpecExecuteTool(mockDataobject.Object);
        }
        class SpecRedisActivity : RedisActivity
        {
            public SpecRedisActivity()
            {
            }

            public SpecRedisActivity(IResourceCatalog resourceCatalog, RedisCacheBase redisCache) : base(resourceCatalog, redisCache)
            {
            }

            public List<string> SpecPerformExecution(Dictionary<string, string> evaluatedValues)
            {
                return base.PerformExecution(evaluatedValues);
            }

            public void SpecExecuteTool(IDSFDataObject dataObject)
            {
                base.ExecuteTool(dataObject, 0);
            }

        }

        class SpecRedisDeleteActivity : RedisDeleteActivity
        {
            public SpecRedisDeleteActivity()
            {
            }

            public SpecRedisDeleteActivity(IResourceCatalog resourceCatalog, RedisCacheBase redisCache) : base(resourceCatalog, redisCache)
            {
            }

            public List<string> SpecPerformExecution(Dictionary<string, string> evaluatedValues)
            {
                return base.PerformExecution(evaluatedValues);
            }

            public void SpecExecuteTool(IDSFDataObject dataObject)
            {
                base.ExecuteTool(dataObject, 0);
            }

        }
    }
}
