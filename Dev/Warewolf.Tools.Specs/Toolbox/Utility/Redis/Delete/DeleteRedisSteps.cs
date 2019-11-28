/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Activities.Redis;
using Dev2.Activities.RedisDelete;
using Dev2.Common.Serializers;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Driver.Redis;
using Warewolf.Storage;

namespace Warewolf.Tools.Specs.Toolbox.Utility.Redis.Delete
{
    [Binding]
    public class RedisDeleteSteps
    {

        readonly ScenarioContext _scenarioContext;
        public RedisDeleteSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this._scenarioContext = scenarioContext;
        }

        public static Stopwatch Stoptime { get; set; }


        [When(@"I execute the delete tool")]
        [Then(@"I execute the delete tool")]
        public void WhenIExecuteTheDeleteTool()
        {
            var redisActivityOld = _scenarioContext.Get<SpecRedisDeleteActivity>(nameof(RedisDeleteActivity));
            var dataToStore = _scenarioContext.Get<Dictionary<string, string>>("dataToStore");
            var hostName = _scenarioContext.Get<string>("hostName");
            var password = _scenarioContext.Get<string>("password");
            var port = _scenarioContext.Get<int>("port");
            var impl = _scenarioContext.Get<RedisCacheImpl>(nameof(RedisCacheImpl));

            GenResourceAndDataobject(redisActivityOld.Key, hostName, password, port, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataobject, out ExecutionEnvironment environment);

            ExecuteDeleteTool(redisActivityOld, mockDataobject);
        }

        [Then(@"The ""(.*)"" Cache exists")]
        public void CacheExists(string key)
        {
            var hostName = _scenarioContext.Get<string>("hostName");
            var password = _scenarioContext.Get<string>("password");
            var port = _scenarioContext.Get<int>("port");
            var redisImpl = GetRedisCacheImpl(hostName, password, port);

            var actualCachedData = GetCachedData(redisImpl, key);
            Assert.IsNotNull(actualCachedData, key + ": Cache does not exists");
        }

        [Then(@"I have an existing key to delete ""(.*)""")]
        public void ThenIHaveAnExistingKeyToDelete(string key)
        {
            var hostName = _scenarioContext.Get<string>("hostName");
            var password = _scenarioContext.Get<string>("password");
            var port = _scenarioContext.Get<int>("port");
            var redisImpl = GetRedisCacheImpl(hostName, password, port);
            GenResourceAndDataobject(key, hostName, password, port, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataobject, out ExecutionEnvironment environment);

            var redisDeleteActivityNew = GetRedisDeleteActivity(mockResourceCatalog.Object, key, hostName, redisImpl);

            _scenarioContext.Add(nameof(RedisDeleteActivity), redisDeleteActivityNew);

            Assert.IsNotNull(redisDeleteActivityNew.Key);
        }

        [Then(@"The ""(.*)"" Cache has been deleted")]
        public void ThenTheCacheHasBeenDeleted(string key)
        {
            var hostName = _scenarioContext.Get<string>("hostName");
            var password = _scenarioContext.Get<string>("password");
            var port = _scenarioContext.Get<int>("port");
            var redisImpl = GetRedisCacheImpl(hostName, password, port);

            var actualCachedData = GetCachedData(redisImpl, key);
            Assert.IsNull(actualCachedData, key + ": Cache still exists");
        }

