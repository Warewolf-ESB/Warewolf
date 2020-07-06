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
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.RedisCache;
using Dev2.Activities.RedisRemove;
using Dev2.Common;
using Dev2.Common.State;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Interfaces;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using Warewolf.UnitTestAttributes;

namespace Dev2.Tests.Activities.ActivityTests.Redis
{
    [TestClass]
    public class RedisRemoveActivityNewTests : BaseActivityTests
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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_ExecuteTool_RedisSource_Null()
        {
            RedisSource redisSource = null;
            var sourceId = Guid.NewGuid();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            mockExecutionEnvironment.Setup(o => o.HasErrors()).Returns(true);
            var expectedErrorMsg = ErrorResource.RedisSourceHasBeenRemoved;
            mockExecutionEnvironment.Setup(o => o.AllErrors).Returns(new HashSet<string> {expectedErrorMsg});
            mockExecutionEnvironment.Setup(o => o.Errors).Returns(new HashSet<string> {expectedErrorMsg});

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment).Returns(mockExecutionEnvironment.Object);
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetResource<RedisSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(redisSource);

            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, null)
            {
                SourceId = sourceId,
            };

            redisRemoveActivity.Execute(mockDataObject.Object, 0);

            mockExecutionEnvironment.Verify(o => o.HasErrors(), Times.Once);
            mockExecutionEnvironment.Verify(o => o.AllErrors, Times.Exactly(2));
            mockExecutionEnvironment.Verify(o => o.Errors, Times.Exactly(2));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveActivity))]
        public void RedisRemoveActivity_ExecuteTool_RedisSource_NotNull()
        {
            var dependency = new Depends(Depends.ContainerType.AnonymousRedis);
            var redisSource = new RedisSource
            {
                HostName = dependency.Container.IP,
                Password = "",
                Port = dependency.Container.Port,
            };
            var sourceId = Guid.NewGuid();
            const string expression = "qwerty";
            var evalResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing);
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            mockExecutionEnvironment.Setup(o => o.EvalToExpression(It.IsAny<string>(), 0)).Returns(expression);
            mockExecutionEnvironment.Setup(o => o.Eval(It.IsAny<string>(), 0, false, true)).Returns(evalResult);

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment).Returns(mockExecutionEnvironment.Object);
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetResource<RedisSource>(GlobalConstants.ServerWorkspaceID, sourceId))
                .Returns(redisSource);

            var mockRedisConnection = new Mock<IRedisConnection>();
            var redisRemoveActivity = new RedisRemoveActivity(mockResourceCatalog.Object, null)
            {
                SourceId = sourceId,
                Connection = mockRedisConnection.Object
            };

            redisRemoveActivity.Execute(mockDataObject.Object, 0);

            mockExecutionEnvironment.Verify(o => o.HasErrors(), Times.Once);
            //TODO: Expand on this test once IRedisConnection is in use for RedisRemoveActivity
            //mockExecutionEnvironment.Verify(o => o.AllErrors, Times.Exactly(2));
            //mockExecutionEnvironment.Verify(o => o.Errors, Times.Exactly(2));
        }
    }
}