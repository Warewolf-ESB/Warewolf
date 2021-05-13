﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Runtime.CompilerServices;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Data.ServiceModel;
using Dev2.Infrastructure.Tests;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.UnitTestAttributes;

namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    public class RedisSourcesTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisSources))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RedisSources_ConstructorWithNullResourceCatalogExpectedThrowsArgumentNullException() => new RedisSources(null);

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
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisSources))]
        public void RedisSources_Test_With_ValidHost_AuthenticationType_Anonymous_Expected_ValidValidationResult()
        {
            try
            {
            var dependency = new Depends(Depends.ContainerType.AnonymousRedis);
            var source = new RedisSource
            {
                HostName = dependency.Container.IP,
                AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.Anonymous,
                Port = dependency.Container.Port
            }.ToString();

                var handler = new RedisSources();
                var result = handler.Test(source);
                Assert.IsTrue(result.IsValid, result.ErrorMessage);
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
        [TestCategory(nameof(RedisSources))]
        public void RedisSources_Test_With_InvalidHost__AuthenticationType_Anonymous_Expected_InvalidValidationResult()
        {
            var source = new RedisSource
            {
                HostName = "ddd:222",
                AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.Anonymous,
                Port = "6379"
            }.ToString();
            try
            {
                var handler = new RedisSources();
                var result = handler.Test(source);
                Assert.IsFalse(result.IsValid);
                Assert.AreEqual("could not connect to redis Instance at ddd:222:6379\r\nNo such host is known", result.ErrorMessage);
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
        [TestCategory(nameof(RedisSources))]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void RedisSources_Test_With_ValidHost_AuthenticationType_Password_Expected_ValidValidationResult()
        {
            CustomContainer.Register(new Mock<IWarewolfPerformanceCounterLocater>().Object);
            CustomContainer.Register(new Mock<IResourceCatalog>().Object);
            var dependency = new Depends(Depends.ContainerType.Redis);
            var source = new RedisSource
            {
                HostName = dependency.Container.IP,
                Port = dependency.Container.Port,
                AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.Password,
                Password = "pass123"
            }.ToString();

            try
            {
                var handler = new RedisSources();
                var result = handler.Test(source);
                Assert.IsTrue(result.IsValid, result.ErrorMessage);
            }
            catch (Exception e)
            {
                Assert.IsFalse(e.Message.Contains("could not connect to redis Instance"), e.Message);
            }
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisSources))]
        public void RedisSources_Test_With_InvalidHost_AuthenticationType_Password_Expected_InvalidValidationResult()
        {
            var source = new RedisSource
            {
                HostName = "ddd",
                Port = "6380",
                AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.Password,
                Password = "Password"
            }.ToString();

            var handler = new RedisSources();
            var result = handler.Test(source);
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("could not connect to redis Instance at ddd:6380\r\nNo such host is known", result.ErrorMessage);
        }
    }
}
