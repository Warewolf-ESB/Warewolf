/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using Dev2.Activities.RedisCache;
using Dev2.Common;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Serializers;
using Dev2.Common.State;
using Dev2.Data.ServiceModel;
using Dev2.Diagnostics;
using Dev2.Factories;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Driver.Redis;
using Warewolf.Interfaces;
using Warewolf.Storage;
using Warewolf.UnitTestAttributes;

namespace Dev2.Tests.Activities.ActivityTests.Redis
{
    [TestClass]
    public class RedisCacheActivityTests : BaseActivityTests
    {
        int TTL { get; set; } = 5;
        private TimeSpan CacheTTL => TimeSpan.FromSeconds(TTL);
        static RedisCacheActivity CreateRedisActivity()
        {
            return new RedisCacheActivity();
        }

        static TestRedisActivity CreateTestRedisActivity(IResourceCatalog resourceCatalog, ResponseManager responseManager, IRedisCache redisCache)
        {
            return new TestRedisActivity(resourceCatalog,responseManager,redisCache);
        }


        private static RedisSource TestRedisSource(out string hostName, out string password, out int port)
        {
            var dependency = new Depends(Depends.ContainerType.Redis, true);
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

        [TestMethod]
        [Timeout(60000)]
        [Owner("Yogesh Rajpurohit")]
        [TestCategory(nameof(RedisCacheActivity))]
        public void RedisCacheActivity_PerformExecution_RedisKeyFound()
        {
            var redisSource = TestRedisSource(out var hostName, out var password, out var port);          
            
             
            var key = "111";
            
            var mockDev2Activity = new Mock<IDev2Activity>();
            mockDev2Activity.Setup(o => o.GetOutputs()).Returns(new List<string>());

            var innerActivity = new DsfMultiAssignActivity()
            {
                FieldsCollection = new List<ActivityDTO>
                    {
                        new ActivityDTO("[[customer(1).id]]", "null", 1),
                        new ActivityDTO("[[customer(2).id]]", "null", 2),
                        new ActivityDTO("[[customer(3).id]]", "null", 3),
                        new ActivityDTO("[[customer(1).name]]", "null", 1),
                        new ActivityDTO("[[customer(2).name]]", "null", 2),
                        new ActivityDTO("[[customer(3).name]]", "null", 3),
                    }
            };

            GenerateMocks(key, redisSource, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataObject);
            CreateRedisActivity(key, hostName, port, password, mockResourceCatalog, out TestRedisActivity sut, innerActivity);

            sut.TestExecuteTool(mockDataObject.Object);
            var debugOutputTest = sut.TestPerformExecution(null);
            var debugOutputs = sut.GetDebugOutputs(mockDataObject.Object.Environment, 0);

            //----------------------Assert-----------------------
            Assert.AreEqual(7, debugOutputs.Count);
            Assert.AreEqual("Redis key { " + sut.Key + " } not found", debugOutputs[0].ResultsList[0].Label);           
            Assert.AreEqual(debugOutputTest[0], "Success");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Yogesh Rajpurohit")]
        [TestCategory(nameof(RedisCacheActivity))]
        public void RedisCacheActivity_PerformExecution_RedisKeyNotFound()
        {
            var redisSource = TestRedisSource(out var hostName, out var password, out var port);
            var mockResponseManager = new Mock<ResponseManager>();
            var mockRedisCacheBase = new Mock<RedisCacheBase>();
            var key = "3";

            var mockDev2Activity = new Mock<IDev2Activity>();
            mockDev2Activity.Setup(o => o.GetOutputs()).Returns(new List<string>());

            var innerActivity = new DsfMultiAssignActivity()
            {
                FieldsCollection = new List<ActivityDTO>
                    {
                        new ActivityDTO("[[customer(1).id]]", "1", 1),
                        new ActivityDTO("[[customer(2).id]]", "2", 2),
                        new ActivityDTO("[[customer(3).id]]", "3", 3),
                        new ActivityDTO("[[customer(1).name]]", "name1", 1),
                        new ActivityDTO("[[customer(2).name]]", "name2", 2),
                        new ActivityDTO("[[customer(3).name]]", "name3", 3),
                    }
            };

            GenerateMocks(key, redisSource, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataObject);
            CreateRedisActivity(key, hostName, port, password, mockResourceCatalog, out TestRedisActivity sut, innerActivity);

            sut.TestExecuteTool(mockDataObject.Object);           
            var debugOutputs = sut.GetDebugOutputs(mockDataObject.Object.Environment, 0);

            //----------------------Assert-----------------------
            Assert.AreEqual(7, debugOutputs.Count);
            Assert.AreEqual("Redis key { " + sut.Key + " } not found", debugOutputs[0].ResultsList[0].Label);
             
        }

        [TestMethod]
        //[Timeout(60000)]
        [Owner("Yogesh Rajpurohit")]
        [TestCategory(nameof(RedisCacheActivity))]
        public void RedisCacheActivity_PerformExecution_Result_RedisKey()
        {

            var redisSource = new RedisSource
            {
                HostName = "localhost",
                Password = "",
                Port = "6379",
            };
             
            var key = "111";

            var mockResponseManager = new Mock<ResponseManager>();
           
            var mockRedisCache = new Mock<IRedisCache>();

            var innerActivity = new DsfMultiAssignActivity()
            {
                FieldsCollection = new List<ActivityDTO>
                    {
                        new ActivityDTO("[[customer(1).id]]", "null", 1),
                        new ActivityDTO("[[customer(2).id]]", "null", 2),
                        new ActivityDTO("[[customer(3).id]]", "null", 3),
                        new ActivityDTO("[[customer(1).name]]", "null", 1),
                        new ActivityDTO("[[customer(2).name]]", "null", 2),
                        new ActivityDTO("[[customer(3).name]]", "null", 3),
                    }
            };

            //ISerializer _serializer = new Dev2JsonSerializer();          
            //mockRedisCache.Setup(o=>o.Set(key, _serializer.Serialize(innerActivity), CacheTTL));

            var innerAct = new TestActivity(); 

            
            GenerateMocks(key, redisSource, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataObject);
             
            var sourceId = Guid.NewGuid();
            mockResourceCatalog.Setup(o => o.GetResource<RedisSource>(It.IsAny<Guid>(), sourceId)).Returns(redisSource as RedisSource);
           


            var sut = CreateTestRedisActivity(mockResourceCatalog.Object, mockResponseManager.Object, mockRedisCache.Object);
           
            sut.ActivityFunc = new ActivityFunc<string, bool>
            {
                Handler = innerAct
            };
            sut.Key = key;
            sut.TestExecuteTool(mockDataObject.Object);
            var debugOutputTest = sut.TestPerformExecution(null);
            var debugOutputs = sut.GetDebugOutputs(mockDataObject.Object.Environment, 0);
            Assert.AreEqual(7, debugOutputs.Count); 
        }


        class TestActivity : DsfActivityAbstract<string>
        {
            public TestActivity()
            {
               
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
                return new List<string> { "this is a lie" };
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


        private static void GenerateMocks(string key, RedisSource redisSource, out Mock<IResourceCatalog> mockResourceCatalog, out Mock<IDSFDataObject> mockDataObject)
        {
            mockResourceCatalog = new Mock<IResourceCatalog>();
            mockDataObject = new Mock<IDSFDataObject>();
            var environment = new ExecutionEnvironment();
            environment.Assign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>());
            environment.EvalToExpression(key, 0);
            //mockResourceCatalog.Setup(o => o.GetResource<RedisSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(redisSource);
            mockResourceCatalog.Setup(o => o.GetResource<RedisSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(redisSource);
            mockDataObject.Setup(o => o.IsDebugMode()).Returns(true);
            mockDataObject.Setup(o => o.Environment).Returns(environment);
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

    }

    class TestRedisActivity : RedisCacheActivity
    {
        public TestRedisActivity(IResourceCatalog resourceCatalog, RedisCacheImpl impl)
            : base(resourceCatalog, impl)
        {
        }

        public TestRedisActivity(IResourceCatalog resourceCatalog, ResponseManager responseManager, IRedisCache redisCache)
           : base(resourceCatalog, responseManager, redisCache)
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