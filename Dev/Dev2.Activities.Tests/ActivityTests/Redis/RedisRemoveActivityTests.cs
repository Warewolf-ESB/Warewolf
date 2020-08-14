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
using System.Linq;
using System.Text;
using Dev2.Activities.RedisCache;
using Dev2.Activities.RedisRemove;
using Dev2.Common;
using Dev2.Common.State;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Driver.Redis;
using Warewolf.Interfaces;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

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
        public void RedisRemoveActivity_Equal_BothareObjects_ShouldNotBeEqual()
        {
            object RedisRemoveActivity = CreateRedisRemoveActivity();
            var other = new object();
            var redisActivityEqual = RedisRemoveActivity.Equals(other);
            Assert.IsFalse(redisActivityEqual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_Equal_BothareObjects_ShouldBeEqual()
        {
            var uniqueId = Guid.NewGuid();

            object RedisRemoveActivity = new RedisRemoveActivity
            {
                UniqueID = uniqueId.ToString()
            };
            var redisActivityEqual = RedisRemoveActivity.Equals(new RedisRemoveActivity
            {
                UniqueID = uniqueId.ToString()
            });

            Assert.IsTrue(redisActivityEqual);
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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_GetState()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var expectedSourceId = Guid.NewGuid();
            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, null)
            {
                Key = "[[Key]]",
                Result = "[[res]]",
                Response = "abcde",
                SourceId = expectedSourceId
            };

            var stateItems = redisRemoveActivity.GetState();
            Assert.AreEqual(4, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "Key",
                    Value = "[[Key]]",
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "Result",
                    Value = "[[res]]",
                    Type = StateVariable.StateType.Output
                },
                new StateVariable
                {
                    Name = "Response",
                    Value = "abcde",
                    Type = StateVariable.StateType.Output
                },
                new StateVariable
                {
                    Name = "SourceId",
                    Value = expectedSourceId.ToString(),
                    Type = StateVariable.StateType.Input
                }
            };

            var iter = redisRemoveActivity.GetState().Select(
                (item, index) => new
                {
                    value = item,
                    expectValue = expectedResults[index]
                }
            );

            //------------Assert Results-------------------------
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_GetOutputs_EmptyList()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, null);

            var outputs = redisRemoveActivity.GetOutputs();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, outputs.Count);
            Assert.IsNull(outputs[0]);
            Assert.IsNull(outputs[1]);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_GetOutputs_ResultList()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var expectedSourceId = Guid.NewGuid();
            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, null)
            {
                Key = "[[Key]]",
                Result = "[[res]]",
                Response = "abcde",
                SourceId = expectedSourceId
            };

            var outputs = redisRemoveActivity.GetOutputs();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, outputs.Count);
            Assert.AreEqual("abcde", outputs[0]);
            Assert.AreEqual("[[res]]", outputs[1]);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_GetDebugOutputs_GivenIsNewReturnsZero()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            //---------------Set up test pack-------------------
            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, null);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            var debugOutputs = redisRemoveActivity.GetDebugOutputs(mockExecutionEnvironment.Object, 1);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugOutputs.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_Equals_Null_AreNotEqual()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            //---------------Set up test pack-------------------
            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, null);
            RedisRemoveActivity redisRemoveActivityOther = null;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(redisRemoveActivity);
            //---------------Execute Test ----------------------
            var equals = redisRemoveActivity.Equals(redisRemoveActivityOther);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_Equals_Same_AreEqual()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            //---------------Set up test pack-------------------
            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, null);
            RedisRemoveActivity redisRemoveActivityOther = redisRemoveActivity;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(redisRemoveActivity);
            //---------------Execute Test ----------------------
            var equals = redisRemoveActivity.Equals(redisRemoveActivityOther);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_Equals_AreNotEqual()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            //---------------Set up test pack-------------------
            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, null);
            var redisRemoveActivityOther = new RedisRemoveActivity(mockResourceCatalog.Object, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(redisRemoveActivity);
            Assert.IsNotNull(redisRemoveActivityOther);
            //---------------Execute Test ----------------------
            var equals = redisRemoveActivity.Equals(redisRemoveActivityOther);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_Equals_DifferentResult_AreNotEqual()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, null)
            {
                UniqueID = uniqId,
                Result = "A",
            };
            var redisRemoveActivityOther = new RedisRemoveActivity(mockResourceCatalog.Object, null)
            {
                UniqueID = uniqId,
                Result = "B",
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(redisRemoveActivity);
            //---------------Execute Test ----------------------
            var equals = redisRemoveActivity.Equals(redisRemoveActivityOther);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_Equals_Object_Null_AreNotEqual()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid();
            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, null)
            {
                SourceId = uniqId,
                Result = "A",
                Key = "[[Key]]",
                DisplayName = "Redis Remove",
                Response = "asdf",
            };
            object redisRemoveActivityOther = null;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(redisRemoveActivity);
            //---------------Execute Test ----------------------
            var equals = redisRemoveActivity.Equals(redisRemoveActivityOther);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_Equals_Object_SameResult_AreEqual()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid();
            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, null)
            {
                SourceId = uniqId,
                Result = "A",
                Key = "[[Key]]",
                DisplayName = "Redis Remove",
                Response = "asdf",
            };
            object redisRemoveActivityOther = redisRemoveActivity;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(redisRemoveActivity);
            Assert.IsNotNull(redisRemoveActivityOther);
            //---------------Execute Test ----------------------
            var equals = redisRemoveActivity.Equals(redisRemoveActivityOther);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_Equals_GetType_AreNotEqual()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid();
            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, null)
            {
                SourceId = uniqId,
                Result = "A",
                Key = "[[Key]]",
                DisplayName = "Redis Remove",
                Response = "asdf",
            };
            object redisCacheActivity = new RedisCacheActivity(mockResourceCatalog.Object, null);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(redisRemoveActivity);
            Assert.IsNotNull(redisCacheActivity);
            //---------------Execute Test ----------------------
            var equals = redisRemoveActivity.Equals(redisCacheActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_Equals_Object_AreEqual()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid();
            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, null)
            {
                SourceId = uniqId,
                Result = "A",
                Key = "[[Key]]",
                DisplayName = "Redis Remove",
                Response = "asdf",
            };
            object redisCacheActivity = redisRemoveActivity;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(redisRemoveActivity);
            Assert.IsNotNull(redisCacheActivity);
            //---------------Execute Test ----------------------
            var equals = redisRemoveActivity.Equals(redisCacheActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_GetHashCode_IsNotNull_Expect_True()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid();
            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, null)
            {
                SourceId = uniqId,
                Result = "A",
                Key = "[[Key]]",
                DisplayName = "Redis Remove",
                Response = "asdf",
            };
            //---------------Execute Test ----------------------
            var hashCode = redisRemoveActivity.GetHashCode();
            //---------------Test Result -----------------------
            Assert.IsNotNull(hashCode);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_ExecuteTool_RedisSource_NotNull_ExpectFailure()
        {
            var key = "key";
            var sourceId = Guid.NewGuid();
            var redisSource = new RedisSource
            {
                HostName = "localhost",
                Password = "",
                Port = "1234",
            };

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(new ExecutionEnvironment());

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetResource<Resource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(redisSource);

            var mockRedisCache = new Mock<IRedisCache>();
            mockRedisCache.Setup(o => o.Remove(key))
                .Returns(false);

            var mockRedisConnection = new Mock<IRedisConnection>();
            mockRedisConnection.Setup(o => o.Cache)
                .Returns(mockRedisCache.Object);

            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, new RedisCacheStub(() => mockRedisConnection.Object))
            {
                Key = key,
                SourceId = sourceId
            };

            redisRemoveActivity.Execute(mockDataObject.Object, 0);

            mockRedisCache.Verify(o => o.Remove(key), Times.Once);

            Assert.AreEqual("Failure", redisRemoveActivity.Response);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_ExecuteTool_RedisSource_NotNull_ExpectRedisSourceHasBeenRemoved()
        {
            var key = "key";
            var env = new ExecutionEnvironment();
            var redisSource = new RedisSource
            {
                HostName = "localhost",
                Password = "",
                Port = "1234",
            };

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);

            var mockRedisCache = new Mock<IRedisCache>();
            mockRedisCache.Setup(o => o.Remove(key))
                .Returns(true);

            var mockRedisConnection = new Mock<IRedisConnection>();
            mockRedisConnection.Setup(o => o.Cache)
                .Returns(mockRedisCache.Object);

            var redisRemoveActivity = new RedisRemoveActivity(new Mock<IResourceCatalog>().Object, new RedisCacheStub(() => mockRedisConnection.Object))
            {
                Key = key,
            };

            var result = redisRemoveActivity.Execute(mockDataObject.Object, 0);

            mockRedisCache.Verify(o => o.Remove(key), Times.Never);

            Assert.IsNull(redisRemoveActivity.Response);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_ExecuteTool_RedisSource_NotNull_ExpectException_As_ExecutionEnvironmentError()
        {
            var key = "key";
            var sourceId = Guid.NewGuid();
            var exceptionMessage = "test: if the resource is not found";

            var env = new ExecutionEnvironment();
            env.Assign("[[key]]", key, 0);

            var redisSource = new RedisSource
            {
                HostName = "localhost",
                Password = "",
                Port = "1234",
            };

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(env);

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetResource<Resource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Throws(new Exception(exceptionMessage));

            var mockRedisCache = new Mock<IRedisCache>();
            mockRedisCache.Setup(o => o.Remove(key))
                .Returns(true);

            var mockRedisConnection = new Mock<IRedisConnection>();
            mockRedisConnection.Setup(o => o.Cache)
                .Returns(mockRedisCache.Object);

            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, new RedisCacheStub(() => mockRedisConnection.Object))
            {
                Key = key,
                SourceId = sourceId
            };

            redisRemoveActivity.Execute(mockDataObject.Object, 0);

            var builder = new StringBuilder();
            builder.Append(GlobalConstants.InnerErrorTag);
            builder.Append(exceptionMessage);
            builder.Append(GlobalConstants.InnerErrorTagEnd);

            Assert.AreEqual(builder.ToString(), env.FetchErrors());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_ExecuteTool_RedisSource_NotNull_ExpectSuccess()
        {
            var key = "key";
            var sourceId = Guid.NewGuid();
            var redisSource = new RedisSource
            {
                HostName = "localhost",
                Password = "",
                Port = "1234",
            };

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(new ExecutionEnvironment());

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetResource<Resource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(redisSource);

            var mockRedisCache = new Mock<IRedisCache>();
            mockRedisCache.Setup(o => o.Remove(key))
                .Returns(true);

            var mockRedisConnection = new Mock<IRedisConnection>();
            mockRedisConnection.Setup(o => o.Cache)
                .Returns(mockRedisCache.Object);

            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, new RedisCacheStub(() => mockRedisConnection.Object))
            {
                Key = key,
                SourceId = sourceId
            };

            redisRemoveActivity.Execute(mockDataObject.Object, 0);

            mockRedisCache.Verify(o => o.Remove(key), Times.Once);

            Assert.AreEqual("Success", redisRemoveActivity.Response);
        }

    }

    internal class RedisCacheStub : RedisCacheBase
    {
        public RedisCacheStub(Func<IRedisConnection> createConnection) : base(createConnection)
        {
        }
    }
}