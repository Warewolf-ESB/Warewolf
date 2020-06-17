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
using Dev2.Activities.RedisRemove;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Driver.Redis;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using Warewolf.UnitTestAttributes;
using WarewolfParserInterop;

namespace Dev2.Tests.Activities.ActivityTests.Redis
{
    [TestClass]
    public class RedisRemoveActivityTests : BaseActivityTests
    {
        static RedisRemoveActivity CreateRedisRemoveActivity()
        {
            return new RedisRemoveActivity();
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_Equal_BothareObjects()
        {
            object RedisRemoveActivity = CreateRedisRemoveActivity();
            var other = new object();
            var redisActivityEqual = RedisRemoveActivity.Equals(other);
            Assert.IsFalse(redisActivityEqual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_GivenEnvironmentIsNull_ShouldHaveNoDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var redisRemoveActivity = CreateRedisRemoveActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugInputs = redisRemoveActivity.GetDebugInputs(null, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugInputs.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_UseVariableAsKey()
        {
            try
            {
                var redisSource = TestRedisSource(out var hostName, out var password, out var port);
                var key = "[[RedisKey]]";
                var keyValue = "someval" + Guid.NewGuid();
                var innerActivity = new DsfMultiAssignActivity()
                {
                    FieldsCollection = new List<ActivityDTO>
                    {
                        new ActivityDTO("[[objectId1]]", "ObjectName1", 1),
                        new ActivityDTO("[[objectId2]]", "ObjectName2", 2)
                    }
                };

                var mockResourceCatalog = new Mock<IResourceCatalog>();
                mockResourceCatalog.Setup(o => o.GetResource<RedisSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(redisSource);

                var environment = new ExecutionEnvironment();
                environment.Assign(key, keyValue, 0);

                var mockDataObject = new Mock<IDSFDataObject>();
                mockDataObject.Setup(o => o.IsDebugMode()).Returns(true);
                mockDataObject.Setup(o => o.Environment).Returns(environment);

                //--create key
                GenerateAddKeyInstance(key, hostName, port, password, mockResourceCatalog, out Dictionary<string, string> evalAdd, out TestRedisActivity sutAdd, innerActivity);
                //----------------------Act--------------------------
                sutAdd.TestExecuteTool(mockDataObject.Object);
                sutAdd.TestPerformExecution(evalAdd);
                //--Remove Key
                GenerateRemoveKeyInstance(key, hostName, port, password, mockResourceCatalog, out TestRedisRemoveActivity sutRemove);
                //----------------------Act--------------------------
                mockDataObject.Setup(o => o.IsDebugMode()).Returns(true);
                mockDataObject.Setup(o => o.Environment).Returns(environment);
                sutRemove.TestExecuteTool(mockDataObject.Object);
                Assert.AreEqual(sutRemove.Key, key);
                Assert.AreEqual("Success", sutRemove.Response);
                sutRemove.TestExecuteTool(mockDataObject.Object);
                Assert.AreEqual("Failure", sutRemove.Response);
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

        private static RedisSource TestRedisSource(out string hostName, out string password, out int port)
        {
            var dependency = new Depends(Depends.ContainerType.AnonymousRedis);
            hostName = dependency.Container.IP;
            password = "";
            port = int.Parse(dependency.Container.Port);
            var redisSource = new RedisSource
            {
                HostName = hostName,
                Password = password,
                Port = port.ToString()
            };
            return redisSource;
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_RecordsetAsKey()
        {
            try
            {
                var redisSource = TestRedisSource(out var hostName, out var password, out var port);
                var keyValue1 = "Cache-1";
                var keyValue2 = "Cache-2";
                var keyValue3 = "Cache-3";

                var l = new List<AssignValue>();
                l.Add(new AssignValue("[[Redis(1).name]]", keyValue1));
                l.Add(new AssignValue("[[Redis(2).name]]", keyValue2));
                l.Add(new AssignValue("[[Redis(3).name]]", keyValue3));

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
                environment.AssignWithFrame(l, 1);

                var env = new Mock<IExecutionEnvironment>();
                env.Setup(e => e.EvalToExpression(It.IsAny<string>(), It.IsAny<int>())).Returns(keyValue1);
                env.Setup(e => e.EvalToExpression(It.IsAny<string>(), It.IsAny<int>())).Returns(keyValue2);
                env.Setup(e => e.EvalToExpression(It.IsAny<string>(), It.IsAny<int>())).Returns(keyValue3);
                mockResourceCatalog.Setup(o => o.GetResource<RedisSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(redisSource);
                mockDataObject.Setup(o => o.IsDebugMode()).Returns(true);
                mockDataObject.Setup(o => o.Environment).Returns(environment);

                //--create keys
                AddKeytoCache(keyValue1, hostName, port, password, mockResourceCatalog, innerActivity, mockDataObject);
                AddKeytoCache(keyValue2, hostName, port, password, mockResourceCatalog, innerActivity, mockDataObject);
                AddKeytoCache(keyValue3, hostName, port, password, mockResourceCatalog, innerActivity, mockDataObject);

                //--Remove Keys
                var sutRemove = RemoveKeyFromCache("[[Redis(1).name]]", hostName, port, password, mockResourceCatalog, mockDataObject, environment);
                Assert.AreEqual("Success", sutRemove.Response);
                sutRemove.TestExecuteTool(mockDataObject.Object);
                Assert.AreEqual("Failure", sutRemove.Response);

                sutRemove = RemoveKeyFromCache("[[Redis(2).name]]", hostName, port, password, mockResourceCatalog, mockDataObject, environment);
                Assert.AreEqual("Success", sutRemove.Response);
                sutRemove.TestExecuteTool(mockDataObject.Object);
                Assert.AreEqual("Failure", sutRemove.Response);

                sutRemove = RemoveKeyFromCache("[[Redis(3).name]]", hostName, port, password, mockResourceCatalog, mockDataObject, environment);
                Assert.AreEqual("Success", sutRemove.Response);
                sutRemove.TestExecuteTool(mockDataObject.Object);
                Assert.AreEqual("Failure", sutRemove.Response);
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


        private static void AddKeytoCache(string key, string hostName, int port, string password, Mock<IResourceCatalog> mockResourceCatalog, DsfMultiAssignActivity innerActivity, Mock<IDSFDataObject> mockDataObject)
        {
            GenerateAddKeyInstance(key, hostName, port, password, mockResourceCatalog, out Dictionary<string, string> evelAdd, out TestRedisActivity sutAdd, innerActivity);
            //----------------------Act--------------------------
            sutAdd.TestExecuteTool(mockDataObject.Object);
            sutAdd.TestPerformExecution(evelAdd);
        }

        private static TestRedisRemoveActivity RemoveKeyFromCache(string key, string hostName, int port, string password, Mock<IResourceCatalog> mockResourceCatalog, Mock<IDSFDataObject> mockDataObject, ExecutionEnvironment environment)
        {
            GenerateRemoveKeyInstance(key, hostName, port, password, mockResourceCatalog, out TestRedisRemoveActivity sutRemove);
            //----------------------Act--------------------------
            mockDataObject.Setup(o => o.IsDebugMode()).Returns(true);
            mockDataObject.Setup(o => o.Environment).Returns(environment);
            sutRemove.TestExecuteTool(mockDataObject.Object);
            return sutRemove;
        }

        private static void GenerateAddKeyInstance(string key, string hostName, int port, string password, Mock<IResourceCatalog> mockResourceCatalog, out Dictionary<string, string> eval, out TestRedisActivity sut, Activity innerActivity)
        {
            eval = new Dictionary<string, string> {{"", ""}};
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
        private static void GenerateRemoveKeyInstance(string key, string hostName, int port, string password, Mock<IResourceCatalog> mockResourceCatalog,out TestRedisRemoveActivity sut)
        {
            var impl = new RedisCacheImpl(hostName, port, password);
            sut = new TestRedisRemoveActivity(mockResourceCatalog.Object, impl)
            {
                Key = key,
            };
        }
    }

    class TestRedisRemoveActivity : RedisRemoveActivity
    {
        public TestRedisRemoveActivity(IResourceCatalog resourceCatalog, RedisCacheImpl impl)
            : base(resourceCatalog, impl)
        {
        }

        public void TestExecuteTool(IDSFDataObject dataObject)
        {
            base.ExecuteTool(dataObject, 0);
        }
    }
}