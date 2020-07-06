/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Threading;
using Dev2.Activities.RedisCache;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

    }

}