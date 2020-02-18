/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Activities.RedisCache;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Serializers;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Driver.Redis;
using Warewolf.Storage;
using Warewolf.UnitTestAttributes;

namespace Warewolf.Tools.Specs.Toolbox.Utility.Redis.Cache
{
    [Binding]
    public class RedisCacheSteps
    {

        readonly ScenarioContext _scenarioContext;
        private Depends _containerOps;

        public RedisCacheSteps(ScenarioContext scenarioContext) => _scenarioContext = scenarioContext ?? throw new ArgumentNullException("scenarioContext");
        public static Stopwatch Stoptime { get; set; }

        public static Mock<IDSFDataObject> _mockDataobject = new Mock<IDSFDataObject>();
        public static ExecutionEnvironment _environment = new ExecutionEnvironment();
        public static Mock<IResourceCatalog> _mockResourceCatalog = new Mock<IResourceCatalog>();

        [Given(@"Redis source ""(.*)"" with password ""(.*)"" and port ""(.*)""")]
        public void GivenRedisSourceWithPasswordAndPort(string hostName, string password, int port)
        {
            SetUpRedisClientConnection(hostName, password, port);
        }

        [Given(@"valid Redis source")]
        public void GivenValidRedisSource()
        {
            _containerOps = new Depends(Depends.ContainerType.AnonymousRedis);
            SetUpRedisClientConnection(Depends.EnableDocker?Depends.RigOpsIP:Depends.SVRDEVIP, "", 6380);
        }

        [Given(@"I have a key ""(.*)"" and ttl of ""(.*)"" milliseconds")]
        public void GivenIHaveAKeyAndTtlOfMilliseconds(string key, int ttl) => GenerateResourceAndDataObject(key, ttl);

        private void GenerateResourceAndDataObject(string myKey, int ttl)
        {
            var hostName = _scenarioContext.Get<string>("hostName");
            var password = _scenarioContext.Get<string>("password");
            var port = _scenarioContext.Get<int>("port");
            var redisImpl = GetRedisCacheImpl(hostName, password, port);

            var environment = new ExecutionEnvironment();
            GenResourceAndDataobject(myKey, hostName, password, port);

            var assignActivity = new DsfMultiAssignActivity();
            var redisActivityNew = SetupRedisActivity(_mockResourceCatalog.Object, myKey, ttl, hostName, redisImpl, assignActivity);

            _scenarioContext.Add(nameof(RedisCacheActivity), redisActivityNew);
            _scenarioContext.Add(nameof(RedisCacheImpl), redisImpl);
            _scenarioContext.Add(nameof(ttl), ttl);

            Assert.IsNotNull(redisActivityNew.Key);
        }

        [Given(@"I have ""(.*)"" of ""(.*)"" and ""(.*)"" of ""(.*)"" seconds")]
        public void GivenIHaveOfAndOfSeconds(string keyName, string key, string ttlName, int ttl)
        {
            _scenarioContext.Add(keyName, key);
            _scenarioContext.Add(ttlName, ttl);
        }

        [Given(@"I have ""(.*)"" of ""(.*)"" with GUID and ""(.*)"" of ""(.*)"" seconds")]
        public void GivenIHaveKeyWithGUIDAndTTLOf(string keyName, string key, string ttlName, int ttl)
        {
            var myNewKey = key + Guid.NewGuid();
            _scenarioContext.Add(keyName, myNewKey);
            _scenarioContext.Add(ttlName, ttl);
        }

        [Given(@"an assign ""(.*)"" into ""(.*)"" with")]
        public void GivenAnAssignIntoWith(string dataToStoreName, string innerActivityName, Table table)
        {
            var activities = GetActivitiesDTOFrom(table);

            var assignActivity = GetDsfMultiAssignActivity(activities);
            _scenarioContext.Add(dataToStoreName, assignActivity);
            _scenarioContext.Add(innerActivityName, assignActivity);
        }

        [Then(@"the assigned ""(.*)"", ""(.*)"" and innerActivity ""(.*)"" is executed by ""(.*)""")]
        public void ThenTheAssignedAndInnerActivityIsExecutedBy(string keyName, string ttlName, string innerActivityName, string redisActivityName)
        {
            var hostName = _scenarioContext.Get<string>("hostName");
            var password = _scenarioContext.Get<string>("password");
            var port = _scenarioContext.Get<int>("port");
            var key = _scenarioContext.Get<string>(keyName);
            var ttl = _scenarioContext.Get<int>(ttlName);
            var impl = _scenarioContext.Get<RedisCacheImpl>("impl");

            var assignActivity = _scenarioContext.Get<DsfMultiAssignActivity>(innerActivityName);
            SpecRedisActivity redisActivity = ExecuteRedisActivity(hostName, password, port, key, ttl, impl, assignActivity);

            _scenarioContext.Add(redisActivityName, redisActivity);
        }