        [Then(@"I add another key ""(.*)""")]
        [Given(@"I add another key ""(.*)""")]
        public void GivenIAddAnotherKey(string key)
        {
            var hostName = _scenarioContext.Get<string>("hostName");
            var password = _scenarioContext.Get<string>("password");
            var port = _scenarioContext.Get<int>("port");
            var ttl = _scenarioContext.Get<int>("ttl");

            var redisImpl = GetRedisCacheImpl(hostName, password, port);
            GenResourceAndDataobject(key, hostName, password, port, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataobject, out ExecutionEnvironment environment);

            var assignActivity = new DsfMultiAssignActivity();
            var redisActivityNew = GetRedisActivity(mockResourceCatalog.Object, key, ttl, hostName, redisImpl, assignActivity);

            _scenarioContext.Remove(nameof(RedisActivity));
            _scenarioContext.Remove(nameof(RedisCacheImpl));
            _scenarioContext.Remove(nameof(ttl));

            _scenarioContext.Add(nameof(RedisActivity), redisActivityNew);
            _scenarioContext.Add(nameof(RedisCacheImpl), redisImpl);
            _scenarioContext.Add(nameof(ttl), ttl);

            Assert.IsNotNull(redisActivityNew.Key);
        }

        [Given(@"another assign ""(.*)"" as")]
        [Then(@"another assign ""(.*)"" as")]
        public void GivenAnotherAssignAs(string data, Table table)
        {
            var redisActivity = _scenarioContext.Get<SpecRedisActivity>(nameof(RedisActivity));

            var assignActivity = GetDsfMultiAssignActivity("[[Var3]]", "Test4");

            redisActivity.ActivityFunc = new ActivityFunc<string, bool> { Handler = assignActivity };

            var assignOutputs = assignActivity.GetForEachOutputs();

            GetExpectedTableData(table, 0, out string expectedKey, out string expectedValue);

            Assert.AreEqual(expectedKey, assignOutputs[0].Value);
            Assert.IsTrue(expectedValue.Contains(assignOutputs[0].Name));

            var dic = new Dictionary<string, string> { { assignOutputs[0].Value, assignOutputs[0].Name } };

            _scenarioContext.Remove(nameof(RedisActivity));
            _scenarioContext.Remove(nameof(DsfMultiAssignActivity));

            _scenarioContext.Add(nameof(RedisActivity), redisActivity);
            _scenarioContext.Add(data, dic);
            _scenarioContext.Add(nameof(DsfMultiAssignActivity), assignActivity);

        }

        private static void GetExpectedTableData(Table table, int rowNumber, out string expectedKey, out string expectedValue)
        {
            var expectedRow = table.Rows[rowNumber].Values.ToList();

            expectedKey = expectedRow[0];
            expectedValue = expectedRow[1];
        }

        private DsfMultiAssignActivity GetDsfMultiAssignActivity(string fieldName, string fieldValue)
        {
            return new DsfMultiAssignActivity() { FieldsCollection = new List<ActivityDTO> { new ActivityDTO(fieldName, fieldValue, 1) } };
        }

        private RedisCacheImpl GetRedisCacheImpl(string hostName, string password, int port)
        {
            return new RedisCacheImpl(hostName: hostName, password: password, port: port);
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

        private static void GenResourceAndDataobject(string key, string hostName, string password, int port, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataobject, out ExecutionEnvironment environment)
        {
            mockResourceCatalog = new Mock<IResourceCatalog>();
            mockDataobject = new Mock<IDSFDataObject>();
            var redisSource = new Dev2.Data.ServiceModel.RedisSource { HostName = hostName, Password = password, Port = port.ToString() };
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

        private static SpecRedisDeleteActivity GetRedisDeleteActivity(IResourceCatalog resourceCatalog, string key, string hostName, RedisCacheImpl impl)
        {
            return new SpecRedisDeleteActivity(resourceCatalog, impl)
            {
                Key = key,
                RedisSource = new Dev2.Data.ServiceModel.RedisSource { HostName = hostName },
            };
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


        [AfterScenario(@"RedisDelete")]
        public void Cleanup()
        {
            _scenarioContext.Remove(nameof(RedisActivity));
            _scenarioContext.Remove(nameof(RedisCacheImpl));
            _scenarioContext.Remove(nameof(RedisDeleteActivity));
            _scenarioContext.Remove("key");
            _scenarioContext.Remove("ttl");
            _scenarioContext.Remove("hostName");
            _scenarioContext.Remove("password");
            _scenarioContext.Remove("port");

        }
    }
}
