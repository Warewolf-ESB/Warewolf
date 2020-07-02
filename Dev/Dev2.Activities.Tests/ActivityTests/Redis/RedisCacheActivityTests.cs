/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Driver.Redis;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using Warewolf.UnitTestAttributes;
using Activity = System.Activities.Activity;

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
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisCacheActivity))]
        public void RedisCacheActivity_Equal_BothareObjects()
        {
            object redisActivity = CreateRedisActivity();
            var other = new object();
            var redisActivityEqual = redisActivity.Equals(other);
            Assert.IsFalse(redisActivityEqual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisCacheActivity))]
        public void RedisCacheActivity_GivenEnvironmentIsNull_ShouldHaveNoDebugOutputs()
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
        public void RedisCacheActivity_CacheVariableResult_GetDebugInputs_With_DataListUtilIsEvaluated_ShouldReturnInnerActivityOutputs()
        {
            try
            {
                //----------------------Arrange----------------------
                var key = "key" + Guid.NewGuid();
                TestAnonymousAuth(out string hostName, out string password, out int port);
                var redisSource = new RedisSource {HostName = hostName, Password = password, Port = port.ToString()};

                var isCalValue = GlobalConstants.CalculateTextConvertPrefix + "rec(*).name" + GlobalConstants.CalculateTextConvertSuffix;
                var innerActivity = new DsfMultiAssignActivity()
                {
                    FieldsCollection = new List<ActivityDTO>
                    {
                        new ActivityDTO("[[objectId1]]", "ObjectName1", 1),
                        new ActivityDTO("[[objectId2]]", "ObjectName2", 2),
                        new ActivityDTO(isCalValue, "ObjectName3", 3)
                    }
                };

                GenerateMocks(key, redisSource, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataObject);
                CreateRedisActivity(key, hostName, port, password, mockResourceCatalog, out TestRedisActivity sut, innerActivity);
                //----------------------Act--------------------------
                sut.TestExecuteTool(mockDataObject.Object);

                var debugInputs = sut.GetDebugOutputs(mockDataObject.Object.Environment, 0);
                //----------------------Assert-----------------------
                var actualInnerActivity = sut.ActivityFunc.Handler;

                Assert.AreEqual("Assign", actualInnerActivity.DisplayName);

                Assert.IsTrue(debugInputs is List<DebugItem>, "Debug inputs must return List<DebugItem>");
                Assert.AreEqual(4, debugInputs.Count);

                Assert.AreEqual(1, debugInputs[0].ResultsList.Count);

                AssertDebugItems(debugInputs, 0, 0, "Redis key { " + sut.Key + " } not found", null, "", "");

                AssertDebugItems(debugInputs, 1, 0, "1", null, "", "");
                AssertDebugItems(debugInputs, 1, 1, null, "[[objectId1]]", "=", "ObjectName1");

                AssertDebugItems(debugInputs, 2, 0, "1", null, "", "");
                AssertDebugItems(debugInputs, 2, 1, null, "[[objectId2]]", "=", "ObjectName2");

                AssertDebugItems(debugInputs, 3, 0, "1", null, "", "");
                AssertDebugItems(debugInputs, 3, 1, null, isCalValue, "=", isCalValue);
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
        public void RedisCacheActivity_CacheVariableResult_ShouldReturnInnerActivityOutputs_TTLExpired()
        {
            try
            {
                //----------------------Arrange----------------------
                var key = "key" + Guid.NewGuid();
                TestAnonymousAuth(out string hostName, out string password, out int port);

                var redisSource = new RedisSource {HostName = hostName, Password = password, Port = port.ToString()};
                var innerActivity = new DsfMultiAssignActivity()
                {
                    FieldsCollection = new List<ActivityDTO>
                    {
                        new ActivityDTO("[[objectId1]]", "ObjectName1", 1),
                        new ActivityDTO("[[objectId2]]", "ObjectName2", 2)
                    }
                };

                GenerateMocks(key, redisSource, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataObject);
                CreateRedisActivity(key, hostName, port, password, mockResourceCatalog, out TestRedisActivity sut, innerActivity);
                //----------------------Act--------------------------
                sut.TestExecuteTool(mockDataObject.Object);
                var timer = new Stopwatch();
                timer.Start();
                do
                {
                    Thread.Sleep(30);
                } while (timer.Elapsed < TimeSpan.FromSeconds(sut.TTL + 5));

                timer.Stop();
                sut.TestExecuteTool(mockDataObject.Object);
                var debugOutputs = sut.GetDebugOutputs(mockDataObject.Object.Environment, 0);
                //----------------------Assert-----------------------
                var actualInnerActivity = sut.ActivityFunc.Handler;

                Assert.AreEqual("Assign", actualInnerActivity.DisplayName);

                Assert.IsTrue(debugOutputs is List<DebugItem>, "Debug outputs must return List<DebugItem>");
                Assert.AreEqual(3, debugOutputs.Count);
                Assert.AreEqual(1, debugOutputs[0].ResultsList.Count);
                AssertDebugItems(debugOutputs, 0, 0, "Redis key { " + sut.Key + " } not found", null, "", "");

                AssertDebugItems(debugOutputs, 1, 0, "1", null, "", "");
                AssertDebugItems(debugOutputs, 1, 1, null, "[[objectId1]]", "=", "ObjectName1");

                AssertDebugItems(debugOutputs, 2, 0, "1", null, "", "");
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
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisCacheActivity))]
        public void RedisCacheActivity_CacheVariableResult_EvaluateVariableAsKey()
        {
            try
            {
                var key = "[[RedisKey]]";
                var keyValue = "someval" + Guid.NewGuid();
                TestAnonymousAuth(out string hostName, out string password, out int port);
                var redisSource = new RedisSource {HostName = hostName, Password = password, Port = port.ToString()};
                var innerActivity = new DsfMultiAssignActivity()
                {
                    FieldsCollection = new List<ActivityDTO>
                    {
                        new ActivityDTO("[[objectId1]]", "ObjectName1", 1),
                        new ActivityDTO("[[objectId2]]", "ObjectName2", 2)
                    }
                };

                var mockResourceCatalog = new Mock<IResourceCatalog>();
                var mockDataObject = new Mock<IDSFDataObject>();
                var environment = new ExecutionEnvironment();
                environment.Assign(key, keyValue, 0);

                var env = new Mock<IExecutionEnvironment>();
                env.Setup(e => e.EvalToExpression(It.IsAny<string>(), It.IsAny<int>())).Returns(key);
                mockResourceCatalog.Setup(o => o.GetResource<RedisSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(redisSource);
                mockDataObject.Setup(o => o.IsDebugMode()).Returns(true);
                mockDataObject.Setup(o => o.Environment).Returns(environment);

                CreateRedisActivity(key, hostName, port, password, mockResourceCatalog, out TestRedisActivity sut, innerActivity);
                //----------------------Act--------------------------
                sut.TestExecuteTool(mockDataObject.Object);
                var debugOutputs = sut.GetDebugOutputs(mockDataObject.Object.Environment, 0);
                //----------------------Assert-----------------------
                Assert.AreEqual(3, debugOutputs.Count);
                Assert.AreEqual(1, debugOutputs[0].ResultsList.Count);
                AssertDebugItems(debugOutputs, 0, 0, "Redis key { " + keyValue + " } not found", null, "", "");

                AssertDebugItems(debugOutputs, 1, 0, "1", null, "", "");
                AssertDebugItems(debugOutputs, 1, 1, null, "[[objectId1]]", "=", "ObjectName1");

                AssertDebugItems(debugOutputs, 2, 0, "1", null, "", "");
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
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisCacheActivity))]
        public void RedisCacheActivity_CacheRecordsetResult_ReturnInnerActivityOutputs()
        {
            try
            {
                //----------------------Arrange----------------------
                var key = "key" + Guid.NewGuid();
                TestAnonymousAuth(out string hostName, out string password, out int port);

                var redisSource = new RedisSource {HostName = hostName, Password = password, Port = port.ToString()};
                var innerActivity = new DsfMultiAssignActivity()
                {
                    FieldsCollection = new List<ActivityDTO>
                    {
                        new ActivityDTO("[[bank(1).id]]", "1", 1),
                        new ActivityDTO("[[bank(2).id]]", "2", 2),
                        new ActivityDTO("[[bank(3).id]]", "3", 3),
                        new ActivityDTO("[[bank(1).name]]", "name1", 1),
                        new ActivityDTO("[[bank(2).name]]", "name2", 2),
                        new ActivityDTO("[[bank(3).name]]", "name3", 3),
                    }
                };

                GenerateMocks(key, redisSource, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataObject);
                CreateRedisActivity(key, hostName, port, password, mockResourceCatalog, out TestRedisActivity sut, innerActivity);
                //----------------------Act--------------------------
                sut.TestExecuteTool(mockDataObject.Object);
                var debugOutputs = sut.GetDebugOutputs(mockDataObject.Object.Environment, 0);

                //----------------------Assert-----------------------
                Assert.AreEqual(7, debugOutputs.Count);
                AssertDebugItems(debugOutputs, 0, 0, "Redis key { " + sut.Key + " } not found", null, "", "");

                AssertDebugItems(debugOutputs, 1, 0, "1", null, "", "");
                AssertDebugItems(debugOutputs, 1, 1, null, "[[bank(1).id]]", "=", "1");

                AssertDebugItems(debugOutputs, 2, 0, "1", null, "", "");
                AssertDebugItems(debugOutputs, 2, 1, null, "[[bank(2).id]]", "=", "2");

                AssertDebugItems(debugOutputs, 3, 0, "1", null, "", "");
                AssertDebugItems(debugOutputs, 3, 1, null, "[[bank(3).id]]", "=", "3");

                AssertDebugItems(debugOutputs, 4, 0, "1", null, "", "");
                AssertDebugItems(debugOutputs, 4, 1, null, "[[bank(1).name]]", "=", "name1");

                AssertDebugItems(debugOutputs, 5, 0, "1", null, "", "");
                AssertDebugItems(debugOutputs, 5, 1, null, "[[bank(2).name]]", "=", "name2");

                AssertDebugItems(debugOutputs, 6, 0, "1", null, "", "");
                AssertDebugItems(debugOutputs, 6, 1, null, "[[bank(3).name]]", "=", "name3");
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

        static void TestAnonymousAuth(out string hostName, out string password, out int port)
        {
            var dependency = new Depends(Depends.ContainerType.AnonymousRedis);
            hostName = "127.0.0.1"; //dependency.Container.IP;
            password = "";
            port = 6379; //int.Parse(dependency.Container.Port);
        }

        private static void CreateRedisActivity(string key, string hostName, int port, string password, Mock<IResourceCatalog> mockResourceCatalog, out TestRedisActivity sut, Activity innerActivity)
        {
            var impl = new RedisCacheImpl(hostName, port, password);
            sut = new TestRedisActivity(mockResourceCatalog.Object, impl)
            {
                Key = key,
                TTL = 30,
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
            base.ExecuteTool(dataObject, 1);
        }

        public void TestPerformExecution(Dictionary<string, string> evaluatedValues)
        {
            base.PerformExecution(evaluatedValues);
        }
    }
}