        private static SpecRedisActivity ExecuteRedisActivity(string hostName, string password, int port, string key, int ttl, RedisCacheImpl impl, DsfMultiAssignActivity assignActivity)
        {
            var environment = new ExecutionEnvironment();
            GenResourceAndDataobject(key, hostName, password, port);

            var redisActivity = SetupRedisActivity(_mockResourceCatalog.Object, key, ttl, hostName, impl, assignActivity);

            ExecuteCacheTool(redisActivity, _mockDataobject);

            var executionResult = redisActivity.SpecPerformExecution(new Dictionary<string, string> { { "", "" } });
            Assert.AreEqual("Success", executionResult[0]);
            
            return redisActivity;
        }

        [Then(@"the Redis Cache under ""(.*)"" will contain")]
        public void ThenTheRedisCacheUnderWillContain(string keyStoringData, Table table)
        {
            AssertTableDataToActual(keyStoringData, table);
        }
        
        [Then(@"the Redis Cache under ""(.*)"" with GUID will contain")]
        public void TheRedisCacheUnderWillContain(string keyName, Table table) => AssertTableDataToActual(_scenarioContext.Get<string>(keyName), table);

        private void AssertTableDataToActual(string keyStoringData, Table table)
        {
            var impl = _scenarioContext.Get<RedisCacheImpl>("impl");

            var expCachedData = GetExpectedCachedDataFrom(table);
            var actualCachedData = GetCachedData(impl, keyStoringData);

            Assert.AreEqual(expCachedData.Count, actualCachedData.Count);
            var count = 0;
            foreach (var item in actualCachedData)
            {
                Assert.AreEqual(expCachedData[count].Name, actualCachedData[count].Name);
                Assert.AreEqual(expCachedData[count].Value, actualCachedData[count].Value);
                count++;
            }
        }

