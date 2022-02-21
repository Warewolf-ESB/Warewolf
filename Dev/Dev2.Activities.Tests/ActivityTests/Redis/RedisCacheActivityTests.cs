/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2022 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using Dev2.Activities.RedisCache;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.State;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Driver.Redis;
using Warewolf.Interfaces;
using Warewolf.Storage;
using WarewolfParserInterop;

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
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RedisCacheActivity))]
        public void RedisCacheActivity_GetChildrenNodes_ShouldReturnChildNodes()
        {
            //---------------Set up test pack-------------------
            var redisActivity = CreateRedisActivity();
            redisActivity.ActivityFunc = new System.Activities.ActivityFunc<string, bool>
            {
                Handler = new DsfFlowDecisionActivity { }
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugInputs = redisActivity.GetChildrenNodes();
            //---------------Test Result -----------------------
            Assert.AreEqual(1, debugInputs.ToList().Count);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RedisCacheActivity))]
        public void RedisCacheActivity_PerformExecution_RedisKeyFoundButObjectIsEmpty()
        {
            var key = "test_24";
            var fakeKey = "this is a lie";
            var fadeData = string.Empty; //this scenario was not handled, hence the bug
            var sourceId = Guid.NewGuid();

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetResource<RedisSource>(It.IsAny<Guid>(), sourceId))
                .Returns(new RedisSource
                {
                    ResourceID = sourceId,
                    HostName = "local",
                    Password = "pass3142",
                    Port = "4231"
                });

            var mockRedisCache = new Mock<IRedisCache>();
            mockRedisCache.Setup(o => o.Get(key))
                .Returns(fadeData);
            mockRedisCache.Setup(o => o.Remove(key))
                .Returns(true);

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(new ExecutionEnvironment());

            var serializer = new Mock<ISerializer>();
            serializer.Setup(o => o.Deserialize<IDictionary<string, string>>(fadeData))
                .Returns(new Dictionary<string, string> { { fakeKey, fadeData } });

            var innerAct = new TestActivity("this will be empty");

            var sut = new TestRedisActivity(mockResourceCatalog.Object, new ResponseManager(), mockRedisCache.Object, serializer.Object)
            {
                SourceId = sourceId,
                Key = key,
                ActivityFunc =
                {
                    Handler = innerAct
                }
            };

            sut.TestExecuteTool(mockDataObject.Object);
            var debugOutputs = sut.GetDebugOutputs(mockDataObject.Object.Environment, 0);

            //----------------------Assert-----------------------
            Assert.AreEqual(1, debugOutputs.Count);
            Assert.AreEqual("Redis key { " + sut.Key + " } found but object is empty", debugOutputs[0].ResultsList[0].Label);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RedisCacheActivity))]
        public void RedisCacheActivity_PerformExecution_RedisKeyFound()
        {
            var key = "test_24";
            var fakeKey = "this is a lie";
            var fakeData = "this is a test";
            var sourceId = Guid.NewGuid();

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetResource<RedisSource>(It.IsAny<Guid>(), sourceId))
                .Returns(new RedisSource
                {
                    ResourceID = sourceId,
                    HostName = "local",
                    Password = "pass3142",
                    Port = "4231"
                });

            var mockRedisCache = new Mock<IRedisCache>();
            mockRedisCache.Setup(o => o.Get(key))
                .Returns(fakeData);
            mockRedisCache.Setup(o => o.Remove(key))
                .Returns(true);

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment)
                .Returns(new ExecutionEnvironment());

            var serializer = new Mock<ISerializer>();
            serializer.Setup(o => o.Deserialize<IDictionary<string, string>>(fakeData))
                .Returns(new Dictionary<string, string> { { fakeKey, fakeData } });
            serializer.Setup(o => o.Deserialize<List<AssignValue>>(fakeData))
                .Returns(new List<AssignValue>());

            var innerAct = new TestActivity(fakeKey);

            var sut = new TestRedisActivity(mockResourceCatalog.Object, new ResponseManager(), mockRedisCache.Object, serializer.Object)
            {
                SourceId = sourceId,
                Key = key,
                ActivityFunc =
                {
                    Handler = innerAct
                }
            };

            sut.TestExecuteTool(mockDataObject.Object);
            var debugOutputs = sut.GetDebugOutputs(mockDataObject.Object.Environment, 0);

            //----------------------Assert-----------------------
            Assert.AreEqual(1, debugOutputs.Count);
            Assert.AreEqual("Redis key { " + sut.Key + " } found", debugOutputs[0].ResultsList[0].Label);
        }

    }

    class TestActivity : DsfActivityAbstract<string>
    {
        private readonly string _outputs;

        public TestActivity(string outputs)
        {
            _outputs = outputs;
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            throw new NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            throw new NotImplementedException();
        }

        public override List<string> GetOutputs()
        {
            return new List<string> { _outputs };
        }

        public override IEnumerable<StateVariable> GetState()
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            throw new NotImplementedException();
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                return hashCode;
            }
        }
    }


    class TestRedisActivity : RedisCacheActivity
    {
        public TestRedisActivity(IResourceCatalog resourceCatalog, RedisCacheImpl impl)
            : base(resourceCatalog, impl)
        {
        }

        public TestRedisActivity(IResourceCatalog resourceCatalog, ResponseManager responseManager, IRedisCache redisCache, ISerializer serializer)
           : base(resourceCatalog, responseManager, redisCache, serializer)
        {
        }

        public void TestExecuteTool(IDSFDataObject dataObject)
        {
            base.ExecuteTool(dataObject, 1);
        }

        public List<string> TestPerformExecution(Dictionary<string, string> evaluatedValues)
        {
            return base.PerformExecution(evaluatedValues);
        }
    }
}