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
using System.Text;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Infrastructure.Tests;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Serialization;
using Warewolf.UnitTestAttributes;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class TestElasticsearchSourceTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestElasticsearchSource))]
        public void TestElasticsearchSource_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var testElasticsearchSource = new TestElasticsearchSource();
            //------------Execute Test---------------------------
            var resId = testElasticsearchSource.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestElasticsearchSource))]
        public void testElasticsearchSource_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var testElasticsearchSource = new TestElasticsearchSource();
            //------------Execute Test---------------------------
            var resId = testElasticsearchSource.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestElasticsearchSource))]
        public void TestElasticsearchSource_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var testElasticsearchSource = new TestElasticsearchSource();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual("TestElasticsearchSource", testElasticsearchSource.HandlesType());
        }
        
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestElasticsearchSource))]
        public void TestElasticsearchSource_CreateServiceEntry_ExpectActions()
        {
            //------------Setup for test--------------------------
            var testElasticsearchSource = new TestElasticsearchSource();
            //------------Execute Test---------------------------
            var dynamicService = testElasticsearchSource.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsNotNull(dynamicService.Actions);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestElasticsearchSource))]
        public void TestElasticsearchSource_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var testElasticsearchSource = new TestElasticsearchSource();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = testElasticsearchSource.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestElasticsearchSource))]
        public void TestElasticsearchSource_Execute_ResourceIDNotPresent_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> {{"item", new StringBuilder()}};
            var testElasticsearchSource = new TestElasticsearchSource();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = testElasticsearchSource.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestElasticsearchSource))]
        [Depends(Depends.ContainerType.Elasticsearch)]
        public void TestElasticsearchSource_Execute_GivenResourceDefinition_ShouldTestNewSourceReturnResourceDefinitionMsg()
        {
            //---------------Set up test pack-------------------
            var serializer = new Dev2JsonSerializer();
            var dependency = new Depends(Depends.ContainerType.Elasticsearch);
            var source = new ElasticsearchSourceDefinition()
            {
                Id = Guid.Empty,
                Name = "Name",
                HostName = dependency.Container.IP,
                Port = dependency.Container.Port,
                AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.Anonymous
            };
            var testElasticsearchSource = new TestElasticsearchSource();
            var values = new Dictionary<string, StringBuilder>
            {
                {"ElasticsearchSource", source.SerializeToJsonStringBuilder()}
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                var jsonResult = testElasticsearchSource.Execute(values, null);
                var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
                //---------------Test Result -----------------------
                Assert.IsFalse(result.HasError, result.Message.ToString());
            }
            catch (Exception e)
            {
                if (e.Message.Contains("could not connect to elasticsearch Instance"))
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