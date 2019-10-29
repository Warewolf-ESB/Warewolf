/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Driver.Redis;
using Warewolf.Interfaces;

namespace Dev2.Tests.Activities.Activities.Redis
{
    [TestClass]
    public class RedisCacheTests
    {
        [TestMethod]      
        [TestCategory("RedisCache")]
        [Owner("Hagashen Naidu")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RedisCache_Constructor_NullFunction_ShouldThrowException()
        {
            //--------------Arrange------------------------------
            //--------------Act----------------------------------
            new RedisCacheStub(null);
            //--------------Assert-------------------------------
        }

        [TestMethod]
        [TestCategory("RedisCache")]
        [Owner("Hagashen Naidu")]
        public void RedisCache_Set_StringValue_ShouldCallStringSetOnCache()
        {
            //--------------Arrange------------------------------
            var mockConnection = new Mock<IRedisConnection>();
            var mockDatabase = new Mock<IRedisCache>();
            mockDatabase.Setup(db => db.StringSet(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
            var redis = new RedisCacheStub(()=> mockConnection.Object);
            //--------------Act----------------------------------
            redis.Set<string>("bob", "the builder");
            //--------------Assert-------------------------------
            mockDatabase.Verify(db => db.StringSet(It.IsAny<string>(), It.IsAny<string>()),Times.Once);

        }

        [TestMethod]
        [TestCategory("RedisCache")]
        [Owner("Hagashen Naidu")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RedisCache_Set_NullValue_ShouldThrowArgumentNullException()
        {
            //--------------Arrange------------------------------
            var mockConnection = new Mock<IRedisConnection>();
            var mockDatabase = new Mock<IRedisCache>();
            mockDatabase.Setup(db => db.StringSet(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
            var redis = new RedisCacheStub(() => mockConnection.Object);
            //--------------Act----------------------------------
            redis.Set<string>("bob", null);
            //--------------Assert-------------------------------

        }

        [TestMethod]
        [TestCategory("RedisCache")]
        [Owner("Hagashen Naidu")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RedisCache_Set_NotStringValue_ShouldThrowInvalidOperationException()
        {
            //--------------Arrange------------------------------
            var mockConnection = new Mock<IRedisConnection>();
            var mockDatabase = new Mock<IRedisCache>();
            mockDatabase.Setup(db => db.StringSet(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
            var redis = new RedisCacheStub(() => mockConnection.Object);
            //--------------Act----------------------------------
            redis.Set<object>("bob", new object());
            //--------------Assert-------------------------------

        }

        [TestMethod]
        [TestCategory("RedisCache")]
        [Owner("Hagashen Naidu")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RedisCache_Set_NullKey_ShouldThrowArgumentNullException()
        {
            //--------------Arrange------------------------------
            var mockConnection = new Mock<IRedisConnection>();
            var mockDatabase = new Mock<IRedisCache>();
            mockDatabase.Setup(db => db.StringSet(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
            var redis = new RedisCacheStub(() => mockConnection.Object);
            //--------------Act----------------------------------
            redis.Set<object>(null, new object());
            //--------------Assert-------------------------------

        }

        [TestMethod]
        [TestCategory("RedisCache")]
        [Owner("Hagashen Naidu")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RedisCache_Get_NullKey_ShouldThrowArgumentNullException()
        {
            //--------------Arrange------------------------------
            var ext = System.Threading.Tasks.TaskExtensions.Unwrap(new System.Threading.Tasks.Task<System.Threading.Tasks.Task>(() => System.Threading.Tasks.Task.FromResult(true)));
            var mockConnection = new Mock<IRedisConnection>();
            var mockDatabase = new Mock<IRedisCache>();
            mockDatabase.Setup(db => db.StringGet(It.IsAny<string>())).Verifiable();
            mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
            var redis = new RedisCacheStub(() => mockConnection.Object);
            //--------------Act----------------------------------
            redis.Get(null);
            //--------------Assert-------------------------------

        }

        [TestMethod]
        [TestCategory("RedisCache")]
        [Owner("Hagashen Naidu")]
        public void RedisCache_Get_ValidKey_ShouldReturn()
        {
            //--------------Arrange------------------------------
            var mockConnection = new Mock<IRedisConnection>();
            var mockDatabase = new Mock<IRedisCache>();
            mockDatabase.Setup(db => db.StringGet("bob")).Verifiable();
            mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
            var redis = new RedisCacheStub(() => mockConnection.Object);
            //--------------Act----------------------------------
            redis.Get("bob");
            //--------------Assert-------------------------------
            mockDatabase.Verify(db => db.StringGet("bob"), Times.Once);
        }
    }

    internal class RedisCacheStub : RedisCacheBase
    {
        public RedisCacheStub(Func<IRedisConnection> createConnection) : base(createConnection)
        {
        }
    }
}