        [Then(@"""(.*)"" output variables have the following values")]
        public void ThenOutputVariablesHaveTheFollowingValues(string redisActivityName, Table table)
        {
            var redisActivity = _scenarioContext.Get<SpecRedisActivity>(redisActivityName);

            var expDebugItemResult = table.CreateSet<DebugItemResult>().ToList();

            var env = new ExecutionEnvironment();
            var debugOutputs = redisActivity.GetDebugOutputs(It.IsAny<ExecutionEnvironment>(), 0);

            AssertDebugItems(debugOutputs, 0, 0, expDebugItemResult[0].Label, expDebugItemResult[0].Variable == "null" ? null : "", expDebugItemResult[0].Operator, expDebugItemResult[0].Value);

            for (int count = 1; count < debugOutputs.Count; count++)
            {
                AssertDebugItems(debugOutputs, count, 1, expDebugItemResult[count].Label == "null" ? null : expDebugItemResult[count].Label, expDebugItemResult[count].Variable == "null" ? null : expDebugItemResult[count].Variable, expDebugItemResult[count].Operator == "null" ? null : expDebugItemResult[count].Operator, expDebugItemResult[count].Value == "null" ? null : expDebugItemResult[count].Value);
            }
        }

        [Then(@"the Execution Environment has these error")]
        public void ThenTheExecutionEnvironmentHasTheseError(Table table)
        {
            var errors = _environment.FetchErrors();

            var expErrors = table.CreateSet<SpecExecutionEnvironmentErrors>();

            foreach (var error in expErrors)
            {
                Assert.IsTrue(errors.Contains(error.Error));
            }
        }

        private void AssertDebugItems(List<DebugItem> debugOutputs, int listIndex, int resultListIndex, string expLabel, string expVariable, string expOparator, string expValue)
        {
            var myKey = _scenarioContext.Get<string>("key2");
            Assert.AreEqual(expLabel?.Replace("MyData", myKey), debugOutputs[listIndex].ResultsList[resultListIndex].Label);
            Assert.AreEqual(expValue, debugOutputs[listIndex].ResultsList[resultListIndex].Value);
            Assert.AreEqual(expVariable, debugOutputs[listIndex].ResultsList[resultListIndex].Variable);
        }

        [Given(@"I have a key ""(.*)"" with GUID and ttl of ""(.*)"" milliseconds")]
        public void GivenIHaveAKeyWithGUIDAndTtlOfMilliseconds(string key, int ttl)
        {
            var myNewGuid = Guid.NewGuid();
            var myNewKey = key + myNewGuid;
            GenerateResourceAndDataObject(myNewKey, ttl);
            _scenarioContext.Add("key", myNewKey);
            _scenarioContext.Add("guid", myNewGuid);
        }

        private List<SpecAssignValue> GetExpectedCachedDataFrom(Table table)
        {
            return table.CreateSet<SpecAssignValue>() as List<SpecAssignValue>;
        }

        private List<ActivityDTO> GetActivitiesDTOFrom(Table table)
        {
            var activities = new List<ActivityDTO>();
            var assignValues = table.CreateSet<SpecAssignValue>();
            var count = 1;
            foreach (var assignValue in assignValues)
            {
                activities.Add(new ActivityDTO(assignValue.Name, assignValue.Value, count++));
            }
            return activities;
        }

        [Given(@"No data in the cache")]
        public void GivenNoDataInTheCache()
        {
            var redisActivityOld = _scenarioContext.Get<SpecRedisActivity>(nameof(RedisCacheActivity));
            var impl = _scenarioContext.Get<RedisCacheImpl>(nameof(RedisCacheImpl));
            var ttl = _scenarioContext.Get<int>("ttl");

            var environment = new ExecutionEnvironment();

            var key = environment.EvalToExpression(redisActivityOld.Key, 0);

            do
            {
                Thread.Sleep(1000);
            } while (Stoptime.ElapsedMilliseconds < ttl);

            var actualCachedData = GetCachedData(impl, key);
            Assert.IsTrue(actualCachedData == null || actualCachedData.Count <= 0, "Cached data found when none was expected.");
        }

        [Given(@"an assign ""(.*)"" as")]
        [Then(@"an assign ""(.*)"" as")]
        public void GivenAnAssignAs(string data, Table table)
        {
            var redisActivity = _scenarioContext.Get<SpecRedisActivity>(nameof(RedisCacheActivity));
            var activities = new List<ActivityDTO> { { new ActivityDTO("[[Var1]]", "Test1", 1) } };

            var assignActivity = GetDsfMultiAssignActivity(activities);

            redisActivity.ActivityFunc = new ActivityFunc<string, bool> { Handler = assignActivity };

            var assignOutputs = assignActivity.GetForEachOutputs();

            GetExpectedTableData(table, 0, out string expectedKey, out string expectedValue);

            Assert.AreEqual(expectedKey, assignOutputs[0].Value);
            Assert.IsTrue(expectedValue.Contains(assignOutputs[0].Name));

            var dic = new Dictionary<string, string> { { assignOutputs[0].Value, assignOutputs[0].Name } };

            _scenarioContext.Remove(nameof(RedisCacheActivity));
            _scenarioContext.Add(nameof(RedisCacheActivity), redisActivity);
            _scenarioContext.Add(data, dic);
            _scenarioContext.Add(nameof(DsfMultiAssignActivity), assignActivity);

        }

        [Then(@"I execute the cache tool")]
        [When(@"I execute the cache tool")]
        [Then(@"I execute the Redis Cache tool")]
        [When(@"I execute the Redis Cache tool")]
        public void WhenIExecuteThecacheTool()
        {
            var redisActivityOld = _scenarioContext.Get<SpecRedisActivity>(nameof(RedisCacheActivity));
            var dataToStore = _scenarioContext.Get<Dictionary<string, string>>("dataToStore");
            var assignActivity = _scenarioContext.Get<DsfMultiAssignActivity>(nameof(DsfMultiAssignActivity));
            var hostName = _scenarioContext.Get<string>("hostName");
            var password = _scenarioContext.Get<string>("password");
            var port = _scenarioContext.Get<int>("port");
            var impl = _scenarioContext.Get<RedisCacheImpl>(nameof(RedisCacheImpl));

            var environment = new ExecutionEnvironment();
            GenResourceAndDataobject(redisActivityOld.Key, hostName, password, port);

            ExecuteCacheTool(redisActivityOld, _mockDataobject);
        }

        [Then(@"the cache will contain")]
        public void ThenTheCacheWillContain(Table table)
        {
            var redisActivity = _scenarioContext.Get<SpecRedisActivity>(nameof(RedisCacheActivity));
            var impl = _scenarioContext.Get<RedisCacheImpl>(nameof(RedisCacheImpl));

            var actualCacheData = GetCachedData(impl, redisActivity.Key);

            GetExpectedTableData(table, 0, out string expectedKey, out string expectedValue);

            Assert.IsTrue(expectedValue.Contains(actualCacheData[0].Name));
            Assert.IsTrue(expectedValue.Contains(actualCacheData[0].Value));

            _scenarioContext.Add(redisActivity.Key, actualCacheData);
        }

        [Then(@"output variables have the following values")]
        public void ThenOutputVariablesHaveTheFollowingValues(Table table)
        {
            var redisActivity = _scenarioContext.Get<RedisCacheActivity>(nameof(RedisCacheActivity));
            var impl = _scenarioContext.Get<RedisCacheImpl>(nameof(RedisCacheImpl));
            var actualCacheData = GetCachedData(impl, redisActivity.Key);

            GetExpectedTableData(table, 0, out string expectedKey, out string expectedValue);

           Assert.AreEqual(expected: expectedKey, actual: actualCacheData[0].Name);
           Assert.IsTrue(expectedValue.Contains(actualCacheData[0].Value));

        }

        void SetUpRedisClientConnection(string hostName, string password, int port)
        {
            var impl = GetRedisCacheImpl(hostName, password, port);

            _scenarioContext.Add("hostName", hostName);
            _scenarioContext.Add("password", password);
            _scenarioContext.Add("port", port);
            _scenarioContext.Add("impl", impl);
        }

        [Given(@"data exists \(TTL not hit\) for key ""(.*)"" with GUID as")]
        public void GivenDataExistsTTLNotHitForKeyWithGUIDAs(string key, Table table)
        {
            var myKey = _scenarioContext.Get<string>("key");
            if (!string.IsNullOrEmpty(myKey))
            {
                GetExpectedTableData(table, 0, out string expectedKey, out string expectedValue);
                if (expectedKey == key)
                {
                    expectedKey = myKey;
                }
                VerifyKey(myKey, expectedKey, expectedValue);
            }
            else
            {
                GetExpectedTableData(table, 0, out string expectedKey, out string expectedValue);
                VerifyKey(key, expectedKey, expectedValue);
            }
        }


        [Given(@"data exists \(TTL not hit\) for key ""(.*)"" as")]
        public void GivenDataExistsTTLNotHitForKeyAs(string key, Table table)
        {
            GetExpectedTableData(table, 0, out string expectedKey, out string expectedValue);
            VerifyKey(key, expectedKey, expectedValue);
        }

        void VerifyKey(string myKey, string expectedKey, string expectedValue)
        {
            var hostName = _scenarioContext.Get<string>("hostName");
            var password = _scenarioContext.Get<string>("password");
            var port = _scenarioContext.Get<int>("port");
            var ttl = _scenarioContext.Get<int>("ttl");
            var redisImpl = GetRedisCacheImpl(hostName, password, port);

            GenResourceAndDataobject(myKey, hostName, password, port);

            var dataStored = new List<ActivityDTO> { new ActivityDTO ("[[Var1]]", "Data in cache", 1 ) };

            var assignActivity = GetDsfMultiAssignActivity(dataStored);

            var redisActivityNew = SetupRedisActivity(_mockResourceCatalog.Object, myKey, ttl, hostName, redisImpl, assignActivity);

            ExecuteCacheTool(redisActivityNew, _mockDataobject);

            var sdfsf = redisActivityNew.SpecPerformExecution(new Dictionary<string, string> { { string.Empty, string.Empty } });

            var actualDataStored = GetCachedData(redisImpl, myKey);

            Assert.AreEqual(expectedKey, myKey);
            Assert.IsTrue(expectedValue.Contains(actualDataStored[0].Name), $"Actual key {actualDataStored[0].Name} is not in expected {expectedValue}");
            Assert.IsTrue(expectedValue.Contains(actualDataStored[0].Value), $"Actual value {actualDataStored[0].Value} is not in expected {expectedValue}");

            _scenarioContext.Add(redisActivityNew.Key, actualDataStored);
            _scenarioContext.Remove(nameof(RedisCacheActivity));
            _scenarioContext.Add(nameof(RedisCacheActivity), redisActivityNew);
        }

        [Then(@"the assign ""(.*)"" is not executed")]
        public void ThenTheAssignIsNotExecuted(string key)
        {
            var dataToStore = _scenarioContext.Get<IDictionary<string, string>>(key);

            Assert.IsNotNull(dataToStore);
        }

        [Given(@"data does not exist \(TTL exceeded\) for key ""(.*)"" as")]
        public void GivenDataDoesNotExistTTLExceededForKeyAs(string key, Table table)
        {
            var ttl = _scenarioContext.Get<int>("ttl");

            var impl = _scenarioContext.Get<RedisCacheImpl>(nameof(RedisCacheImpl));

            do
            {
                Thread.Sleep(1000);
            } while (Stoptime.ElapsedMilliseconds < ttl);

            var guidKey = _scenarioContext.Get<string>("key");
            if (!string.IsNullOrEmpty(guidKey))
            {
                key = guidKey;
            }
            var actualCachedData = GetCachedData(impl, key);

            Assert.AreEqual(0, table.RowCount);
            Assert.IsTrue(actualCachedData == null || actualCachedData.Count == 0, $"Key=Value exists: {actualCachedData?.FirstOrDefault()}={actualCachedData?.FirstOrDefault()}");
        }

        [Then(@"the assign ""(.*)"" is executed")]
        public void ThenTheAssignIsExecuted(string dataToStoreKey)
        {
            var dataToStore = _scenarioContext.Get<Dictionary<string, string>>(dataToStoreKey);
            var redisActivity = _scenarioContext.Get<SpecRedisActivity>(nameof(RedisCacheActivity));

            var executioResult = redisActivity.SpecPerformExecution(dataToStore);

            Assert.AreEqual("Success", executioResult[0]);
        }

        public static string errors { get; set; }
        private static void ExecuteCacheTool(SpecRedisActivity redisActivity, Mock<IDSFDataObject> mockDataobject)
        {
            redisActivity.SpecExecuteTool(mockDataobject.Object);
        }

        private RedisCacheImpl GetRedisCacheImpl(string hostName, string password, int port)
        {
            return new RedisCacheImpl(hostName: hostName, password: password, port: port);
        }

        private DsfMultiAssignActivity GetDsfMultiAssignActivity(List<ActivityDTO> activities)
        {
            return new DsfMultiAssignActivity() { FieldsCollection = activities };
        }

        void GetExpectedTableData(Table table, int rowNumber, out string expectedKey, out string expectedValue)
        {
            var expectedRow = table.Rows[rowNumber].Values.ToList();
            expectedKey = expectedRow[0];
            expectedValue = expectedRow[1];
        }

        private List<SpecAssignValue> GetCachedData(RedisCacheImpl impl, string key)
        {
            var activities = new List<SpecAssignValue>();
            var actualCacheData = impl.Get(key);

            if (actualCacheData != null)
            {
                var serializer = new Dev2JsonSerializer();
                var actualData = serializer.Deserialize<IDictionary<string, string>>(actualCacheData);

                foreach (var item in actualData)
                {
                    activities.Add(new SpecAssignValue {  Name = item.Key, Value = item.Value });
                }
            }
            return activities;
        }

        private static void GenResourceAndDataobject(string key, string hostName, string password, int port)
        {
            var redisSource = new Dev2.Data.ServiceModel.RedisSource { HostName = hostName, Password = password, Port = port.ToString() };
            _mockResourceCatalog.Setup(o => o.GetResource<Dev2.Data.ServiceModel.RedisSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(redisSource);

            _environment.Assign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>());
            _environment.EvalToExpression(key, 0);

            _mockDataobject.Setup(o => o.IsDebugMode()).Returns(true);
            _mockDataobject.Setup(o => o.Environment).Returns(_environment);
        }

        private static SpecRedisActivity SetupRedisActivity(IResourceCatalog resourceCatalog, string key, int ttl, string hostName, RedisCacheImpl impl, DsfMultiAssignActivity assignActivity)
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

        [AfterScenario(@"RedisCache")]
        public void Cleanup()
        {

            _scenarioContext.Remove("hostName");
            _scenarioContext.Remove("password");
            _scenarioContext.Remove("port");

            _scenarioContext.Remove(nameof(RedisCacheActivity));
            _scenarioContext.Remove(nameof(RedisCacheImpl));
            _scenarioContext.Remove("ttl");

            _scenarioContext.Remove(nameof(RedisCacheActivity));
            _scenarioContext.Remove("dataToStore");
            _scenarioContext.Remove(nameof(DsfMultiAssignActivity));
        }

    }
}
