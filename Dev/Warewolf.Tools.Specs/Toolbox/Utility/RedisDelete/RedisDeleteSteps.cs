using Dev2.Activities.RedisDelete;
using Dev2.Common.Serializers;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Warewolf.Driver.Redis;
using Warewolf.Storage;

namespace Warewolf.Tools.Specs.Toolbox.Utility.RedisDelete
{
    [Binding]
    public class RedisDeleteSteps
    {
        readonly ScenarioContext _scenarioContext;

        private static SpecRedisDeleteActivity GetRedisDeleteActivity(IResourceCatalog resourceCatalog, string key, string hostName, RedisCacheImpl impl)
        {
            return new SpecRedisDeleteActivity(resourceCatalog, impl)
            {
                Key = key,
                RedisSource = new Dev2.Data.ServiceModel.RedisSource { HostName = hostName },
            };
        }

        [When(@"I execute the delete tool")]
        [Then(@"I execute the delete tool")]
        public void WhenIExecuteTheDeleteTool()
        {
            var redisActivityOld = _scenarioContext.Get<SpecRedisDeleteActivity>(nameof(RedisDeleteActivity));
            var dataToStore = _scenarioContext.Get<Dictionary<string, string>>("dataToStore");
            var hostName = _scenarioContext.Get<string>("hostName");
            var impl = _scenarioContext.Get<RedisCacheImpl>(nameof(RedisCacheImpl));

            GenResourceAndDataobject(redisActivityOld.Key, hostName, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataobject, out ExecutionEnvironment environment);

            ExecuteDeleteTool(redisActivityOld, mockDataobject);
        }

        [Then(@"The ""(.*)"" Cache exists")]
        public void CacheExists(string key)
        {
            var hostName = _scenarioContext.Get<string>("hostName");
            var redisImpl = GetRedisCacheImpl(hostName);

            var actualCachedData = GetCachedData(redisImpl, key);
            Assert.IsNotNull(actualCachedData, key + ": Cache does not exists");
        }

        [Then(@"I have an existing key to delete ""(.*)""")]
        public void GivenIHaveAKeyToDelete(string key)
        {
            var hostName = _scenarioContext.Get<string>("hostName");
            var redisImpl = GetRedisCacheImpl(hostName);
            GenResourceAndDataobject(key, hostName, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataobject, out ExecutionEnvironment environment);

            var redisDeleteActivityNew = GetRedisDeleteActivity(mockResourceCatalog.Object, key, hostName, redisImpl);

            _scenarioContext.Add(nameof(RedisDeleteActivity), redisDeleteActivityNew);

            Assert.IsNotNull(redisDeleteActivityNew.Key);
        }

        [Then(@"The ""(.*)"" Cache has been deleted")]
        public void CacheHasBeenDeleted(string key)
        {
            var hostName = _scenarioContext.Get<string>("hostName");
            var redisImpl = GetRedisCacheImpl(hostName);

            var actualCachedData = GetCachedData(redisImpl, key);
            Assert.IsNull(actualCachedData, key + ": Cache still exists");
        }

        [Then(@"I add another key ""(.*)""")]
        public void ThenIAddAnotherKey(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"another assign ""(.*)"" as")]
        public void ThenAnotherAssignAs(string p0, Table table)
        {
            ScenarioContext.Current.Pending();
        }

        private RedisCacheImpl GetRedisCacheImpl(string hostName)
        {
            return new RedisCacheImpl(hostName, 6379, "");
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
        private static void ExecuteDeleteTool(SpecRedisDeleteActivity redisDeleteActivity, Mock<IDSFDataObject> mockDataobject)
        {
            redisDeleteActivity.SpecExecuteTool(mockDataobject.Object);
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
