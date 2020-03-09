/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Dev2.Activities.RedisCache;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client.Exceptions;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Driver.Redis;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using Warewolf.UnitTestAttributes;

namespace Dev2.Tests.Activities.ActivityTests.Redis
{
    [TestClass]
    public class RedisCacheActivityTests : BaseActivityTests
    {
        static RedisCacheActivity CreateRedisActivity()
        {
            return new RedisCacheActivity();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisCacheActivity))]
        public void RedisActivity_Equal_BothareObjects()
        {
            object redisActivity = CreateRedisActivity();
            var other = new object();
            var redisActivityEqual = redisActivity.Equals(other);
            Assert.IsFalse(redisActivityEqual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisCacheActivity))]
        public void RedisActivity_GivenEnvironmentIsNull_ShouldHaveNoDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var redisActivity = CreateRedisActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugInputs = redisActivity.GetDebugInputs(null, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugInputs.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RedisCacheActivity))]
        [Depends(Depends.ContainerType.AnonymousRedis)]
        public void RedisActivity_GetDebugInputs_ShouldReturnInnerActivityOutputs()
        {
            //----------------------Arrange----------------------
            try
            {
                //----------------------Arrange----------------------
                TestAnonymousAuth(new Depends(Depends.ContainerType.AnonymousRedis), out string key, out string hostName, out string password, out int port);

                var redisSource = new RedisSource { HostName = hostName, Password = password, Port = port.ToString() };
                var innerActivity = new DsfMultiAssignActivity() { FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[objectId1]]", "ObjectName1", 1), new ActivityDTO("[[objectId2]]", "ObjectName2", 2) } };

                GenerateMocks(key, redisSource, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataObject);
                GenerateSUTInstance(key, hostName, port, password, mockResourceCatalog, out Dictionary<string, string> evel, out TestRedisActivity sut, innerActivity);
                //----------------------Act--------------------------
                sut.TestExecuteTool(mockDataObject.Object);
                sut.TestPerformExecution(evel);

                var debugInputs = sut.GetDebugInputs(mockDataObject.Object.Environment, 0);
                //----------------------Assert-----------------------
                var actualInnerActivity = sut.ActivityFunc.Handler;

                Assert.AreEqual("Assign", actualInnerActivity.DisplayName);

                Assert.IsTrue(debugInputs is List<DebugItem>, "Debug inputs must return List<DebugItem>");
                Assert.AreEqual(4, debugInputs.Count);

                Assert.AreEqual(1, debugInputs[0].ResultsList.Count);
                AssertDebugItems(debugInputs, 0, 0, "Key", null, "=", sut.Key);
                AssertDebugItems(debugInputs, 1, 0, "Redis key { " + sut.Key + " } not found", null, "", "");

                AssertDebugItems(debugInputs, 2, 0, "1", null, "", "");
                AssertDebugItems(debugInputs, 2, 1, null, "[[objectId1]]", "=", "ObjectName1");

                AssertDebugItems(debugInputs, 3, 0, "2", null, "", "");
                AssertDebugItems(debugInputs, 3, 1, null, "[[objectId2]]", "=", "ObjectName2");
            }
            catch (Exception e)
            {
                if (e.Message.Contains("could not connect to redis Instance"))
                {
                    Assert.Inconclusive(e.Message);
                }
                else
                {
                    throw;
                }
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RedisCacheActivity))]
        [Depends(Depends.ContainerType.AnonymousRedis)]
        public void RedisActivity_GetDebugInputs_With_DataListUtilIsEvaluated_ShouldReturnInnerActivityOutputs()
        {
            //----------------------Arrange----------------------
            try
            {

                //----------------------Arrange----------------------
                TestAnonymousAuth(new Depends(Depends.ContainerType.AnonymousRedis), out string key, out string hostName, out string password, out int port);

                var redisSource = new RedisSource { HostName = hostName, Password = password, Port = port.ToString() };
                var isCalValue = GlobalConstants.CalculateTextConvertPrefix + "rec(*).name" + GlobalConstants.CalculateTextConvertSuffix;
                var innerActivity = new DsfMultiAssignActivity() { FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[objectId1]]", "ObjectName1", 1), new ActivityDTO("[[objectId2]]", "ObjectName2", 2), new ActivityDTO(isCalValue, "ObjectName3", 3) } };


                GenerateMocks(key, redisSource, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataObject);
                GenerateSUTInstance(key, hostName, port, password, mockResourceCatalog, out Dictionary<string, string> evel, out TestRedisActivity sut, innerActivity);
                //----------------------Act--------------------------
                sut.TestExecuteTool(mockDataObject.Object);
                sut.TestPerformExecution(evel);

                var debugInputs = sut.GetDebugInputs(mockDataObject.Object.Environment, 0);
                //----------------------Assert-----------------------
                var actualInnerActivity = sut.ActivityFunc.Handler;

                Assert.AreEqual("Assign", actualInnerActivity.DisplayName);

                Assert.IsTrue(debugInputs is List<DebugItem>, "Debug inputs must return List<DebugItem>");
                Assert.AreEqual(5, debugInputs.Count);

                Assert.AreEqual(1, debugInputs[0].ResultsList.Count);
                AssertDebugItems(debugInputs, 0, 0, "Key", null, "=", sut.Key);
                AssertDebugItems(debugInputs, 1, 0, "Redis key { " + sut.Key + " } not found", null, "", "");

                AssertDebugItems(debugInputs, 2, 0, "1", null, "", "");
                AssertDebugItems(debugInputs, 2, 1, null, "[[objectId1]]", "=", "ObjectName1");

                AssertDebugItems(debugInputs, 3, 0, "2", null, "", "");
                AssertDebugItems(debugInputs, 3, 1, null, "[[objectId2]]", "=", "ObjectName2");

                AssertDebugItems(debugInputs, 4, 0, "3", null, "", "");
                AssertDebugItems(debugInputs, 4, 1, null, isCalValue, "=", isCalValue);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("could not connect to redis Instance"))
                {
                    Assert.Inconclusive(e.Message);
                }
                else
                {
                    throw;
                }
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RedisCacheActivity))]
        public void RedisActivity_GetDebugOutputs_ShouldReturnCachedData_TTLNotReached()
        {
            //----------------------Arrange----------------------
            try {
                //----------------------Arrange----------------------
                TestAnonymousAuth(new Depends(Depends.ContainerType.AnonymousRedis), out string key, out string hostName, out string password, out int port);

                var redisSource = new RedisSource { HostName = hostName, Password = password, Port = port.ToString() };
                var innerActivity = new DsfMultiAssignActivity() { FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[objectId1]]", "ObjectName1", 1), new ActivityDTO("[[objectId2]]", "ObjectName2", 2) } };


                GenerateMocks(key, redisSource, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataObject);
                GenerateSUTInstance(key, hostName, port, password, mockResourceCatalog, out Dictionary<string, string> evel, out TestRedisActivity sut, innerActivity);
                //----------------------Act--------------------------
                sut.TestExecuteTool(mockDataObject.Object);
                sut.TestPerformExecution(evel);

                var debugOutputs = sut.GetDebugOutputs(mockDataObject.Object.Environment, 0);
                //----------------------Assert-----------------------
                var actualInnerActivity = sut.ActivityFunc.Handler;

                Assert.AreEqual("Assign", actualInnerActivity.DisplayName);

                Assert.IsTrue(debugOutputs is List<DebugItem>, "Debug inputs must return List<DebugItem>");
                Assert.AreEqual(3, debugOutputs.Count);

                Assert.AreEqual(1, debugOutputs[0].ResultsList.Count);
                AssertDebugItems(debugOutputs, 0, 0, "Redis key { " + sut.Key + " } found", null, "", "");

                AssertDebugItems(debugOutputs, 1, 0, "1", null, "", "");
                AssertDebugItems(debugOutputs, 1, 1, null, "[[objectId1]]", "=", "ObjectName1");

                AssertDebugItems(debugOutputs, 2, 0, "2", null, "", "");
                AssertDebugItems(debugOutputs, 2, 1, null, "[[objectId2]]", "=", "ObjectName2");
            }
            catch (Exception e)
            {
                if (e.Message.Contains("could not connect to redis Instance"))
                {
                    Assert.Inconclusive(e.Message);
                }
                else
                {
                    throw;
                }
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RedisCacheActivity))]
        [Depends(Depends.ContainerType.AnonymousRedis)]
        public void RedisActivity_GetDebugOutputs_ShouldReturnInnerActivityOutputs_TTLReached()
        {
            //----------------------Arrange----------------------
            try {
                //----------------------Arrange----------------------
                TestAnonymousAuth(new Depends(Depends.ContainerType.AnonymousRedis), out string key, out string hostName, out string password, out int port);

                var redisSource = new RedisSource { HostName = hostName, Password = password, Port = port.ToString() };
                var innerActivity = new DsfMultiAssignActivity() { FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[objectId1]]", "ObjectName1", 1), new ActivityDTO("[[objectId2]]", "ObjectName2", 2) } };

                GenerateMocks(key, redisSource, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataObject);
                GenerateSUTInstance(key, hostName, port, password, mockResourceCatalog, out Dictionary<string, string> evel, out TestRedisActivity sut, innerActivity);
                //----------------------Act--------------------------
                sut.TestExecuteTool(mockDataObject.Object);
                sut.TestPerformExecution(evel);

                var timer = new Stopwatch();
                timer.Start();
                do
                {
                    Thread.Sleep(1000);
                } while (timer.Elapsed < TimeSpan.FromMilliseconds(sut.TTL));
                timer.Stop();

                var debugOutputs = sut.GetDebugOutputs(mockDataObject.Object.Environment, 0);
                //----------------------Assert-----------------------
                var actualInnerActivity = sut.ActivityFunc.Handler;

                Assert.AreEqual("Assign", actualInnerActivity.DisplayName);

                Assert.IsTrue(debugOutputs is List<DebugItem>, "Debug inputs must return List<DebugItem>");
                Assert.AreEqual(3, debugOutputs.Count);

                Assert.AreEqual(1, debugOutputs[0].ResultsList.Count);
                AssertDebugItems(debugOutputs, 0, 0, "Redis key { " + sut.Key + " } found", null, "", "");

                AssertDebugItems(debugOutputs, 1, 0, "1", null, "", "");
                AssertDebugItems(debugOutputs, 1, 1, null, "[[objectId1]]", "=", "ObjectName1");

                AssertDebugItems(debugOutputs, 2, 0, "2", null, "", "");
                AssertDebugItems(debugOutputs, 2, 1, null, "[[objectId2]]", "=", "ObjectName2");
            }
            catch (Exception e)
            {
                if (e.Message.Contains("could not connect to redis Instance"))
                {
                    Assert.Inconclusive(e.Message);
                }
                else
                {
                    throw;
                }
            }
        }
        
        static void TestAnonymousAuth(Depends dependency, out string key, out string hostName, out string password, out int port)
        {
            key = "key" + Guid.NewGuid();
            hostName = dependency.Container.IP;
            password = "";
            port = int.Parse(dependency.Container.Port);
        }

        private static void GenerateSUTInstance(string key, string hostName, int port, string password, Mock<IResourceCatalog> mockResourceCatalog, out Dictionary<string, string> evel, out TestRedisActivity sut, Activity innerActivity)
        {
            evel = new Dictionary<string, string> { { "", "" } };
            var impl = new RedisCacheImpl(hostName, port, password);
            sut = new TestRedisActivity(mockResourceCatalog.Object, impl)
            {
                Key = key,
                TTL = 3000,
                ActivityFunc = new ActivityFunc<string, bool>
                {
                    Handler = innerActivity
                }
            };
        }

        private static void GenerateMocks(string key, RedisSource redisSource, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataObject)
        {
            mockResourceCatalog = new Mock<IResourceCatalog>();
            mockDataObject = new Mock<IDSFDataObject>();
            var environment = new ExecutionEnvironment();
            environment.Assign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>());
            environment.EvalToExpression(key, 0);
            mockResourceCatalog.Setup(o => o.GetResource<RedisSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(redisSource);

            mockDataObject.Setup(o => o.IsDebugMode()).Returns(true);
            mockDataObject.Setup(o => o.Environment).Returns(environment);
        }

        private void AssertDebugItems(List<DebugItem> debugInputs, int listIndex, int resultListIndex, string expLabel, string expVariable, string expOparator, string expValue)
        {
            Assert.AreEqual(expLabel, debugInputs[listIndex].ResultsList[resultListIndex].Label);
            Assert.AreEqual(expOparator, debugInputs[listIndex].ResultsList[resultListIndex].Operator);
            Assert.AreEqual(expValue, debugInputs[listIndex].ResultsList[resultListIndex].Value);
            Assert.AreEqual(expVariable, debugInputs[listIndex].ResultsList[resultListIndex].Variable);
        }

    }

    class TestRedisActivity : RedisCacheActivity
    {
        public TestRedisActivity(IResourceCatalog resourceCatalog, RedisCacheImpl impl)
            : base(resourceCatalog, impl)
        {

        }
        
        public void TestExecuteTool(IDSFDataObject dataObject)
        {
            base.ExecuteTool(dataObject, 0);
        }

        public void TestPerformExecution(Dictionary<string, string> evaluatedValues)
        {
            base.PerformExecution(evaluatedValues);
        }
    }
}
