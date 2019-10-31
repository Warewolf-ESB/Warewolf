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
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    public class RedisSourcesTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisSources))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RedisSources_ConstructorWithNullResourceCatalogExpectedThrowsArgumentNullException()
        {
            var handler = new RedisSources(null);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisSources))]
        public void RedisSources_TestWithValidArgs_Expected_Valid_ValidationResult()
        {
            var handler = new RedisSources();
            var redisSource = new RedisSource();
            var result = handler.Test(redisSource);
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisSources))]
        public void RedisSources_Test_With_InValidArgs_Expected_Valid_InValidationResult()
        {
            var handler = new RedisSources();      
         
            var result = handler.Test("asd");
            Assert.IsFalse(result.IsValid);
        }
        [TestMethod]
        public void RedisSources_Test_With_ValidHost_Expected_ValidValidationResult()
        {
            var source = new RedisSource { HostName = "localhost:222", Port = "6379" }.ToString();

            var handler = new RedisSources();
            var result = handler.Test(source);
            Assert.IsTrue(result.IsValid, result.ErrorMessage);
        }

    }
}
