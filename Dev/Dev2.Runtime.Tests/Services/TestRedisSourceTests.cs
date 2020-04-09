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
using System.Collections.Generic;
using System.Text;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UnitTestAttributes;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class TestRedisSourceTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestRedisSource))]
        public void TestRedisSource_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var testRedisSource = new TestRedisSource();
            //------------Execute Test---------------------------
            var resId = testRedisSource.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestRedisSource))]
        public void TestRedisSource_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var testRedisSource = new TestRedisSource();
            //------------Execute Test---------------------------
            var resId = testRedisSource.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestRedisSource))]
        public void TestRedisSource_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var testRedisSource = new TestRedisSource();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual("TestRedisSource", testRedisSource.HandlesType());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestRedisSource))]
        public void TestRedisSource_CreateServiceEntry_ExpectActions()
        {
            //------------Setup for test--------------------------
            var testRedisSource = new TestRedisSource();
            //------------Execute Test---------------------------
            var dynamicService = testRedisSource.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsNotNull(dynamicService.Actions);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestRedisSource))]
        public void TestRedisSource_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var testRedisSource = new TestRedisSource();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = testRedisSource.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestRedisSource))]
        public void TestRedisSource_Execute_ResourceIDNotPresent_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "item", new StringBuilder() } };
            var testRedisSource = new TestRedisSource();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = testRedisSource.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestRedisSource))]
        public void TestRedisSource_Execute_GivenResourceDefinition_ShouldTestNewSourceReturnResourceDefinitionMsg()
        {
            //---------------Set up test pack-------------------
            var dependency = new Depends(Depends.ContainerType.AnonymousRedis);
            var serializer = new Dev2JsonSerializer();
            var source = new RedisSourceDefinition()
            {
                Id = Guid.Empty,
                Name = "Name",
                HostName = dependency.Container.IP,
                Port = dependency.Container.Port,
                AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.Anonymous
            };
            var testRedisSource = new TestRedisSource();
            var values = new Dictionary<string, StringBuilder>
            {
                { "RedisSource", source.SerializeToJsonStringBuilder() }
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                var jsonResult = testRedisSource.Execute(values, null);
                var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
                //---------------Test Result -----------------------
                Assert.IsFalse(result.HasError, result.Message.ToString());
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
    }
}
